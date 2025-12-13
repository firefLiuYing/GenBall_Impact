using GenBall.BattleSystem;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm
{
    public abstract class FsmModule : Module
    {
        public abstract AttackResult OnAttacked(AttackInfo attackInfo);
        public abstract void OnDeath();
    }
}