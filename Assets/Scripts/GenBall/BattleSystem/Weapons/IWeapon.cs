using GenBall.Player;
using GenBall.Utils.EntityCreator;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public interface IWeapon:IEntity
    {
        public IAttacker Owner { get; }
        public void Trigger(ButtonState triggerState);
        public void OnEquip(IAttacker owner);
        public void OnUnequip();
        // public IWeaponStats BaseStats { get; }
        // public IWeaponStats AdditionStats { get; set;}
        // public IWeaponStats FinalStats { get; }
        public bool AddAccessory(Accessory.Accessory accessory);
        public bool RemoveAccessory(Accessory.Accessory accessory);
    }
}