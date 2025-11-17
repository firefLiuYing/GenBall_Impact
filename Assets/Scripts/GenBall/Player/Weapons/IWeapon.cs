using System.Numerics;
using GenBall.BattleSystem;

namespace GenBall.Player
{
    public interface IWeapon
    {
        public void Trigger(ButtonState triggerState);
        public void OnEquip(IAttacker owner);
        public void OnUnequip();
    }
}