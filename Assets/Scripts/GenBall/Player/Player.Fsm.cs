using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public partial class Player
    {
        private Fsm<Player> _fsm;
        private readonly List<FsmState<Player>> _states = new();
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
            var fireInput = ReferencePool.Acquire<Variable<ButtonState>>();
            _fsm.SetData("FireInput",fireInput);
            var jumpPreInput=ReferencePool.Acquire<Variable<bool>>();
            _fsm.SetData("JumpPreInput",jumpPreInput);
        }
        
        private void RegisterStates()
        {
            _states.Add(new PlayerInitState());
            _states.Add(new PlayerMoveState());
            _states.Add(new PlayerJumpState());
            _states.Add(new PlayerDashState());
        }
    }
}