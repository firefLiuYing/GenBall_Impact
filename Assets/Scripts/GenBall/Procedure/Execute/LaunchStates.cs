using GenBall.Event;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;
using UnityEngine;

namespace GenBall.Procedure.Execute
{
    /// <summary>
    /// 启动加载阶段。通过 CEventRouter 发射全局事件通知 UI 层。
    /// Procedure 层不引用任何 UI 类型。
    /// </summary>
    public class StartupLoadingState : SimpleFsmState<ILaunchSystem>
    {
        public override void OnEnter(ILaunchSystem context)
        {
            CEventRouter.Instance.FireNow((int)GlobalEventId.StartupLoadingBegin);
        }
    }

    /// <summary>
    /// 主菜单阶段。通过 CEventRouter 发射全局事件通知 UI 层。
    /// </summary>
    public class StartFormState : SimpleFsmState<ILaunchSystem>
    {
        public override void OnEnter(ILaunchSystem context)
        {
            CEventRouter.Instance.FireNow((int)GlobalEventId.StartFormBegin);
        }
    }

    /// <summary>
    /// 场景加载阶段。编排：初始化场景状态 → 加载场景 → 等待完成 → 执行场景初始化。
    /// </summary>
    public class LoadSceneState : SimpleFsmState<ILaunchSystem>
    {
        public override void OnEnter(ILaunchSystem context)
        {
            var launchSystem = context as LaunchSystemDefault;
            var startContext = launchSystem?.PendingGameStartContext;
            if (startContext == null)
            {
                Debug.LogError("[LoadSceneState] No pending GameStartContext. Cannot proceed.");
                return;
            }

            // 1. Fire GameLaunch event — tells UI to close StartForm and re-open LoadingForm as loading screen
            CEventRouter.Instance.FireNow((int)GlobalEventId.GameLaunch);

            // 2. Hide and lock cursor for gameplay
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 3. Initialize scene state and map config
            var gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            var sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();

            var mapProvider = gameManager.GetProvider("Map") as MapSaveDataProvider;
            var mapSaveData = mapProvider?.RuntimeData ?? new MapSaveData();
            sceneSystem.InitializeSceneStateObjs(mapSaveData);

#if UNITY_EDITOR
            sceneSystem.InitializeMapConfig(ConfigProvider.GetOrCreateMapConfig());
#else
            sceneSystem.InitializeMapConfig(new MapModel());
#endif

            // 4. Look up save point for spawn position
            var savePoint = sceneSystem.GetSavePointModel(startContext.TargetSceneName, startContext.TargetSavePointIndex);
            var spawnPosition = savePoint?.spawnPosition ?? Vector3.zero;
            var spawnRotation = savePoint?.spawnRotation ?? Quaternion.identity;

            // 5. Subscribe to LoadingComplete to chain into scene initialization
            CEventRouter.Instance.Subscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);

            // 6. Begin async scene loading
            var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
            loadSystem.LoadScene(startContext.TargetSceneName);
        }

        private void OnLoadingComplete()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);

            var launchSystem = SystemRepository.Instance.GetSystem<ILaunchSystem>() as LaunchSystemDefault;
            var startContext = launchSystem?.PendingGameStartContext;
            if (startContext == null) return;

            var sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            var savePoint = sceneSystem.GetSavePointModel(startContext.TargetSceneName, startContext.TargetSavePointIndex);

            var initContext = new SceneInitContext
            {
                SpawnPosition = savePoint?.spawnPosition ?? Vector3.zero,
                SpawnRotation = savePoint?.spawnRotation ?? Quaternion.identity,
            };

            var executor = SystemRepository.Instance.GetSystem<ISceneExecutorSystem>();
            executor.ExecuteSceneSetup(initContext);
        }
    }
}
