using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.UI;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class SceneExecutorSystemDefault : ISceneExecutorSystem
    {
        private IGameManagerSystem _gameManager;
        // TODO: Enemy 迁移后重新启用
        // private ISceneStateSystem _sceneSystem;
        private ITeleportSystem _teleportSystem;
        // private IConfigProvider _configProvider;
        private IPlayerSystem _playerSystem;

        public void Init()
        {
            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            // _sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _teleportSystem = SystemRepository.Instance.GetSystem<ITeleportSystem>();
            // _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            _playerSystem = SystemRepository.Instance.GetSystem<IPlayerSystem>();
        }

        public void UnInit() { }

        public void ExecuteSceneSetup()
        {
            var gameData = _gameManager.GameData;
            if (gameData == null)
            {
                Debug.LogWarning("[SceneExecutorSystem] GameData is null, skipping scene setup.");
                return;
            }

            // 重置光标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // 加载地图
            // GameEntry.Map.LoadSavePointAround(loadInfo.SavePointIndex);
            // TODO: Enemy 尚未迁移到新框架，LoadEnemyUnit() 暂不启用
            // 加载敌人
            // LoadEnemyUnit();
            // 初始化UI (新 MVP)
            MainHudFormLogic.Open();
            // 加载Player
            // var savePointInfo = SceneMapIndexProvider.GetMapConfig(loadInfo.SceneName).savePointInfos.FirstOrDefault(s=>s.index==loadInfo.SavePointIndex);
            // if (savePointInfo != null)
            // {
            //     GameEntry.Player.CreatePlayer(savePointInfo.playerSpawnPosition, savePointInfo.playerSpawnRotation);
            // }
            // Spawn player at teleport target or default position
            var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
            var targetSavePoint = loadSystem.TargetSavePoint;
            if (targetSavePoint != null)
            {
                _playerSystem.CreatePlayer(targetSavePoint.spawnPosition, targetSavePoint.spawnRotation);
            }
            else
            {
                _playerSystem.CreatePlayer();
            }
            _teleportSystem.IsTeleporting = false;

            // TODO: Enemy 迁移后删除此测试代码
            // EnemyId.TestOrbis.Create();
        }

        // TODO: Enemy 迁移到新框架后重新启用
        /*
        private static readonly Dictionary<string, string> EnemyPrefabPaths = new()
        {
            { "NormalOrbis", "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/NormalOrbis.prefab" },
        };

        private void LoadEnemyUnit()
        {
            var enemyUnitModels = _sceneSystem.GetAllUnKilledEnemyModel(SceneManager.GetActiveScene().name);
            foreach (var enemyUnitModel in enemyUnitModels)
            {
                var path = EnemyPrefabPaths[enemyUnitModel.enemyType];
                var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
                var go = Object.Instantiate(prefab, enemyUnitModel.spawnPosition, enemyUnitModel.spawnRotation);

                // Legacy initialization
                var enemy = go.GetComponent<EnemyBase>();
                enemy.Initialize();

                // BattleEntity assembly
                var aiController = go.GetComponentInChildren<EnemyAIController>();
                if (aiController != null)
                {
                    EnemyEntityFactory.AssembleEnemy(go, aiController.AiConfig);
                }
            }
        }
        */
    }
}
