using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class DefaultWeapon : MonoBehaviour,IWeapon
    {
        public IAttacker Owner { get;private set; }
        [SerializeField] private Transform bulletSpawnPoint;
        private BulletCreator BulletCreator => GameEntry.GetModule<BulletCreator>();
        [SerializeField] private float countdownTime;
        private float _countdownTime;
        private float _timer;
        private bool _autoFire = false;
        public void Trigger(ButtonState triggerState)
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

        public void OnEquip(IAttacker owner)
        {
            Owner = owner;
            _countdownTime = countdownTime;
            _timer = 0f;
            gameObject.SetActive(true);
        }

        public void WeaponUpdate(float deltaTime)
        {
            _timer+=deltaTime;
            if (_autoFire && _timer > countdownTime)
            {
                Fire();
            }
        }

        public void OnUnequip()
        {
            
        }

        private void Fire()
        {
            _timer = 0f;
            var bullet=BulletCreator.CreateBullet<DefaultBullet>();
            bullet.Fire(this,bulletSpawnPoint.position);
        }
    }
}