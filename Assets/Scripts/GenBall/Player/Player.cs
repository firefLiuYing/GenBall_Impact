using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;

namespace GenBall.Player
{
    public partial class Player:MonoBehaviour,IEntity
    {
        [SerializeField]private Transform mainCameraTransform;

        private void SetCamera()
        {
            if (mainCameraTransform == null)
            {
                throw new Exception("Main Camera Transform √ª≈‰÷√");
            }
            var camTrans=Camera.main.transform;
            camTrans.SetParent(mainCameraTransform,false);
            camTrans.localPosition = Vector3.zero;
            camTrans.localRotation = Quaternion.identity;
        }
      
        public void Initialize()
        {
            gameObject.SetActive(true);
            SetCamera();
            InitPhysics();
            InitFsm();
            InitCountdown();
            RegisterEvents();
            StartFsm();
            EquipPhysicsWeapon<DefaultWeapon>();
        }
        public void EntityUpdate(float deltaTime)
        {
            CountdownUpdate(deltaTime);
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            PhysicsUpdate();
        }

        private void RegisterEvents()
        {
            RegisterInputHandlers();
            
            _fsm.GetData<Variable<Vector3>>("Velocity").Observe(OnVelocityChange);
            _fsm.GetData<Variable<Quaternion>>("ViewRotation").Observe(OnViewRotationChange);
        }

        public void OnRecycle()
        {
            
        }
    }
}