using GenBall.BattleSystem.Framework;
using GenBall.Framework.Entity;

namespace GenBall.BattleSystem.Weapons.Components.Ammo
{
    /// <summary>
    /// Manages magazine reload as a state machine. AmmoCount/MagazineCapacity/ReloadTime
    /// live in StatComponent. WeaponFireDecision reads AmmoCount directly.
    /// </summary>
    public class WeaponMagazineExecutor : IAmmoSystem, IEntityLogicUpdate
    {
        private readonly BattleEntity _weapon;
        private bool _isReloading;
        private float _reloadTimer;

        private const string StatAmmo = "AmmoCount";
        private const string StatCapacity = "MagazineCapacity";
        private const string StatReloadTime = "ReloadTime";

        public WeaponMagazineExecutor(BattleEntity weapon)
        {
            _weapon = weapon;
        }

        private StatComponent Stats => _weapon?.Get<StatComponent>();
        private int Ammo => (int)(Stats?.GetValue(StatAmmo) ?? 0);
        private int Capacity => (int)(Stats?.GetValue(StatCapacity) ?? 0);
        private float ReloadDuration => Stats?.GetValue(StatReloadTime) ?? 2f;

        // Called by WeaponAttackExecutor on ReloadCommand
        public void Reload()
        {
            if (_isReloading) return;
            if (Ammo >= Capacity) return;
            _isReloading = true;
            _reloadTimer = 0f;
        }

        public bool IsReloading => _isReloading;

        // ======== IEntityLogicUpdate ========

        public void LogicUpdate(float deltaTime)
        {
            if (!_isReloading) return;

            _reloadTimer += deltaTime;
            if (_reloadTimer >= ReloadDuration)
            {
                _isReloading = false;
                Stats?.SetBase(StatAmmo, Capacity);
            }
        }

        // ======== IAmmoSystem ========

        public AmmoDisplayInfo GetDisplayInfo()
        {
            return new AmmoDisplayInfo
            {
                Type = _isReloading ? AmmoDisplayType.Charge : AmmoDisplayType.Magazine,
                CurrentValue = Ammo,
                MaxValue = Capacity,
                NormalizedValue = Capacity > 0 ? (float)Ammo / Capacity : 0f
            };
        }
    }
}
