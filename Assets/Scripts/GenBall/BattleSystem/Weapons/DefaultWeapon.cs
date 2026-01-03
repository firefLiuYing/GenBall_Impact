using System;
using GenBall.BattleSystem.Bullets;
using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    [RequireComponent(typeof(FireComponent))]
    public class DefaultWeapon : WeaponBase
    {
        private FireComponent Fire => GetWeaponComponent<FireComponent>();
        public override IWeaponStats Stats { get; }

        // protected override void OnTrigger(ButtonState triggerState)=>Fire.Trigger(triggerState);

        protected override void OnEquip(IAttacker owner)
        {
            transform.localPosition = Vector3.zero;
        }
    }
}