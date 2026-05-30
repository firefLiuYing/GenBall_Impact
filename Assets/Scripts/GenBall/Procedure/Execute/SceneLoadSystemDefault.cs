using GenBall.Event;
using GenBall.Map;
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
        private SavePointModel _targetSavePoint;
        private float _loadStartTime;

        public float LoadProgress => _loadProgress;
        public bool IsLoading => _isLoading;
        public string TargetSceneName => _targetSceneName;
        public SavePointModel TargetSavePoint => _targetSavePoint;

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
            _targetSavePoint = null;
            _loadStartTime = 0f;
        }

        public void SetTargetSavePoint(SavePointModel savePoint)
        {
            _targetSavePoint = savePoint;
        }

        public void AsyncLoadScene(string sceneName)
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

            Debug.Log($"[SceneLoadSystem] Starting async load of scene: {sceneName}");

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // SceneManager.LoadSceneAsync throws in EditMode. Set state for test verification only.
                Debug.Log($"[SceneLoadSystem] EditMode: state set for scene '{sceneName}' (load skipped)");
                return;
            }
#endif

            _asyncOp = SceneManager.LoadSceneAsync(sceneName);
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

                // Trigger scene setup (e.g. spawn player, enemies, HUD)
                // Specific initialization logic is TBD — will be revised after design discussion
                SystemRepository.Instance.GetSystem<ISceneExecutorSystem>().ExecuteSceneSetup();

                CEventRouter.Instance.FireNow((int)GlobalEventId.LoadingComplete);
            }
        }
    }
}
