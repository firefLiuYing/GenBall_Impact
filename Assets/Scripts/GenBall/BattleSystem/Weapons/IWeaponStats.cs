using System;

namespace GenBall.BattleSystem.Weapons
{
    [Obsolete]
    public interface IWeaponStats
    {
        public int Damage { get; }
        public float ImpactForce { get; }
    }
}