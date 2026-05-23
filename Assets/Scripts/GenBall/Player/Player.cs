using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Player
{
    public partial class Player:MonoBehaviour,IEntityFrameUpdate,IEntityLogicUpdate
    {
        [SerializeField]private Transform mainCameraTransform;

        private void SetCamera()
        {
            if (mainCameraTransform == null)
            {
                throw new Exception("Main Camera Transform û����");
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
            InitHealth();
            InitCountdown();
            RegisterEvents();
            StartFsm();
            EquipPhysicsWeapon<DefaultWeapon>();

            var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            entitySystem.AddFrameUpdate(this);
            entitySystem.AddLogicUpdate(this);
        }

        public void FrameUpdate(float deltaTime)
        {
            CountdownUpdate(deltaTime);
        }

        public void LogicUpdate(float deltaTime)
        {
            PhysicsUpdate();
        }

        private void OnDestroy()
        {
            var entitySystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            entitySystem?.RemoveFrameUpdate(this);
            entitySystem?.RemoveLogicUpdate(this);
        }

        private void RegisterEvents()
        {
            RegisterInputHandlers();
            
            _fsm.GetData<Variable<Vector3>>("Velocity").Observe(OnVelocityChange);
            _fsm.GetData<Variable<Quaternion>>("ViewRotation").Observe(OnViewRotationChange);
        }

    }
}