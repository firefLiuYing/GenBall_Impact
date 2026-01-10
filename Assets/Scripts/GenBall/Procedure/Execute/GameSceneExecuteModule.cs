using System.Linq;
using GenBall.Map;
using GenBall.Procedure.Game;
using GenBall.UI;
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
            var loadInfo = GameManager.Instance.CachedLoadInfo;
            
            // 隐藏鼠标
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // 加载地图
            GameEntry.Map.LoadSavePointAround(loadInfo.SavePointIndex);
            // 加载UI
            GameEntry.UI.OpenForm<MainHud>();
            // 加载Player
            var savePointInfo = SceneMapIndexProvider.GetMapConfig(loadInfo.SceneName).savePointInfos.FirstOrDefault(s=>s.index==loadInfo.SavePointIndex);
            if (savePointInfo != null)
            {
                GameEntry.Player.CreatePlayer(savePointInfo.playerSpawnPosition, savePointInfo.playerSpawnRotation);
            }
            
        }
        
        public void Init()
        {
            if (GameManager.Instance.GameData== null||GameManager.Instance.CachedLoadInfo==null)
            {
                Debug.LogWarning("gzp 检测到不是Launcher场景启动，自动设置为不保存不读取");
                GameManager.Instance.Mode =0;
                GameManager.Instance.GameData = new();
                GameManager.Instance.CurSaveIndex = 0;
                GameManager.Instance.CachedLoadInfo = new()
                {
                    SceneName = SceneManager.GetActiveScene().name,
                };
            }
            
            Execute();
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