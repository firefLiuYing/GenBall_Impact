namespace GenBall.BattleSystem.Framework
{
    public class WeaponDamageCalculator : IDamageCalculator
    {
        public int Calculate(DamageContext context)
        {
            float attack = context.AttackerStats?.GetValue("Attack") ?? 0f;
            float weaponDmg = context.WeaponStats?.GetValue("Damage") ?? 0f;
            return (int)(attack + weaponDmg);
        }
    }
}
