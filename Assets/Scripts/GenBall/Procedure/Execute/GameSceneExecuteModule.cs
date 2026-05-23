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
            var gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            if (gameManager.GameData == null)
            {
                Debug.LogWarning("gzp 检测到未经过Launcher，将自动初始化为不读取存档");
                gameManager.Mode = 0;
                gameManager.GameData = new();
                gameManager.CurSaveIndex = 0;
            }

            SystemRepository.Instance.GetSystem<ISceneExecutorSystem>().ExecuteSceneSetup();
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
