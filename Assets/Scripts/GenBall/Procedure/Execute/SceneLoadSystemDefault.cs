using GenBall.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class SceneLoadSystemDefault : ISceneLoadSystem, IFrameUpdate
    {
        private const float LoadTimeout = 30f;

        private AsyncOperation _asyncOp;
        private float _loadProgress;
        private bool _isLoading;
        private string _targetSceneName;
        private float _loadStartTime;

        public float LoadProgress => _loadProgress;
        public bool IsLoading => _isLoading;
        public string TargetSceneName => _targetSceneName;

        public SystemScope FrameUpdateScope => SystemScope.Framework;

        public void Init()
        {
        }

        public void UnInit()
        {
            _asyncOp = null;
            _loadProgress = 0f;
            _isLoading = false;
            _targetSceneName = null;
            _loadStartTime = 0f;
        }

        public void LoadScene(string sceneName, SceneLoadMode mode = SceneLoadMode.Single)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoadSystem] Already loading scene \"{_targetSceneName}\", ignoring request for \"{sceneName}\"");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoadSystem] Cannot load scene: sceneName is null or empty.");
                return;
            }

            _isLoading = true;
            _targetSceneName = sceneName;
            _loadProgress = 0f;
            _loadStartTime = Time.unscaledTime;

            Debug.Log($"[SceneLoadSystem] Starting async load of scene: {sceneName} (mode={mode})");

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log($"[SceneLoadSystem] EditMode: state set for scene '{sceneName}' (load skipped)");
                return;
            }
#endif

            var unityMode = mode == SceneLoadMode.Additive
                ? LoadSceneMode.Additive
                : LoadSceneMode.Single;
            _asyncOp = SceneManager.LoadSceneAsync(sceneName, unityMode);
            _asyncOp.allowSceneActivation = true;
        }

        public void FrameUpdate(float deltaTime)
        {
            if (!_isLoading || _asyncOp == null)
                return;

            // Check for timeout
            if (Time.unscaledTime - _loadStartTime > LoadTimeout)
            {
                Debug.LogError($"[SceneLoadSystem] Scene load timed out for \"{_targetSceneName}\" after {LoadTimeout} seconds.");
                CEventRouter.Instance.FireNow((int)GlobalEventId.LoadingComplete);
                return;
            }

            // Report normalized progress (0 → 0.9 → completed)
            float normalizedProgress = Mathf.Clamp01(_asyncOp.progress / 0.9f);
            _loadProgress = normalizedProgress;
            CEventRouter.Instance.FireNow((int)GlobalEventId.LoadingProgress, normalizedProgress);

            // Check for completion
            if (_asyncOp.isDone)
            {
                _isLoading = false;
                _loadProgress = 1f;
                Debug.Log($"[SceneLoadSystem] Scene load complete: {_targetSceneName}");

                CEventRouter.Instance.FireNow((int)GlobalEventId.LoadingComplete);
            }
        }
    }
}
