using UnityEngine;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Context passed to damage calculators. Contains all possible damage sources.
    /// Any field can be null depending on the entity type.
    /// </summary>
    public class DamageContext
    {
        /// <summary>The attacking entity</summary>
        public BattleEntity Attacker;

        /// <summary>The defending entity (set by DamageSystem at application time)</summary>
        public BattleEntity Defender;

        /// <summary>Attacker's stats (contains "Attack" etc.)</summary>
        public StatComponent AttackerStats;

        /// <summary>Weapon stats if applicable (contains "Damage" etc.)</summary>
        public StatComponent WeaponStats;

        /// <summary>Bullet stats if applicable (contains "Damage" etc.)</summary>
        public StatComponent BulletStats;

        public Vector3 Direction;
    }
}
