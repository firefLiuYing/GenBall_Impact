using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Weapons.Components.Ammo;
using GenBall.BattleSystem.Weapons.Components.Trigger;
using GenBall.Framework.Entity;
using GenBall.Player;

namespace GenBall.BattleSystem.Weapons.Components
{
    /// <summary>
    /// Weapon-internal Decision layer.
    /// SetTriggerState evaluates immediately on state change for point-fire weapons.
    /// LogicUpdate handles continuous FullAuto fire and cooldown/ammo ticks.
    /// </summary>
    public class WeaponFireDecision : IWeaponTrigger, IEntityLogicUpdate
    {
        private readonly BattleEntity _weapon;
        private ITriggerBehavior _behavior;
        private IAmmoSystem _ammo;

        private ButtonState _currentState = ButtonState.None;
        private ButtonState _previousState = ButtonState.None;
        private float _cooldownTimer;

        private const string StatFireInterval = "FireInterval";
        private const string StatAmmo = "AmmoCount";
        private const string StatHeatPerShot = "HeatPerShot";

        private float FireInterval => _weapon.Get<StatComponent>()?.GetValue(StatFireInterval) ?? 0.1f;

        public WeaponFireDecision(BattleEntity weapon, ITriggerBehavior behavior)
        {
            _weapon = weapon;
            _behavior = behavior;
        }

        public void SetBehavior(ITriggerBehavior behavior) => _behavior = behavior;

        // ======== IWeaponTrigger ========

        public void SetTriggerState(ButtonState newState)
        {
            if (newState == _currentState) return;

            _previousState = _currentState;
            _currentState = newState;

            ResolveAmmo();

            // Immediate fire on state change (SemiAuto/Shotgun on Down, Charge on Up)
            var request = _behavior?.Evaluate(_currentState, stateChangedThisFrame: true, 0f, _ammo);
            if (request != null)
            {
                ExecuteFire(request.Value);
                _cooldownTimer = 0f;
            }
        }

        // Firing does not block movement — old WeaponExecutor returned false always.
        public bool IsFiring => false;

        // ======== IEntityLogicUpdate ========

        public void LogicUpdate(float deltaTime)
        {
            _cooldownTimer += deltaTime;

            if (_cooldownTimer < FireInterval)
                return;

            ResolveAmmo();

            bool stateChanged = _currentState != _previousState;
            var request = _behavior?.Evaluate(_currentState, stateChanged, deltaTime, _ammo);

            _previousState = _currentState;

            if (request != null)
            {
                ExecuteFire(request.Value);
                _cooldownTimer = 0f;
            }
        }

        // ======== Internal ========

        private void ExecuteFire(FireRequest request)
        {
            // Check ammo from stats directly (not through interface)
            var stats = _weapon.Get<StatComponent>();
            if (stats == null) return;

            if (stats.HasStat(StatAmmo))
            {
                // Magazine weapon: check AmmoCount
                int ammo = (int)stats.GetValue(StatAmmo);
                if (ammo < request.BulletCount) return;
                stats.SetBase(StatAmmo, ammo - request.BulletCount);
            }
            else if (stats.HasStat(StatHeatPerShot))
            {
                // Heat weapon: check heat capacity
                if (_weapon.TryGet<HeatComponent>(out var heat))
                {
                    if (!heat.CanFire) return;
                    heat.AddHeat();
                }
            }
            // Infinite ammo: no check needed

            _weapon.Get<WeaponFireExecutor>()?.Fire(request);
        }

        private void ResolveAmmo()
        {
            if (_ammo == null)
                _weapon.TryGet<IAmmoSystem>(out _ammo);
        }
    }
}
