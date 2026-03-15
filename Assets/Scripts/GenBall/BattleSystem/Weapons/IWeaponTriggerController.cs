using GenBall.BattleSystem.Character;
using GenBall.Player;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeaponTriggerController
    {
        public void Init(WeaponState weapon);
        public void Trigger(ButtonState  buttonState);
    }
}