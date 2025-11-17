using GenBall.BattleSystem.Weapons;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public interface IBullet
    {
        public void Fire(IWeapon gun, IAttacker shooter, Vector3 spawnPoint);
        public void OnRecycle();
    }
}