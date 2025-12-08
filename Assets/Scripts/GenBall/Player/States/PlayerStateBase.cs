using GenBall.BattleSystem;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public abstract class PlayerStateBase : FsmState<Player>
    {
        public abstract void OnInteract(IInteractToken interactToken,out  IInteractToken[] responses);
    }
}