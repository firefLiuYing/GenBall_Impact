using System;
using GenBall.BattleSystem;

namespace GenBall.Player
{
    public partial class Player:IAttacker
    {
        private IWeapon _physicsWeapon;

        internal void PhysicsWeaponTrigger(ButtonState triggerState)=>_physicsWeapon?.Trigger(triggerState);

        internal void EquipPhysicsWeapon(IWeapon newWeapon)
        {
            if (_physicsWeapon != null)
            {
                throw new Exception("has already been equipped");
            }

            _physicsWeapon = newWeapon;
            newWeapon.OnEquip(this);
        }

        internal void UnequipPhysicsWeapon()
        {
            if (_physicsWeapon == null)
            {
                throw new Exception("has not been equipped");
            }
            _physicsWeapon.OnUnequip();
        }
    }
}