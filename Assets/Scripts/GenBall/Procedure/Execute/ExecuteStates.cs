using GenBall.Map;
using GenBall.Procedure.Game;
using GenBall.UI;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class ProcedureLoadState : FsmState<ILaunchSystem>
    {
        protected internal override void OnUpdate(Fsm<ILaunchSystem> fsm, float elapsedTime, float realElapseTime)
        {
            fsm.ChangeState<StartFormState>();
        }
    }

    public class StartFormState : FsmState<ILaunchSystem>
    {
        private Fsm<ILaunchSystem> _fsm;
        protected internal override void OnEnter(Fsm<ILaunchSystem> fsm)
        {
            _fsm = fsm;
            GameEntry.UI.OpenForm<StartForm>();
            _fsm.GetData<Variable<GameData>>("GameData")?.Observe(OnGameDataChanged);
        }

        protected internal override void OnExit(Fsm<ILaunchSystem> fsm, bool isShutdown = false)
        {
            _fsm.GetData<Variable<GameData>>("GameData")?.Unobserve(OnGameDataChanged);
        }

        private void OnGameDataChanged(GameData gameData)
        {
            _fsm.ChangeState<LoadSceneState>();
        }
    }

    public class LoadSceneState : FsmState<ILaunchSystem>
    {
        protected internal override void OnEnter(Fsm<ILaunchSystem> fsm)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            var sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            sceneSystem.InitializeSceneStateObjs(fsm.GetData<Variable<GameData>>("GameData").Value.mapSaveData);
            #if UNITY_EDITOR
            sceneSystem.InitializeMapConfig(ConfigProvider.GetOrCreateMapConfig());
            #else
            sceneSystem.InitializeMapConfig(new MapModel());
            #endif
            if ((fsm.Owner.Mode&RunningMode.LoadData)==0)
            {
                SystemRepository.Instance.GetSystem<ITeleportSystem>().Teleport(new TeleportRequestInfo()
                {
                    SceneName = fsm.Owner.StartSceneName,
                    SavePointIndex = 0
                });
            }
        }
    }
}