using GenBall.BattleSystem.Weapons;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class DefaultBullet : MonoBehaviour, IBullet
    {
        public IWeapon Source { get;private set; }
        
        [SerializeField]private float bulletSpeed;

        private Vector3 _spawnPoint;
        private Vector3 _direction;
        private Vector3 _logicSource;
        private Vector3 _logicTarget;
        public void Fire(IWeapon source, IAttacker shooter, Vector3 spawnPoint)
        {
            Source = source;
        }

        public void OnRecycle()
        {
            
        }

        public void BulletUpdate(float deltaTime)
        {
            
        }
    }
}