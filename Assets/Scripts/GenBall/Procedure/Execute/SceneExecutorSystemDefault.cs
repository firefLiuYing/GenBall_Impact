using System.Linq;
using GenBall.BattleSystem.Character;
using GenBall.Enemy;
using GenBall.Framework.Config;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class SceneExecutorSystemDefault : ISceneExecutorSystem
    {
        private IGameManagerSystem _gameManager;
        private ISceneStateSystem _sceneSystem;
        private ITeleportSystem _teleportSystem;
        private IConfigProvider _configProvider;
        private IPlayerSystem _playerSystem;

        public void Init()
        {
            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _teleportSystem = SystemRepository.Instance.GetSystem<ITeleportSystem>();
            _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            _playerSystem = SystemRepository.Instance.GetSystem<IPlayerSystem>();
        }

        public void UnInit() { }

        public void ExecuteSceneSetup()
        {
            var gameData = _gameManager.GameData;

            // 重置光标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // 初始化地图存档信息
            #if UNITY_EDITOR
            _sceneSystem.InitializeMapConfig(ConfigProvider.GetOrCreateMapConfig());
            _sceneSystem.InitializeSceneStateObjs(gameData.mapSaveData);
            #else
            _sceneSystem.InitializeMapConfig(new MapModel());
            _sceneSystem.InitializeSceneStateObjs(new MapSaveData());
            #endif
            // 加载地图
            // GameEntry.Map.LoadSavePointAround(loadInfo.SavePointIndex);
            // 加载敌人
            LoadEnemyUnit();
            // 初始化UI
            GameEntry.UI.OpenForm<MainHud>();
            // 加载Player
            // var savePointInfo = SceneMapIndexProvider.GetMapConfig(loadInfo.SceneName).savePointInfos.FirstOrDefault(s=>s.index==loadInfo.SavePointIndex);
            // if (savePointInfo != null)
            // {
            //     GameEntry.Player.CreatePlayer(savePointInfo.playerSpawnPosition, savePointInfo.playerSpawnRotation);
            // }
            if (_teleportSystem.IsTeleporting)
            {
                var targetSavePoint = _teleportSystem.CachedSavePointModel;
                _playerSystem.CreatePlayer(targetSavePoint.spawnPosition, targetSavePoint.spawnRotation);
                _teleportSystem.IsTeleporting = false;
            }
            else
            {
                _playerSystem.CreatePlayer();
            }

            // todo gzp 测试代码，记得删
            GameEntry.CharacterCreator.CreateEntity<CharacterState>(nameof(EnemyId.TestOrbis));
        }

        private void LoadEnemyUnit()
        {
            var enemyUnitModels = _sceneSystem.GetAllUnKilledEnemyModel(SceneManager.GetActiveScene().name);
            foreach (var enemyUnitModel in enemyUnitModels)
            {
                var enemy = GameEntry.GetModule<EntityCreator<IEnemy>>().CreateEntity<EnemyBase>(
                    enemyUnitModel.enemyType, enemyUnitModel.spawnPosition, enemyUnitModel.spawnRotation);
                enemy.Initialize();
            }
        }
    }
}
