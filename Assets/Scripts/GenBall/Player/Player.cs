using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public partial class Player:MonoBehaviour
    {
        private void Awake()
        {
            InitPhysics();
            InitFsm();
            RegisterEvents();
            StartFsm();
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