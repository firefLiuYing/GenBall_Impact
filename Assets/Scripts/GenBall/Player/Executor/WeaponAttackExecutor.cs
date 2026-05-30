using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Weapons.Components.Ammo;
using GenBall.BattleSystem.Weapons.Components.Trigger;
using GenBall.BattleSystem.Weapons.Factory;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Player-side Executor layer for weapon-related commands.
    /// Routes AttackCommand to weapon's IWeaponTrigger,
    /// ReloadCommand to weapon's WeapnMagazineExecutor.
    /// </summary>
    public class WeaponAttackExecutor : IAttack, IReload, ISwitchWeapon
    {
        private BattleEntity _weaponEntity;
        private IWeaponTrigger _trigger;
        private readonly Transform _weaponSpawnPoint;
        private readonly BattleEntity _playerEntity;

        public WeaponAttackExecutor(BattleEntity playerEntity, Transform weaponSpawnPoint)
        {
            _playerEntity = playerEntity;
            _weaponSpawnPoint = weaponSpawnPoint;
        }

        // ============ Weapon Management ============

        public void EquipWeapon(BattleEntity weaponEntity)
        {
            _weaponEntity = weaponEntity;
            _weaponEntity?.TryGet<IWeaponTrigger>(out _trigger);
        }

        public void UnequipWeapon()
        {
            _trigger = null;
            _weaponEntity = null;
        }

        public BattleEntity CurrentWeapon => _weaponEntity;

        // ============ IAttack ============

        public bool IsAttacking => _trigger?.IsFiring ?? false;

        public void Attack(AttackCommand cmd)
        {
            _trigger?.SetTriggerState(cmd.TriggerState);
        }

        public void Cancel()
        {
            _trigger?.SetTriggerState(ButtonState.Up);
        }

        // ============ IReload ============

        public bool IsReloading
        {
            get
            {
                if (_weaponEntity != null && _weaponEntity.TryGet<WeaponMagazineExecutor>(out var mag))
                    return mag.IsReloading;
                return false;
            }
        }

        public void Reload(ReloadCommand cmd)
        {
            if (_weaponEntity != null && _weaponEntity.TryGet<WeaponMagazineExecutor>(out var mag))
                mag.Reload();
        }

        // ============ ISwitchWeapon ============

        public bool IsSwitching => false; // TODO: equip animation

        public void SwitchWeapon(SwitchWeaponCommand cmd)
        {
            // TODO: 进化系统 + 配件系统重新设计后完成武器切换流程
            // var evolution = SystemRepository.Instance.GetSystem<IEvolutionSystem>();
            // var equipInfo = evolution.GetEquipInfo(level);
            // Destroy old → CreateWeapon → EquipWeapon
            Debug.Log("[WeaponAttackExecutor] SwitchWeapon: TODO — evolution system redesign pending");
        }
    }
}
