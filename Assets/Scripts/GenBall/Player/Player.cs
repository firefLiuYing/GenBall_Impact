using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public class Player:MonoBehaviour
    {
        private Fsm<Player> _fsm;
        private readonly List<FsmState<Player>> _states = new();
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private Variable<bool> _onGround;
        private void Init()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<CapsuleCollider>();
            RegisterStates();
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm("PlayerFsm", this, _states);
            RegisterFsmDatas();
            RegisterEvents();
        }

        private void Awake()
        {
            Init();
            _fsm.Start<PlayerMoveState>();
        }

        private void FixedUpdate()
        {
            GroundDetection();
        }

        private void GroundDetection()
        {
            int layerToExclude = LayerMask.NameToLayer("Player");
            LayerMask layerMask=~(1<<layerToExclude);
            var origin = transform.position + _collider.center;
            var hit=Physics.Raycast(origin,Vector3.down,_collider.height/2+0.01f,layerMask);
            if(hit!=_onGround.Value) _onGround.PostValue(hit);
        }
        private void OnVelocityChange(Vector3 velocity)=>_rigidbody.velocity=velocity;

        private void OnViewRotationChange(Quaternion viewRotation)=>Camera.main.transform.rotation=viewRotation;
        private void OnMoveInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<Vector2> args) return;
            var moveInput=_fsm.GetData<Variable<Vector2>>("MoveInput");
            moveInput.PostValue(args.Args);
        }

        private void OnViewInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<Vector2> args) return;
            var viewInput=_fsm.GetData<Variable<Vector2>>("ViewInput");
            viewInput.PostValue(args.Args);
        }

        private void OnDashInputChange(object sender, GameEventArgs eventArgs)
        {
            if(eventArgs is not InputEventArgs<ButtonState> args) return;
            var dashInput=_fsm.GetData<Variable<ButtonState>>("DashInput");
            dashInput.PostValue(args.Args);
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
        }

        private void RegisterStates()
        {
            _states.Add(new PlayerMoveState());
            _states.Add(new PlayerJumpState());
            _states.Add(new PlayerDashState());
        }

        private void RegisterEvents()
        {
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<Vector2>.GetHashCode("MoveInput"),OnMoveInputChange);
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<Vector2>.GetHashCode("ViewInput"),OnViewInputChange);
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<ButtonState>.GetHashCode("DashInput"),OnDashInputChange);
            
            _fsm.GetData<Variable<Vector3>>("Velocity").Observe(OnVelocityChange);
            _fsm.GetData<Variable<Quaternion>>("ViewRotation").Observe(OnViewRotationChange);
        }
    }
}