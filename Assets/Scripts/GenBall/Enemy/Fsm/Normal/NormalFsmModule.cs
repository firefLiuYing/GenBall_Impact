using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class NormalFsmModule : FsmModule
    {
        private Fsm<EnemyEntity> _fsm;
        public override void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log($"我被打了，是{attackInfo.Attacker}干的，扣了{attackInfo.Damage}血");
        }

        public override void Initialize()
        {
            
        }

        public override void ModuleUpdate(float deltaTime)
        {
            
        }

        public override void ModuleFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public override void OnRecycle()
        {
            
        }
    }
}