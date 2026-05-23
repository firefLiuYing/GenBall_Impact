using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class ExecuteComponent : MonoBehaviour, IComponent
    {
        public int Priority => 10000;

        public void Init()
        {
            Debug.Log("[ExecuteComponent] Delegating to ILaunchSystem");
        }

        public void StartNewGame()
        {
            SystemRepository.Instance.GetSystem<ILaunchSystem>().StartNewGame();
        }

        public void ContinueLastGame()
        {
            SystemRepository.Instance.GetSystem<ILaunchSystem>().ContinueLastGame();
        }

        public void LoadGame(int saveIndex)
        {
            SystemRepository.Instance.GetSystem<ILaunchSystem>().LoadGame(saveIndex);
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
