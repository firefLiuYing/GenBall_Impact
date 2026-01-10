using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class GameSceneExecuteModule : MonoBehaviour, IComponent
    {
        public int Priority => 10000;
        public void Init()
        {
            var gameData = GameManager.Instance.GameData;
            if (gameData == null)
            {
                Debug.LogWarning("gzp 检测到不是Launcher场景启动，自动设置为不保存不读取");
                GameManager.Instance.Mode =0;
                GameManager.Instance.GameData = new();
                GameManager.Instance.CurSaveIndex = 0;
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