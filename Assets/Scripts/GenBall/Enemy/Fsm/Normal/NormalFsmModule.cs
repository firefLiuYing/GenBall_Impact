using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class NormalFsmModule : FsmModule
    {
        private Fsm<EnemyEntity> _fsm;
        private readonly List<FsmState<EnemyEntity>> _states=new();
        private Variable<Player.Player> _target;
        public override void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log($"我被打了，是{attackInfo.Attacker}干的，扣了{attackInfo.Damage}血");
        }
    
        public override void Initialize()
        {
            _states.Clear();
            _states.Add(new InitState());
            _states.Add(new WanderState());
            _states.Add(new ChaseState());
            _states.Add(new BackState());
            _states.Add(new AttackState());
            
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm($"Normal_{GetHashCode()}",Owner, _states);
            RegisterFsmDatas();
            RegisterFsmEvents();
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
            UnregisterFsmEvents();
            GameEntry.GetModule<FsmManager>().DestroyFsm(_fsm);
        }
        
        private void RegisterFsmDatas()
        {
            _target = ReferencePool.Acquire<Variable<Player.Player>>();
            _fsm.SetData("Target", _target);
        }

        private void RegisterFsmEvents()
        {
            _target.Observe(Owner.SetTarget);
        }

        private void UnregisterFsmEvents()
        {
            _target.Unobserve(Owner.SetTarget);
        }
    }
}