namespace GenBall.BattleSystem.AbilityWeapon
{
    public interface IAbilityWeaponConfig
    {
        AbilityWeaponId Id { get; }
        float CooldownSeconds { get; }
        string DisplayName { get; }
        string IconResourcePath { get; }
    }
}
