using GenBall.Player;
using GenBall.BattleSystem.Weapons.Components.Ammo;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public class ShotgunTriggerBehavior : ITriggerBehavior
    {
        private readonly int _pelletCount;
        private readonly float _spreadAngle;

        public ShotgunTriggerBehavior(int pelletCount = 8, float spreadAngle = 15f)
        {
            _pelletCount = pelletCount;
            _spreadAngle = spreadAngle;
        }

        public FireRequest? Evaluate(ButtonState currentState, bool stateChangedThisFrame, float deltaTime, IAmmoSystem ammo)
        {
            if (!stateChangedThisFrame || currentState != ButtonState.Down)
                return null;
            return new FireRequest { BulletCount = _pelletCount, DamageMultiplier = 1f, SpreadAngle = _spreadAngle };
        }
    }
}
