using GenBall.BattleSystem.Weapons;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public interface IBullet
    {
        public IWeapon Source { get; }
        public void Fire(IWeapon source, Vector3 spawnPoint);
        public void OnRecycle();
        public void BulletUpdate(float deltaTime);
    }
}