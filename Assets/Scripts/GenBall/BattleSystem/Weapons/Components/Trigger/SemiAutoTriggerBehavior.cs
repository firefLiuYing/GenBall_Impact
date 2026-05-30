using GenBall.Player;
using GenBall.BattleSystem.Weapons.Components.Ammo;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public class SemiAutoTriggerBehavior : ITriggerBehavior
    {
        public FireRequest? Evaluate(ButtonState currentState, bool stateChangedThisFrame, float deltaTime, IAmmoSystem ammo)
        {
            if (!stateChangedThisFrame || currentState != ButtonState.Down)
                return null;
            return new FireRequest { BulletCount = 1, DamageMultiplier = 1f };
        }
    }
}
