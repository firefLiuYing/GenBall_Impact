using System;
using GenBall.BattleSystem.Character;
using GenBall.Player;

namespace GenBall.BattleSystem.Weapons
{
    [Obsolete]
    public interface IWeaponTriggerController
    {
        public void Init(WeaponState weapon);
        public void Trigger(ButtonState  buttonState);
    }
}