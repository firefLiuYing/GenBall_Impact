using System;
using GenBall.Player;

namespace GenBall.BattleSystem.Weapons
{
    [Obsolete]
    public interface IWeaponReloadController
    {
        public void Init(WeaponState weapon);
        public void Reload(ButtonState  button);
    }

    public struct MagazineInfo
    {
        public int AmmunitionCount;
        public int Capacity;
    }
}