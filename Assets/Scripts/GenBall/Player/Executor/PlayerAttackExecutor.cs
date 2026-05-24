using GenBall.BattleSystem.Command;
using GenBall.Player.Controller;
using GenBall.Player.Input;

namespace GenBall.Player.Executor
{
    public class PlayerAttackExecutor : IAttack
    {
        private readonly WeaponController _weaponController;

        public PlayerAttackExecutor(WeaponController weaponController)
        {
            _weaponController = weaponController;
        }

        public void Attack(AttackCommand cmd)
        {
            // TODO: AttackId from cmd to map to different weapon modes
            _weaponController.Fire(ButtonState.Down);
        }

        /// <summary>
        /// Weapon is fire-and-forget. WeaponState has no completion-related flag.
        /// NormalTriggerController fires each frame in Update while button is held,
        /// so there is no persistent "attack in progress" state to track here.
        /// </summary>
        public bool IsAttacking => false;
    }
}
