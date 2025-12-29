using System;
using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class DefaultWeapon : WeaponBase
    {
        // public IAttacker Owner { get;private set; }
        [SerializeField] private Transform bulletSpawnPoint;
        // private BulletCreator BulletCreator => GameEntry.GetModule<BulletCreator>();
        private EntityCreator<IBullet> BulletCreator => GameEntry.GetModule<EntityCreator<IBullet>>();
        [SerializeField] private float countdownTime;
        private float _countdownTime;
        private float _timer;
        private bool _autoFire = false;
        
        public override IWeaponStats Stats { get; }

        protected override void OnTrigger(ButtonState triggerState)
        {
            // Debug.Log(triggerState);
            if (triggerState == ButtonState.Down)
            {
                _autoFire = true;
                if (_timer >= countdownTime)
                {
                    Fire();
                }
            }

            if (triggerState == ButtonState.Up)
            {
                _autoFire = false;
            }
        }

        protected override void OnEquip(IAttacker owner)
        {
            _countdownTime = countdownTime;
            _timer = 0f;
            // transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            // gameObject.SetActive(true);
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer+=deltaTime;
            if (_autoFire && _timer > _countdownTime)
            {
                Fire();
            }
        }


        private void Fire()
        {
            _timer = 0f;
            // var bullet=BulletCreator.CreateBullet<DefaultBullet>();
            var bullet = BulletCreator.CreateEntity<DefaultBullet>();
            bullet.Fire(this,bulletSpawnPoint.position,Camera.main.transform.forward.normalized);
        }
    }
}