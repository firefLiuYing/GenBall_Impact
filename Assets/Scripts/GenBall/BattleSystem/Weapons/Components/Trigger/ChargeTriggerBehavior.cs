using GenBall.Player;
using GenBall.BattleSystem.Weapons.Components.Ammo;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public class ChargeTriggerBehavior : ITriggerBehavior
    {
        private float _chargeTime;
        private readonly float _maxChargeTime;
        private readonly float _maxDamageMultiplier;

        public ChargeTriggerBehavior(float maxChargeTime = 2f, float maxDamageMultiplier = 3f)
        {
            _maxChargeTime = maxChargeTime;
            _maxDamageMultiplier = maxDamageMultiplier;
        }

        public float ChargeProgress => _maxChargeTime > 0f ? Mathf.Min(_chargeTime / _maxChargeTime, 1f) : 0f;

        public FireRequest? Evaluate(ButtonState currentState, bool stateChangedThisFrame, float deltaTime, IAmmoSystem ammo)
        {
            if (currentState == ButtonState.Hold || (currentState == ButtonState.Down && stateChangedThisFrame))
            {
                _chargeTime += deltaTime;
                return null;
            }

            if (currentState == ButtonState.Up && _chargeTime > 0f)
            {
                float t = Mathf.Min(_chargeTime / _maxChargeTime, 1f);
                float dmg = 1f + (_maxDamageMultiplier - 1f) * t;
                _chargeTime = 0f;
                return new FireRequest { BulletCount = 1, DamageMultiplier = dmg };
            }

            _chargeTime = 0f;
            return null;
        }
    }
}
