using GenBall.BattleSystem;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm
{
    public abstract class FsmModule : Module
    {
        public abstract void OnAttacked(AttackInfo attackInfo);
    }
}