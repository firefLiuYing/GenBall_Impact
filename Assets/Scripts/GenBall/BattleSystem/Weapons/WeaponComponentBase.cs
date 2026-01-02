using UnityEngine;

namespace GenBall.BattleSystem.Weapons
{
    public abstract class WeaponComponentBase : MonoBehaviour, IWeaponComponent
    {
        public IWeapon Owner { get;private set; }
        public void Equip(IWeapon owner)
        {
            Owner = owner;
            OnEquip();
        }
        protected virtual void OnEquip(){}

        public void Unequip()
        {
            OnUnequip();
            Owner = null;
        }
        protected virtual void OnUnequip(){}
    }
}