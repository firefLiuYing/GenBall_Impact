using GenBall.Event;
using GenBall.Framework.Config;
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

            // Load scene config via IConfigProvider and convert to MapModel for SceneSystem compatibility.
            // TODO: Refactor SceneSystem to accept SceneConfigCollection directly, then remove MapModel.
            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var sceneConfig = configProvider?.GetConfig<SceneConfigCollection>();
            sceneSystem.InitializeMapConfig(ConvertToMapModel(sceneConfig));

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

        /// <summary>
        /// Temporary conversion from new SceneConfigCollection to legacy MapModel.
        /// TODO: Remove when SceneSystem directly consumes SceneConfigCollection.
        /// </summary>
        private static MapModel ConvertToMapModel(SceneConfigCollection sceneConfig)
        {
            var mapModel = ScriptableObject.CreateInstance<MapModel>();
            if (sceneConfig == null) return mapModel;

            foreach (var entry in sceneConfig.scenes)
            {
                var sceneModel = new SceneModel
                {
                    sceneName = entry.sceneName,
                    displayName = entry.displayName,
                };

                foreach (var sp in entry.savePoints)
                {
                    sceneModel.savePoints.Add(new SavePointModel
                    {
                        id = sp.id,
                        displayName = sp.displayName,
                        spawnPosition = sp.position,
                        spawnRotation = sp.rotation,
                    });
                }

                foreach (var es in entry.enemySpawns)
                {
                    sceneModel.enemyUnits.Add(new EnemyUnitModel
                    {
                        id = es.id,
                        enemyType = es.enemyType,
                        spawnPosition = es.position,
                        spawnRotation = es.rotation,
                    });
                }

                mapModel.scenes.Add(sceneModel);
            }

            return mapModel;
        }
    }
}
