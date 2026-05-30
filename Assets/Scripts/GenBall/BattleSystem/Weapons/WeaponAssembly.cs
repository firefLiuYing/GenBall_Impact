using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    /// <summary>
    /// Placed on a weapon prefab to define which components the weapon BattleEntity
    /// should be assembled with, and their parameters.
    /// The WeaponEntityFactory reads this to assemble the weapon at runtime.
    /// </summary>
    public class WeaponAssembly : MonoBehaviour
    {
        [Header("Archetype")]
        public WeaponArchetype Archetype = WeaponArchetype.SemiAutoPistol;
        public AmmoArchetype AmmoType = AmmoArchetype.Magazine;

        [Header("Trigger — SemiAuto / FullAuto common")]
        public float Damage = 10f;
        public float FireInterval = 0.1f;

        [Header("Trigger — Shotgun")]
        public int ShotgunPelletCount = 8;
        public float ShotgunSpreadAngle = 15f;

        [Header("Trigger — Charge")]
        public float ChargeMaxTime = 2f;
        public float ChargeMaxDamageMult = 3f;

        [Header("Spread")]
        public float SpreadBase;
        public float SpreadMoving = 5f;

        [Header("Ammo — Magazine")]
        public int MagazineCapacity = 30;
        public float ReloadTime = 2f;

        [Header("Ammo — Heat")]
        public float MaxHeat = 100f;
        public float HeatPerShot = 10f;
        public float HeatCoolRate = 20f;

        [Header("References")]
        public Transform BulletSpawnPoint;
    }

    public enum WeaponArchetype
    {
        SemiAutoPistol,
        FullAutoRifle,
        Shotgun,
        ChargeLaser
    }

    public enum AmmoArchetype
    {
        Magazine,
        Heat,
        Infinite
    }
}
