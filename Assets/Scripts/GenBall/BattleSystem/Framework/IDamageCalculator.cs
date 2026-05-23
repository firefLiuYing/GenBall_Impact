namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Strategy for calculating raw damage before buffs/defense.
    /// Different implementations handle different damage source combinations.
    /// </summary>
    public interface IDamageCalculator
    {
        int Calculate(DamageContext context);
    }
}
