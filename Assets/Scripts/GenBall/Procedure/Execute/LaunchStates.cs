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
    /// Splash / Loading 阶段。通过 CEventRouter 发射全局事件通知 UI 层。
    /// Procedure 层不引用任何 UI 类型。
    /// </summary>
    public class SplashState : SimpleFsmState<ILaunchSystem>
    {
        public override void OnEnter(ILaunchSystem context)
        {
            CEventRouter.Instance.FireNow((int)GlobalEventId.SplashBegin);
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
    /// 场景加载阶段。发射 GameLaunch 事件 → 初始化场景状态 → 异步加载场景。
    /// </summary>
    public class LoadSceneState : SimpleFsmState<ILaunchSystem>
    {
        public override void OnEnter(ILaunchSystem context)
        {
            // 1. Fire GameLaunch event — tells UI to close StartForm and re-open SplashForm as loading screen
            CEventRouter.Instance.FireNow((int)GlobalEventId.GameLaunch);

            // 2. Hide and lock cursor for gameplay
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 3. Initialize scene state and map config (single source of truth)
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

            // 4. Determine target scene name and save point index
            string targetSceneName;
            int savePointIndex;

            var playerProvider = gameManager.GetProvider("Player") as PlayerSaveDataProvider;
            if ((context.Mode & RunningMode.LoadData) != 0
                && playerProvider != null
                && !string.IsNullOrEmpty(playerProvider.RuntimeData.lastSceneName))
            {
                // Loading saved game: use saved scene and save point
                targetSceneName = playerProvider.RuntimeData.lastSceneName;
                savePointIndex = playerProvider.RuntimeData.lastSavePointIndex;
            }
            else
            {
                // New game or no-load mode: use config defaults
                targetSceneName = context.StartSceneName;
                savePointIndex = 0;
            }

            // 5. Validate scene name
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("[LoadSceneState] Target scene name is null or empty. Cannot load scene.");
                return;
            }

            // 6. Look up save point and set on scene load system
            var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
            var savePoint = sceneSystem.GetSavePointModel(targetSceneName, savePointIndex);
            if (savePoint != null)
            {
                loadSystem.SetTargetSavePoint(savePoint);
            }
            else
            {
                Debug.LogWarning($"[LoadSceneState] Save point not found for scene={targetSceneName} index={savePointIndex}, using default spawn");
            }

            // 7. Begin async scene loading
            loadSystem.AsyncLoadScene(targetSceneName);
        }
    }
}
