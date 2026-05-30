using GenBall.Player;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public interface IWeaponTrigger
    {
        void SetTriggerState(ButtonState newState);
        bool IsFiring { get; }
    }
}
