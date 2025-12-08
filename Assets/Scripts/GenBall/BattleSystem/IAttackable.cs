namespace GenBall.BattleSystem
{
    public interface IAttackable:IHealth
    {
        public void OnAttacked(AttackInfo attackInfo);
    }
}