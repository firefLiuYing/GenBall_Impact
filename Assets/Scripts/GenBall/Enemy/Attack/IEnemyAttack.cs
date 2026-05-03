using GenBall.BattleSystem.Character;

namespace GenBall.Enemy.Attack
{
    public interface IEnemyAttack
    {
        int AttackId { get; }
        bool CanExecute { get; }
        bool IsExecuting { get; }
        void Init(CharacterState owner);
        void Execute();
        void Cancel();
        void Tick(float deltaTime);
    }
}
