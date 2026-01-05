namespace GenBall.BattleSystem
{
    public interface IAttackable:IEffectable,IDamageable
    {
        public AttackResult OnAttacked(AttackInfo attackInfo);
    }

    public interface IHealth
    {
        public int Health { get; }
        public int MaxHealth { get; }
    }

    public interface IDamageable:IHealth
    {
        public void TakeDamage(int damage);
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