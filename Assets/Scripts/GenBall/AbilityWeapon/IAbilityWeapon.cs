using GenBall.BattleSystem.Framework;
using GenBall.Player;

namespace GenBall.AbilityWeapon
{
    public interface IAbilityWeapon
    {
        void Activate(BattleEntity player);
        void Deactivate();
        void HandlePrimary(ButtonState state);
        void HandleSecondary(ButtonState state);
        bool IsExhausted { get; }
        IAbilityWeaponConfig Config { get; }
        void LogicUpdate(float deltaTime);
    }
}
