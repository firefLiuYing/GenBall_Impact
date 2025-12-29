namespace GenBall.BattleSystem
{
    public interface IAttackable:IEffectable
    {
        public AttackResult OnAttacked(AttackInfo attackInfo);
    }
    
    public delegate AttackResult OnAttackDelegate(AttackInfo attackInfo);
}