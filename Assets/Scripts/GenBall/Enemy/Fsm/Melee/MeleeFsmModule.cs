using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class MeleeFsmModule : FsmModule
    {
        private Fsm<EnemyEntity> _fsm;
        private readonly List<FsmState<EnemyEntity>> _states=new();
        private Variable<Player.Player> _target;
        private Variable<int> _health;

        private LiveDelegate<OnAttackDelegate> _onAttackDelegate;
        public override AttackResult OnAttacked(AttackInfo attackInfo)
        {
            if (_onAttackDelegate.Value != null)
            {
                return _onAttackDelegate.Value.Invoke(attackInfo);
            }
            _health.PostValue(_health.Value-attackInfo.Damage);
            return AttackResult.Hit;
        }

        public override void OnDeath()
        {
            var deadArgs = EnemyDeadEventArgs.Create(Owner);
            deadArgs.KillPoints = 10;
            GameEntry.GetModule<EventManager>().Fire(this,deadArgs);
        }

        public override void Initialize()
        {
            _states.Clear();
            _states.Add(new InitState());
            _states.Add(new WanderState());
            _states.Add(new ChaseState());
            _states.Add(new BackState());
            _states.Add(new AttackState());
            _states.Add(new DeathState());
            
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm($"Normal_{GetHashCode()}",Owner, _states);
            RegisterFsmDatas();
            RegisterFsmEvents();

            // _fsm.PrintLog = true;
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
            _onAttackDelegate=ReferencePool.Acquire<LiveDelegate<OnAttackDelegate>>();
            _fsm.SetData("OnAttackDelegate", _onAttackDelegate);
            _health = Variable<int>.Create();
            _fsm.SetData("Health", _health);
            // todo gzp 后续改成可以读配置
            _health.PostValue(100);
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