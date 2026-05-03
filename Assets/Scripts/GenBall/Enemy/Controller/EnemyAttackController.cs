using System.Collections.Generic;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.Attack;

namespace GenBall.Enemy.Controller
{
    public class EnemyAttackController : CharacterControllerBase, IAttack
    {
        private CharacterState _characterState;
        private readonly Dictionary<int, IEnemyAttack> _attacks = new();
        private IEnemyAttack _currentAttack;
        public bool IsAttacking => _currentAttack is { IsExecuting: true };

        public override void Initialize(CharacterState characterState)
        {
            _characterState = characterState;
            var attacks = characterState.GetComponentsInChildren<IEnemyAttack>();
            foreach (var attack in attacks)
            {
                attack.Init(characterState);
                _attacks[attack.AttackId] = attack;
            }
        }

        public void Attack(AttackCommand command)
        {
            if (IsAttacking) return;
            if (_attacks.TryGetValue(command.AttackId, out var attack) && attack.CanExecute)
            {
                _currentAttack = attack;
                attack.Execute();
            }
        }

        public override void Tick(float deltaTime)
        {
            _currentAttack?.Tick(deltaTime);
            if (_currentAttack is { IsExecuting: false })
                _currentAttack = null;
        }
    }
}
