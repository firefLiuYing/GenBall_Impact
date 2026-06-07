namespace GenBall.AbilityWeapon.StackGun
{
    public class StackGunConfig : IAbilityWeaponConfig
    {
        public AbilityWeaponId Id => AbilityWeaponId.StackGun;
        public float CooldownSeconds => 10f;
        public string DisplayName => "匣纳之枪";
        public string IconResourcePath => "Assets/AssetBundles/UI/MainHud/Sprites/New/HalfHeart@4x.png";

        // --- Absorption ---
        public int MaxCapacity = 2;
        public float AbsorbRadius = 3f;
        public float AbsorbAngle = 60f; // half-angle of the absorb cone

        // --- Projectile ---
        public float ProjectileSpeed = 15f;
        public float ArmTime = 0.3f;
        public float ProjectileLifetime = 5f;
        public int BaseDamage = 20;
        public float KnockbackForce = 10f;
    }
}
