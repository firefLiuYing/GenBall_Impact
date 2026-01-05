using GenBall.BattleSystem;

namespace GenBall.Enemy.Hurt
{
    public abstract class HurtModule : Module
    {
        public abstract AttackResult OnAttacked(AttackInfo attackInfo);
    }
}