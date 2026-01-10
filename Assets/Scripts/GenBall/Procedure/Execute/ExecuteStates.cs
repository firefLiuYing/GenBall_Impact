using GenBall.Map;
using GenBall.UI;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Procedure.Execute
{
    public class ProcedureLoadState : FsmState<ExecuteComponent>
    {
        protected internal override void OnUpdate(Fsm<ExecuteComponent> fsm, float elapsedTime, float realElapseTime)
        {
            fsm.ChangeState<StartFormState>();
        }
    }

    public class StartFormState : FsmState<ExecuteComponent>
    {
        private Fsm<ExecuteComponent> _fsm;
        protected internal override void OnEnter(Fsm<ExecuteComponent> fsm)
        {
            _fsm = fsm;
            GameEntry.UI.OpenForm<StartForm>();
            _fsm.GetData<Variable<GameData>>("GameData")?.Observe(OnGameDataChanged);
        }

        protected internal override void OnExit(Fsm<ExecuteComponent> fsm, bool isShutdown = false)
        {
            _fsm.GetData<Variable<GameData>>("GameData")?.Unobserve(OnGameDataChanged);
        }

        private void OnGameDataChanged(GameData gameData)
        {
            _fsm.ChangeState<LoadSceneState>();
        }
    }

    public class LoadSceneState : FsmState<ExecuteComponent>
    {
        protected internal override void OnEnter(Fsm<ExecuteComponent> fsm)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (fsm.Owner.Mode == ExecuteComponent.PlayMode.Debug)
            {
                GameEntry.Scene.LoadScene(new LoadInfo()
                {
                    SceneName = fsm.Owner.StartSceneName,
                });
            }
        }
    }
}