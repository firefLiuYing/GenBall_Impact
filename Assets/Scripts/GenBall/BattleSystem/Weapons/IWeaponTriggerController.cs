using GenBall.Player;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeaponTriggerController
    {
        public void Trigger(ButtonState  buttonState);
    }
}