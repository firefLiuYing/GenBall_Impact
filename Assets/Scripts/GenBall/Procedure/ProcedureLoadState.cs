using GenBall.Player;
using GenBall.UI;
using Yueyn.Fsm;

namespace GenBall.Procedure
{
    public class ProcedureLoadState : FsmState<ExecuteProcedure>
    {
        protected internal override void OnEnter(Fsm<ExecuteProcedure> fsm)
        {
            LoadMainHud();
            LoadPlayer();
        }

        private void LoadPlayer()
        {
            GameEntry.GetModule<PlayerManager>().CreatePlayer();
        }

        private void LoadMainHud()
        {
            GameEntry.GetModule<UIManager>().OpenForm<MainHud>();
        }
    }
}