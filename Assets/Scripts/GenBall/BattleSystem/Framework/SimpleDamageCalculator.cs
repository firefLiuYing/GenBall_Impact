namespace GenBall.BattleSystem.Framework
{
    public class SimpleDamageCalculator : IDamageCalculator
    {
        public int Calculate(DamageContext context)
        {
            if (context.AttackerStats == null) return 0;
            return (int)context.AttackerStats.GetValue("Attack");
        }
    }
}
