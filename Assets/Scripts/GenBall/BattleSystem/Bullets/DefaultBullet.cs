using GenBall.BattleSystem.Weapons;
using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    public class DefaultBullet : MonoBehaviour, IBullet
    {
        public IWeapon Source { get;private set; }

        public void Fire(IWeapon source, IAttacker shooter, Vector3 spawnPoint)
        {
            Source = source;
        }

        public void OnRecycle()
        {
            
        }
    }
}