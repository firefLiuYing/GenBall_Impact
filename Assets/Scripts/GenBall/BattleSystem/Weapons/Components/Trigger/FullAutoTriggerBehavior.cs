using GenBall.Player;
using GenBall.BattleSystem.Weapons.Components.Ammo;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public class FullAutoTriggerBehavior : ITriggerBehavior
    {
        public FireRequest? Evaluate(ButtonState currentState, bool stateChangedThisFrame, float deltaTime, IAmmoSystem ammo)
        {
            if (currentState != ButtonState.Down && currentState != ButtonState.Hold)
                return null;
            return new FireRequest { BulletCount = 1, DamageMultiplier = 1f };
        }
    }
}
