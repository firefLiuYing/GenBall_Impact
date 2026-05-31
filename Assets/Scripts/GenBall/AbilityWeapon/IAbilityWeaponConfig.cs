namespace GenBall.AbilityWeapon
{
    public interface IAbilityWeaponConfig
    {
        AbilityWeaponId Id { get; }
        float CooldownSeconds { get; }
        string DisplayName { get; }
    }
}
