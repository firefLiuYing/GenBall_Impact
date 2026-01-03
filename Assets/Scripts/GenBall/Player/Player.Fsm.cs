using System;
using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public partial class Player:IAttackable
    {
        private Fsm<Player> _fsm;
        private readonly List<FsmState<Player>> _states = new();
        private LiveDelegate<OnAttackDelegate> _onAttackDelegate;
        private void InitFsm()
        {
            RegisterStates();
            CreateFsm();
            RegisterFsmDatas();
        }

        private void StartFsm()
        {
            _fsm.Start<PlayerInitState>();
        }
        private void CreateFsm()
        {
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm("PlayerFsm", this, _states);
        }
        private void RegisterFsmDatas()
        {
            var moveInput = ReferencePool.Acquire<Variable<Vector2>>();
            _fsm.SetData("MoveInput",moveInput);   
            var velocity=ReferencePool.Acquire<Variable<Vector3>>();
            _fsm.SetData("Velocity",velocity);
            var viewInput = ReferencePool.Acquire<Variable<Vector2>>();
            _fsm.SetData("ViewInput",viewInput);
            var viewRotation = ReferencePool.Acquire<Variable<Quaternion>>();
            _fsm.SetData("ViewRotation",viewRotation);
            _onGround = ReferencePool.Acquire<Variable<bool>>();
            _fsm.SetData("OnGround",_onGround);
            var jumpInput = ReferencePool.Acquire<Variable<ButtonState>>();
            _fsm.SetData("JumpInput",jumpInput);
            var dashInput=ReferencePool.Acquire<Variable<ButtonState>>();
            _fsm.SetData("DashInput",dashInput);
            var jumpPreInput=ReferencePool.Acquire<Variable<bool>>();
            _fsm.SetData("JumpPreInput",jumpPreInput);
            _onAttackDelegate = LiveDelegate<OnAttackDelegate>.Create();
            _fsm.SetData("OnAttackDelegate",_onAttackDelegate);
        }
        
        private void RegisterStates()
        {
            _states.Add(new PlayerInitState());
            _states.Add(new PlayerMoveState());
            _states.Add(new PlayerJumpState());
            _states.Add(new PlayerDashState());
        }

        public AttackResult OnAttacked(AttackInfo attackInfo)
        {
            if (_onAttackDelegate.Value != null)
            {
                return _onAttackDelegate.Value.Invoke(attackInfo);
            }

            PlayerController.Instance.ApplyDamage(attackInfo.Damage);
            return AttackResult.Create(attackInfo.Damage);
        }

        public void AddEffect(IEffect effect)
        {
            throw new NotImplementedException();
        }

        public bool RemoveEffect(IEffect effect)
        {
            throw new NotImplementedException();
        }


        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public void FireEvent(object sender, GameEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void FireNow(object sender, GameEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}