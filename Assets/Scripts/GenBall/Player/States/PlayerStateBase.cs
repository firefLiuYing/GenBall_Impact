using GenBall.BattleSystem;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public abstract class PlayerStateBase : FsmState<Player>
    {
        public abstract void OnAttacked(AttackInfo attackInfo);
    }
}