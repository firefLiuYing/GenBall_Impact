using GenBall.BattleSystem;
using UnityEngine;

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
            Debug.Log($" ‹µΩ{attackInfo.Damage}µ„…À∫¶");
            Owner.TakeDamage(attackInfo.Damage);
            return AttackResult.Create(attackInfo.Damage);
        }
    }
}
