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
        private void Init()
        {
            _rigidbody=GetComponent<Rigidbody>();
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
        
        private void OnVelocityChange(Vector3 velocity)=>_rigidbody.velocity=velocity;

        private void OnViewRotationChange(Quaternion viewRotation)
        {
            Camera.main.transform.rotation=viewRotation;
        }
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
        }

        private void RegisterStates()
        {
            _states.Add(new PlayerMoveState());
        }

        private void RegisterEvents()
        {
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<Vector2>.GetHashCode("MoveInput"),OnMoveInputChange);
            GameEntry.GetModule<EventManager>().Subscribe(InputEventArgs<Vector2>.GetHashCode("ViewInput"),OnViewInputChange);
            
            _fsm.GetData<Variable<Vector3>>("Velocity").Observe(OnVelocityChange);
            _fsm.GetData<Variable<Quaternion>>("ViewRotation").Observe(OnViewRotationChange);
        }
    }
}