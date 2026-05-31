using GenBall.AbilityWeapon;

namespace GenBall.AbilityWeapon.StackGun
{
    public class StackGunConfig : IAbilityWeaponConfig
    {
        public AbilityWeaponId Id => AbilityWeaponId.StackGun;
        public float CooldownSeconds => 10f;
        public string DisplayName => "匣纳之枪";
        public string IconResourcePath => "Assets/AssetBundles/UI/MainHud/Sprites/New/HalfHeart@4x.png";
    }
}
