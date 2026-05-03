namespace GenBall.BattleSystem.Command
{
    public interface IAttack
    {
        public void Attack(AttackCommand command);
        public bool IsAttacking { get; }
    }
}
