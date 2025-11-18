using System;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Weapons;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Resource;

namespace GenBall.Player
{
    public partial class Player:IAttacker
    {
        // private WeaponCreator WeaponCreator => GameEntry.GetModule<WeaponCreator>();
        private EntityCreator<IWeapon> WeaponCreator => GameEntry.GetModule<EntityCreator<IWeapon>>();
        private IWeapon _physicsWeapon;
        [SerializeField] private Transform weaponSpawnPoint;

        // private void WeaponsUpdate(float deltaTime)
        // {
        //     _physicsWeapon?.WeaponUpdate(deltaTime);
        // }

        internal void PhysicsWeaponTrigger(ButtonState triggerState)=>_physicsWeapon?.Trigger(triggerState);
        internal void EquipPhysicsWeapon<TWeapon>() where TWeapon : IWeapon
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon<TWeapon>();
        }
        internal void EquipPhysicsWeapon<TWeapon>(string name) where TWeapon : IWeapon
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon<TWeapon>(name);
        }
        private void InternalEquipPhysicsWeapon<TWeapon>() where TWeapon : IWeapon
        {
            var weapon = WeaponCreator.CreateEntity<TWeapon>(weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }
        private void InternalEquipPhysicsWeapon<TWeapon>(string name) where TWeapon : IWeapon
        {
            var weapon = WeaponCreator.CreateEntity<TWeapon>(weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }
        private void InternalEquipPhysicsWeapon(IWeapon newWeapon)
        {
            if (_physicsWeapon != null)
            {
                throw new Exception("has already been equipped");
            }

            _physicsWeapon = newWeapon;
            newWeapon.OnEquip(this);
        }

        private void UnequipPhysicsWeapon()
        {
            if (_physicsWeapon == null)
            {
                throw new Exception("has not been equipped");
            }
            _physicsWeapon.OnUnequip();
            if(_physicsWeapon is not MonoBehaviour monoBehaviour)return;
            WeaponCreator.RecycleEntity(monoBehaviour.gameObject);
        }
    }
}