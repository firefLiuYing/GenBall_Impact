using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class DefaultWeapon : MonoBehaviour,IWeapon
    {
        public IAttacker Owner { get;private set; }
        [SerializeField] private Transform bulletSpawnPoint;
        // private BulletCreator BulletCreator => GameEntry.GetModule<BulletCreator>();
        private EntityCreator<IBullet> BulletCreator => GameEntry.GetModule<EntityCreator<IBullet>>();
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
            // transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
        }

        public void EntityUpdate(float deltaTime)
        {
            _timer+=deltaTime;
            if (_autoFire && _timer > _countdownTime)
            {
                Fire();
            }
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

        public void OnUnequip()
        {
            
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