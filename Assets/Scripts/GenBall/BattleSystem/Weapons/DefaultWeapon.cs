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
        public override IWeaponStats Stats { get; } = new WeaponStats();

        protected override void OnEquip(IAttacker owner)
        {
            transform.localPosition = Vector3.zero;
        }

        public class WeaponStats : IWeaponStats
        {
            public int Damage => DamageStats.CurrentValue;
            public float ImpactForce => ImpactForceStats.CurrentValue;
            public IntStat DamageStats { get; } = new IntStat(50);
            public FloatStat ImpactForceStats { get; } = new FloatStat(1);
        }
    }
}