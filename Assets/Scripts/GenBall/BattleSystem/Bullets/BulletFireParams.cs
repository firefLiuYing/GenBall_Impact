using UnityEngine;

namespace GenBall.BattleSystem.Bullets
{
    /// <summary>
    /// Runtime parameters passed when firing a bullet.
    /// Contains both immutable references and buff-modified override values
    /// computed by WeaponFireExecutor at fire time.
    /// </summary>
    public struct BulletFireParams
    {
        /// <summary>BulletConfig Id to look up from BulletConfigCollection.</summary>
        public BulletId ConfigId;

        /// <summary>Logic spawn point — where hit detection starts (e.g. Camera.main.position for FPS).</summary>
        public Vector3 LogicOrigin;

        /// <summary>Visual spawn point — where the visual bullet appears (e.g. gun muzzle position).</summary>
        public Vector3 VisualOrigin;

        /// <summary>Initial direction for both logic and visual.</summary>
        public Vector3 Direction;

        /// <summary>The GameObject that fired this bullet (player weapon, enemy, etc.). Can be null.</summary>
        public GameObject Source;

        // ── Buff-modified override values ──
        // These are computed by WeaponFireExecutor from weapon.Stats (which includes Buff modifiers).

        /// <summary>Final damage value after all buff/modifier calculations.</summary>
        public int FinalDamage;

        /// <summary>Final speed after buff multipliers.</summary>
        public float FinalSpeed;

        /// <summary>Final collision radius after buff modifiers.</summary>
        public float FinalRadius;

        /// <summary>Extra penetrations added by buffs (on top of BulletConfig base).</summary>
        public int ExtraPenetrations;

        /// <summary>Extra bounces added by buffs (on top of BulletConfig base).</summary>
        public int ExtraBounces;

        /// <summary>Speed multiplier from buffs (applied on top of FinalSpeed).</summary>
        public float SpeedMultiplier;
    }
}
