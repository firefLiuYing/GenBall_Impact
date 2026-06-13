using GenBall.Event;
using GenBall.Procedure.Execute;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.Map
{
    public class TeleportSystem : ITeleportSystem
    {
        private SavePointModel _cachedSavePoint;
        private bool _loadingCompleteSubscribed;

        public bool IsTeleporting { get; private set; }

        public void Init() { }

        public void UnInit()
        {
            if (_loadingCompleteSubscribed)
            {
                CEventRouter.Instance.Unsubscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);
                _loadingCompleteSubscribed = false;
            }
            _cachedSavePoint = null;
            IsTeleporting = false;
        }

        public bool Teleport(TeleportRequestInfo request)
        {
            if (IsTeleporting)
            {
                Debug.LogWarning("[TeleportSystem] Already teleporting, ignoring request.");
                return false;
            }

            if (string.IsNullOrEmpty(request.SceneName))
            {
                Debug.LogError("[TeleportSystem] SceneName is null or empty.");
                return false;
            }

            var sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            var savePointModel = sceneSystem.GetSavePointModel(request.SceneName, request.SavePointIndex);
            if (savePointModel == null)
            {
                Debug.LogError($"[TeleportSystem] SavePoint not found: scene={request.SceneName}, index={request.SavePointIndex}");
                return false;
            }

            IsTeleporting = true;
            _cachedSavePoint = savePointModel;

            var currentSceneName = SceneManager.GetActiveScene().name;
            if (request.SceneName == currentSceneName)
            {
                // Same scene: skip loading, go directly to initialization
                Debug.Log($"[TeleportSystem] Same-scene teleport to {request.SceneName}, savePoint={request.SavePointIndex}");
                ExecuteInit();
            }
            else
            {
                // Cross scene: load first, then initialize
                Debug.Log($"[TeleportSystem] Cross-scene teleport: {currentSceneName} → {request.SceneName}");
                SubscribeToLoadingComplete();
                var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
                loadSystem.LoadScene(request.SceneName);
            }

            return true;
        }

        private void ExecuteInit()
        {
            var context = new SceneInitContext
            {
                SpawnPosition = _cachedSavePoint.spawnPosition,
                SpawnRotation = _cachedSavePoint.spawnRotation,
            };

            var executor = SystemRepository.Instance.GetSystem<ISceneExecutorSystem>();
            executor.ExecuteSceneSetup(context);

            IsTeleporting = false;
            _cachedSavePoint = null;
        }

        private void SubscribeToLoadingComplete()
        {
            if (_loadingCompleteSubscribed) return;
            CEventRouter.Instance.Subscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);
            _loadingCompleteSubscribed = true;
        }

        private void OnLoadingComplete()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);
            _loadingCompleteSubscribed = false;

            ExecuteInit();
        }
    }
}
