using System;

namespace GenBall.BattleSystem.Weapons
{
    [Obsolete]
    public interface IWeaponComponent
    {
        public IWeapon Owner { get; }
        public void Equip(IWeapon owner);
        public void Unequip();
    }
}