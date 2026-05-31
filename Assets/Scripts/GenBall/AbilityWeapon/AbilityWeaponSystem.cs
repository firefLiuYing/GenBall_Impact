using System;
using System.Collections.Generic;
using GenBall.AbilityWeapon.StackGun;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.Event;
using GenBall.Player.Executor;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.AbilityWeapon
{
    public class AbilityWeaponSystem : IAbilityWeaponSystem, ILogicUpdate
    {
        private enum State
        {
            Idle,
            Hiding,
            AbilityActive,
            Showing
        }

        private State _state = State.Idle;

        // ── Bound references ──
        private BattleEntity _playerEntity;
        private CommandDispatcherComponent _dispatcher;
        private object _weaponAttackExecutor;
        private object _visibilityExecutor;

        // ── Weapons ──
        private readonly Dictionary<AbilityWeaponId, IAbilityWeapon> _weapons = new();
        private AbilityWeaponId? _activeWeaponId;
        private AbilityWeaponId? _pendingWeaponId;
        private IAbilityWeapon _activeWeapon;
        private readonly CooldownTracker _cooldownTracker = new();

        private AbilityWeaponExecutor _abilityExecutor;

        // ── Event subscriptions ──
        private Action _onCombatStateChanged;

        public bool IsAnyActive => _state == State.AbilityActive;
        public AbilityWeaponId? ActiveWeaponId => _activeWeaponId;

        public IReadOnlyList<AbilityWeaponId> AvailableWeaponIds
        {
            get
            {
                var ids = new List<AbilityWeaponId>(_weapons.Keys);
                return ids;
            }
        }

        public SystemScope LogicUpdateScope => SystemScope.Game;

        // ================================================================
        // ISystem
        // ================================================================

        public void Init()
        {
            _weapons[AbilityWeaponId.StackGun] = new StackGunAbility();
            _abilityExecutor = new AbilityWeaponExecutor();

            _onCombatStateChanged = () => _cooldownTracker.ResetAll();
            CEventRouter.Instance.Subscribe((int)GlobalEventId.CombatStateChanged, _onCombatStateChanged);
        }

        public void UnInit()
        {
            if (_onCombatStateChanged != null)
            {
                CEventRouter.Instance.Unsubscribe((int)GlobalEventId.CombatStateChanged, _onCombatStateChanged);
                _onCombatStateChanged = null;
            }
        }

        // ================================================================
        // Binding
        // ================================================================

        public void BindPlayer(BattleEntity playerEntity, object weaponAttackExecutor, object visibilityExecutor)
        {
            _playerEntity = playerEntity;
            _dispatcher = playerEntity?.Get<CommandDispatcherComponent>();
            _weaponAttackExecutor = weaponAttackExecutor;
            _visibilityExecutor = visibilityExecutor;
        }

        // ================================================================
        // Cooldown
        // ================================================================

        public float GetCooldownRemaining(AbilityWeaponId weaponId)
        {
            return _cooldownTracker.GetRemaining(weaponId);
        }

        public IAbilityWeaponConfig GetWeaponConfig(AbilityWeaponId weaponId)
        {
            if (_weapons.TryGetValue(weaponId, out var weapon))
                return weapon.Config;
            return null;
        }

        // ================================================================
        // Activate / Deactivate (public, called by WheelExecutor)
        // ================================================================

        public void ActivateWeapon(AbilityWeaponId weaponId)
        {
            if (_cooldownTracker.HasCooldown(weaponId)) return;
            if (!_weapons.TryGetValue(weaponId, out var weapon)) return;

            _pendingWeaponId = weaponId;
            _activeWeapon = weapon;

            _dispatcher?.Issue(new WeaponVisibilityCommand(false));
            _state = State.Hiding;
        }

        public void DeactivateWeapon()
        {
            if (_activeWeapon != null)
            {
                _activeWeapon.Deactivate();

                var config = _activeWeapon.Config;
                if (config != null && config.CooldownSeconds > 0f)
                {
                    _cooldownTracker.StartCooldown(config.Id, config.CooldownSeconds);
                    CEventRouter.Instance.FireNow((int)GlobalEventId.AbilityCooldownChanged, config.Id);
                }
            }

            _dispatcher?.UnregisterExecutor<AttackCommand>();
            _dispatcher?.UnregisterExecutor<AbilitySecondaryCommand>();
            if (_weaponAttackExecutor != null)
            {
                _dispatcher?.RegisterExecutor<AttackCommand>(_weaponAttackExecutor);
            }

            _dispatcher?.Issue(new WeaponVisibilityCommand(true));
            _state = State.Showing;

            CEventRouter.Instance.FireNow((int)GlobalEventId.AbilityWeaponDeactivated);
        }

        // ================================================================
        // ILogicUpdate
        // ================================================================

        public void LogicUpdate(float deltaTime)
        {
            switch (_state)
            {
                case State.Hiding:
                    if (!IsVisibilityTransitioning())
                    {
                        _dispatcher?.UnregisterExecutor<AttackCommand>();
                        _dispatcher?.RegisterExecutor<AttackCommand>(_abilityExecutor);
                        _dispatcher?.RegisterExecutor<AbilitySecondaryCommand>(_abilityExecutor);

                        _abilityExecutor.SetActiveWeapon(_activeWeapon);
                        _activeWeapon?.Activate(_playerEntity);

                        _activeWeaponId = _pendingWeaponId;
                        _state = State.AbilityActive;

                        if (_activeWeaponId.HasValue)
                            CEventRouter.Instance.FireNow((int)GlobalEventId.AbilityWeaponActivated, _activeWeaponId.Value);
                    }
                    break;

                case State.AbilityActive:
                    if (_activeWeapon != null)
                    {
                        _activeWeapon.LogicUpdate(deltaTime);

                        if (_activeWeapon.IsExhausted)
                        {
                            DeactivateWeapon();
                        }
                    }
                    break;

                case State.Showing:
                    if (!IsVisibilityTransitioning())
                    {
                        _activeWeapon = null;
                        _activeWeaponId = null;
                        _abilityExecutor.SetActiveWeapon(null);
                        _state = State.Idle;
                    }
                    break;
            }

            _cooldownTracker.Tick(deltaTime);
        }

        // ================================================================
        // Helpers
        // ================================================================

        private bool IsVisibilityTransitioning()
        {
            return (_visibilityExecutor as IWeaponVisibility)?.IsTransitioning ?? false;
        }

        // ================================================================
        // CooldownTracker
        // ================================================================

        private class CooldownTracker
        {
            private readonly Dictionary<AbilityWeaponId, float> _cooldowns = new();

            public void Tick(float deltaTime)
            {
                if (_cooldowns.Count == 0) return;
                var keys = new List<AbilityWeaponId>(_cooldowns.Keys);
                foreach (var key in keys)
                {
                    _cooldowns[key] -= deltaTime;
                    if (_cooldowns[key] <= 0f)
                        _cooldowns.Remove(key);
                }
            }

            public void StartCooldown(AbilityWeaponId id, float seconds)
            {
                _cooldowns[id] = seconds;
            }

            public void ResetAll() => _cooldowns.Clear();

            public void Reset(AbilityWeaponId id) => _cooldowns.Remove(id);

            public float GetRemaining(AbilityWeaponId id)
            {
                return _cooldowns.TryGetValue(id, out var remaining) ? Mathf.Max(0f, remaining) : 0f;
            }

            public bool HasCooldown(AbilityWeaponId id) => _cooldowns.ContainsKey(id);
        }
    }
}
