using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Weapons;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: executes weapon commands (Attack, Reload, SwitchWeapon).
    /// Registered directly on CommandDispatcherComponent.
    ///
    /// MonoBehaviour is kept for the serialized weaponSpawnPoint reference — the Transform
    /// is carefully positioned on the prefab and cannot be found by path in a robust way.
    /// </summary>
    public class WeaponExecutor : MonoBehaviour, IAttack, IReload, ISwitchWeapon
    {
        [SerializeField] private Transform weaponSpawnPoint;

        /// <summary>Exposed for PlayerEntityFactory to read without Find().</summary>
        public Transform WeaponSpawnPoint => weaponSpawnPoint;

        private GameObject _playerGo;
        private WeaponState _currentWeapon;
        private IEvolutionSystem _evolution;

        public bool IsAttacking => false;
        public bool IsReloading => false;
        public bool IsSwitching => false;

        public void Init(GameObject playerGo)
        {
            _playerGo = playerGo;
            _evolution = SystemRepository.Instance.GetSystem<IEvolutionSystem>();
            // Equip(1);
        }

        // ---- IAttack ----

        public void Attack(AttackCommand cmd)
        {
            _currentWeapon?.Trigger(cmd.TriggerState);
        }

        public void Cancel() { }

        // ---- IReload ----

        public void Reload(ReloadCommand cmd)
        {
            _currentWeapon?.Reload(ButtonState.Down);
        }

        // ---- ISwitchWeapon ----

        public void SwitchWeapon(SwitchWeaponCommand cmd)
        {
            if (_evolution == null || !_evolution.CanEvolve) return;
            Equip(_evolution.CurrentEvolutionLevel + 1);
        }

        private void Equip(int level)
        {
            var equipInfo = _evolution.GetEquipInfo(level);
            if (equipInfo == null) return;
            _evolution.CurrentEvolutionLevel = level;
            Equip(equipInfo);
        }

        private void Equip(EquipInfo info)
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.OnUnequip();
                Object.Destroy(_currentWeapon.gameObject);
                _currentWeapon = null;
            }

            _currentWeapon = info.WeaponId.Create();
            if (_currentWeapon == null) return;
            _currentWeapon.transform.SetParent(weaponSpawnPoint, false);
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.Init(_playerGo);

            if (info.Accessories == null) return;
            foreach (var accessory in info.Accessories)
            {
#if UNITY_EDITOR
                var model = AccessoryModelConfigProvider.GetOrCreateConfig().GetModel(accessory);
#else
                var model = new AccessoryModel();
#endif
                var obj = AccessoryObj.Create(model);
                _currentWeapon.AddAccessory(obj);
            }
        }
    }
}
