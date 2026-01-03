using System;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Weapons;
using GenBall.Utils.EntityCreator;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Resource;

namespace GenBall.Player
{
    public partial class Player:IAttacker
    {
        // private WeaponCreator WeaponCreator => GameEntry.GetModule<WeaponCreator>();
        private EntityCreator<IWeapon> WeaponCreator => GameEntry.GetModule<EntityCreator<IWeapon>>();
        private IWeapon _physicsWeapon;
        public IWeapon PhysicsWeapon => _physicsWeapon;
        [SerializeField] private Transform weaponSpawnPoint;

        // private void WeaponsUpdate(float deltaTime)
        // {
        //     _physicsWeapon?.WeaponUpdate(deltaTime);
        // }

        private void PhysicsWeaponTrigger(ButtonState triggerState)=>_physicsWeapon?.Trigger(triggerState);
        public IWeapon EquipPhysicsWeapon<TWeapon>() where TWeapon : IWeapon
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon<TWeapon>();
            return _physicsWeapon;
        }

        public IWeapon EquipPhysicsWeapon(Type type)
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon(type);
            return _physicsWeapon;
        }
        public IWeapon EquipPhysicsWeapon<TWeapon>(string name) where TWeapon : IWeapon
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon<TWeapon>(name);
            return _physicsWeapon;
        }

        public IWeapon EquipPhysicsWeapon(string name, [NotNull] Type type)
        {
            if (_physicsWeapon != null)
            {
                UnequipPhysicsWeapon();
            }
            InternalEquipPhysicsWeapon(name, type);
            return _physicsWeapon;
        }
        private void InternalEquipPhysicsWeapon<TWeapon>() where TWeapon : IWeapon
        {
            var weapon = WeaponCreator.CreateEntity<TWeapon>(weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }

        private void InternalEquipPhysicsWeapon([NotNull] Type type)
        {
            var weapon = WeaponCreator.CreateEntity(type, weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }
        private void InternalEquipPhysicsWeapon<TWeapon>(string name) where TWeapon : IWeapon
        {
            var weapon = WeaponCreator.CreateEntity<TWeapon>(name,weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }

        private void InternalEquipPhysicsWeapon(string name, [NotNull] Type type)
        {
            var weapon = WeaponCreator.CreateEntity(name, type, weaponSpawnPoint);
            InternalEquipPhysicsWeapon(weapon);
        }
        private void InternalEquipPhysicsWeapon(IWeapon newWeapon)
        {
            if (_physicsWeapon != null)
            {
                throw new Exception("has already been equipped");
            }

            _physicsWeapon = newWeapon;
            newWeapon.Equip(this);
        }

        private void UnequipPhysicsWeapon()
        {
            if (_physicsWeapon == null)
            {
                throw new Exception("has not been equipped");
            }
            _physicsWeapon.Unequip();
            if(_physicsWeapon is not MonoBehaviour monoBehaviour)return;
            WeaponCreator.RecycleEntity(monoBehaviour.gameObject);
            _physicsWeapon=null;
        }

    }
}