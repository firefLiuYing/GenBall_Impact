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
            InitCountdown();
            RegisterEvents();
            StartFsm();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            CountdownUpdate(deltaTime);
        }

        private void FixedUpdate()
        {
            PhysicsUpdate();
        }

        private void RegisterEvents()
        {
            RegisterInputHandlers();
            
            _fsm.GetData<Variable<Vector3>>("Velocity").Observe(OnVelocityChange);
            _fsm.GetData<Variable<Quaternion>>("ViewRotation").Observe(OnViewRotationChange);
        }
    }
}