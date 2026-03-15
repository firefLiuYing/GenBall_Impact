using System;
using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public class NormalTriggerController : MonoBehaviour, IWeaponTriggerController
    {
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private FireMode fireMode=FireMode.Automatic;
        public void Trigger(ButtonState buttonState)
        {
            if (buttonState == ButtonState.Down)
            {
                GameEntry.Bullet.FireBullet(BulletLaunchInfo.Create(
                    new BulletModel
                    {
                        Speed = 100
                    },
                    Camera.main.transform.position,
                    bulletSpawnPoint.position,
                    Camera.main.transform.forward,
                    gameObject));
            }
        }
        [Flags]
        public enum FireMode
        {
            None = 1<<0,
            Automatic = 1 << 1,
            AutoReload = 1 << 2,
        }
    }
}