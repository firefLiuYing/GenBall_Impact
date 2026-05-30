using GenBall.Player;
using GenBall.BattleSystem.Weapons.Components.Ammo;

namespace GenBall.BattleSystem.Weapons.Components.Trigger
{
    public interface ITriggerBehavior
    {
        FireRequest? Evaluate(ButtonState currentState, bool stateChangedThisFrame, float deltaTime, IAmmoSystem ammo);
    }

    public struct FireRequest
    {
        public int BulletCount;
        public float DamageMultiplier;
        public float SpreadAngle;
    }
}
