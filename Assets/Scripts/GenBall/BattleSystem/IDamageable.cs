namespace GenBall.BattleSystem
{
    public interface IHealth
    {
        public int Health { get; }
        public int MaxHealth { get; }
        public bool IsDead { get; }
        public void Die(DeathInfo deathInfo);
    }
    public interface IDamageable:IHealth
    {
        /// <summary>
        /// 实际扣除伤害，改方法传入的伤害和冲击力等数值都是已经计算完成的，不要再修改
        /// </summary>
        /// <param name="damageInfo"></param>
        public void TakeDamage(DamageInfo damageInfo);
    }


    public interface IHealable:IHealth
    {
        public void Heal(int healAmount);
    }

    public interface IArmor
    {
        public int Armor { get; }
        public int MaxArmor { get; }
        public void AddArmor(int armorAmount);
        public void SubArmor(int armorAmount);
    }
    
    public delegate AttackResult OnAttackDelegate(AttackInfo attackInfo);
}