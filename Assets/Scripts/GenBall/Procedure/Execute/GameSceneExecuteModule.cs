using System.Linq;
using GenBall.Enemy;
using GenBall.Map;
using GenBall.Procedure.Game;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class GameSceneExecuteModule : MonoBehaviour, IComponent
    {
        public int Priority => 10000;

        private void Execute()
        {
            var gameData = GameManager.Instance.GameData;
            
            // 隐藏鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // 初始化地图存档信息
            SceneSystem.Instance.InitializeMapConfig(ConfigProvider.GetOrCreateMapConfig());
            SceneSystem.Instance.InitializeSceneStateObjs(gameData.mapSaveData);
            // 加载地图
            // GameEntry.Map.LoadSavePointAround(loadInfo.SavePointIndex);
            // 加载敌人
            LoadEnemyUnit();
            // 加载UI
            GameEntry.UI.OpenForm<MainHud>();
            // 加载Player
            // var savePointInfo = SceneMapIndexProvider.GetMapConfig(loadInfo.SceneName).savePointInfos.FirstOrDefault(s=>s.index==loadInfo.SavePointIndex);
            // if (savePointInfo != null)
            // {
            //     GameEntry.Player.CreatePlayer(savePointInfo.playerSpawnPosition, savePointInfo.playerSpawnRotation);
            // }
            if (TeleportSystem.Instance.IsTeleporting)
            {
                var targetSavePoint = TeleportSystem.Instance.CachedSavePointModel;
                GameEntry.Player.CreatePlayer(targetSavePoint.spawnPosition,targetSavePoint.spawnRotation);
                TeleportSystem.Instance.IsTeleporting=false;
            }
            else
            {
                GameEntry.Player.CreatePlayer();
            }
        }
        
        public void Init()
        {
            if (GameManager.Instance.GameData== null)
            {
                Debug.LogWarning("gzp 检测到不是Launcher场景启动，自动设置为不保存不读取");
                GameManager.Instance.Mode =0;
                GameManager.Instance.GameData = new();
                GameManager.Instance.CurSaveIndex = 0;
            }
            
            Execute();
        }

        private void LoadEnemyUnit()
        {
            var enemyUnitModels = SceneSystem.Instance.GetAllUnKilledEnemyModel(SceneManager.GetActiveScene().name);
            foreach (var enemyUnitModel in enemyUnitModels)
            {
                var enemy= GameEntry.GetModule<EntityCreator<IEnemy>>().CreateEntity<EnemyBase>(
                    enemyUnitModel.enemyType,enemyUnitModel.spawnPosition,enemyUnitModel.spawnRotation);
                enemy.Initialize();
            }
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
    }
}