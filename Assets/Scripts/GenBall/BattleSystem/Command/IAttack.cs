namespace GenBall.BattleSystem.Command
{
    public interface IAttack
    {
        void Attack(AttackCommand command);
        void Cancel();
        bool IsAttacking { get; }
    }
}
