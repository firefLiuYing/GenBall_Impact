using GenBall.BattleSystem;
using GenBall.Enemy.Hurt;
using UnityEngine;

namespace GenBall.Enemy
{
    [RequireComponent(typeof(NormalHurtModule))]
    public class NormalOrbis : EnemyBase
    {
        private NormalHurtModule _hurtModule;
        protected override void OnInitialize()
        {
            _hurtModule=GetModule<NormalHurtModule>();
        }

        public override int MaxHealth => 100;

        public override AttackResult OnAttacked(AttackInfo attackInfo) => _hurtModule.OnAttacked(attackInfo);
    }
}