using GenBall.Map;
using GenBall.Procedure.Game;
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
            SceneSystem.Instance.InitializeSceneStateObjs(fsm.GetData<Variable<GameData>>("GameData").Value.mapSaveData);
            SceneSystem.Instance.InitializeMapConfig(ConfigProvider.GetOrCreateMapConfig());
            if ((fsm.Owner.Mode&RunningMode.LoadData)==0)
            {
                TeleportSystem.Instance.Teleport(new TeleportRequestInfo()
                {
                    SceneName = fsm.Owner.StartSceneName,
                    SavePointIndex = 0
                });
            }
        }
    }
}