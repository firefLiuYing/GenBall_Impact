using System;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Generated;
using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    [RequireComponent(typeof(MagazineComponent))]
    public class FireComponent : WeaponComponentBase
    {
        [SerializeField] private float baseFireInterval;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private FireMode fireMode=FireMode.Automatic;
        public Type BulletType { get; private set; }=typeof(DefaultBullet);
        private EntityCreator<IBullet> BulletCreator => GameEntry.GetModule<EntityCreator<IBullet>>();
        private MagazineComponent Magazine => Owner.GetWeaponComponent<MagazineComponent>();
        public FireMode Mode { get=>fireMode; set=>fireMode=value; } 
        public void SetBulletType(Type type)=>BulletType = type;
        public readonly FloatStat FireInterval= new FloatStat();
        public bool CanFire { get; set; } = true;
        public void SetBaseFireInterval(float time)=>baseFireInterval = time;
        
        private float _fireColdTime;
        private ButtonState _fireButtonState;
        private bool _fireButtonDownThisFrame=false;

        protected override void OnEquip()
        {
            FireInterval.SetBaseValue(baseFireInterval);
            _fireColdTime=FireInterval.CurrentValue+0.01f;
            
            // Mode=FireMode.None;
            
            RegisterEvents();
        }

        protected override void OnUnequip()
        {
            UnregisterEvents();
            _fireColdTime = 0;
        }

        private void OnUpdate(float deltaTime)
        {
            _fireColdTime += deltaTime;
            if (CanFireCurFrame())
            {
                Fire();
            }
        }

        private void Trigger(ButtonState buttonState)
        {
            _fireButtonState = buttonState;
            if (_fireButtonState == ButtonState.Down)
            {
                _fireButtonDownThisFrame = true;
            }
        }

        private bool CanFireCurFrame()
        {
            if(!CanFire)  return false;
            if(_fireColdTime<FireInterval.CurrentValue) return false;
            if (!Magazine.CanFire()) return false;
            if ((Mode & FireMode.Automatic) == FireMode.Automatic)
            {
                return _fireButtonState is ButtonState.Down or ButtonState.Hold;
            }
            return _fireButtonDownThisFrame;
        }

        private void Fire()
        {
            _fireColdTime = 0;
            _fireButtonDownThisFrame = false;
            Magazine.Fire();
            var bullet = BulletCreator.CreateEntity(BulletType);
            bullet.Fire(Owner,bulletSpawnPoint.position,PlayerController.PlayerFace);
        }

        private void RegisterEvents()
        {
            Owner.SubscribeSystemUpdate(OnUpdate);
            Owner.SubscribeInputTrigger(Trigger);
        }

        private void UnregisterEvents()
        {
            Owner.UnsubscribeSystemUpdate(OnUpdate);
            Owner.UnsubscribeInputTrigger(Trigger);
        }
        [Flags]
        public enum FireMode
        {
            None = 0,
            Automatic = 1 << 0,
            AutoReload = 1 << 1,
        }
    }
}