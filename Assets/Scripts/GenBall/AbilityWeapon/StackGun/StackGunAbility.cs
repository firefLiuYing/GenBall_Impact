using System.Collections.Generic;
using GenBall.BattleSystem.Framework;
using GenBall.Player;
using UnityEngine;

namespace GenBall.AbilityWeapon.StackGun
{
    public class StackGunAbility : IAbilityWeapon
    {
        private readonly Stack<GameObject> _orbStack = new();
        private const int MaxCapacity = 2;
        private bool _isShooting;
        private bool _wasShooting;

        public bool IsExhausted { get; private set; }
        public IAbilityWeaponConfig Config { get; } = new StackGunConfig();

        public void Activate(BattleEntity player)
        {
            _orbStack.Clear();
            _isShooting = false;
            _wasShooting = false;
            IsExhausted = false;
        }

        public void Deactivate()
        {
            _orbStack.Clear();
        }

        public void HandlePrimary(ButtonState state)
        {
            if (state == ButtonState.Down)
            {
                if (!_isShooting)
                {
                    _isShooting = true;
                    return;
                }

                if (_orbStack.Count > 0)
                {
                    var orb = _orbStack.Pop();
                    Debug.Log("[StackGun] Fired orb!");
                    IsExhausted = _orbStack.Count == 0;
                    if (IsExhausted)
                    {
                        _isShooting = false;
                    }
                }
                else
                {
                    IsExhausted = true;
                    _isShooting = false;
                }
            }
        }

        public void HandleSecondary(ButtonState state)
        {
            if (state == ButtonState.Down)
            {
                if (!_isShooting && _orbStack.Count < MaxCapacity)
                {
                    Debug.Log("[StackGun] Absorbed orb!");
                }
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            // Placeholder — orb physics/size updates go here
        }
    }
}
