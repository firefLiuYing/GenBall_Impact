using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class NormalFsmModule : FsmModule
    {
        private Fsm<EnemyEntity> _fsm;
        private readonly List<FsmState<EnemyEntity>> _states=new();
        public override void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log($"我被打了，是{attackInfo.Attacker}干的，扣了{attackInfo.Damage}血");
        }

        public override void Initialize()
        {
            _states.Clear();
            _states.Add(new InitState());
            _states.Add(new WanderState());
            
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm($"Normal_{GetHashCode()}",Owner, _states);
            _fsm.Start<InitState>();
        }

        public override void ModuleUpdate(float deltaTime)
        {
            
        }

        public override void ModuleFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public override void OnRecycle()
        {
            GameEntry.GetModule<FsmManager>().DestroyFsm(_fsm);
        }
    }
}