using GenBall.AbilityWeapon;
using GenBall.BattleSystem.Command;
using GenBall.Player;

namespace GenBall.Player.Executor
{
    public class AbilityWeaponExecutor : IAttack, IAbilitySecondary
    {
        private IAbilityWeapon _activeWeapon;

        public void SetActiveWeapon(IAbilityWeapon weapon) => _activeWeapon = weapon;
        public bool IsAttacking => _activeWeapon != null && !_activeWeapon.IsExhausted;

        public void Attack(AttackCommand cmd) => _activeWeapon?.HandlePrimary(cmd.TriggerState);

        public void AbilitySecondary(AbilitySecondaryCommand cmd) => _activeWeapon?.HandleSecondary(cmd.TriggerState);

        public void CancelAbilitySecondary() { }

        public void Cancel() => _activeWeapon?.HandlePrimary(ButtonState.Up);
    }
}
