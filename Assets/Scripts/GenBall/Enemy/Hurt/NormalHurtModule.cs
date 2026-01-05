using GenBall.BattleSystem;

namespace GenBall.Enemy.Hurt
{
    public class NormalHurtModule : HurtModule
    {
        public override void Initialize()
        {
            
        }

        public override void OnRecycle()
        {
            
        }

        public override AttackResult OnAttacked(AttackInfo attackInfo)
        {
            Owner.TakeDamage(attackInfo.Damage);
            return AttackResult.Create(attackInfo.Damage);
        }
    }
}
