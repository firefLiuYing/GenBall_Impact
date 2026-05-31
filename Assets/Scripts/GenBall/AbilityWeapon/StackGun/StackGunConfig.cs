using GenBall.AbilityWeapon;

namespace GenBall.AbilityWeapon.StackGun
{
    public class StackGunConfig : IAbilityWeaponConfig
    {
        public AbilityWeaponId Id => AbilityWeaponId.StackGun;
        public float CooldownSeconds => 10f;
        public string DisplayName => "匣纳之枪";
    }
}
