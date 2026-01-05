using GenBall.BattleSystem;
using UnityEngine;

namespace GenBall.Player
{
    public partial class Player:IAttackable,IHealable,IArmor
    {
        public AttackResult OnAttacked(AttackInfo attackInfo)
        {
            int totalDamage=attackInfo.Damage;
            if (Armor > totalDamage)
            {
                SubArmor(totalDamage);
            }
            else
            {
                totalDamage -= Armor;
                SubArmor(Armor);
                TakeDamage(totalDamage);
            }
            return AttackResult.Create(attackInfo.Damage);
        }
        
        public int Health { get; private set; }

        public int MaxHealth => MaxHealthStat.CurrentValue;

        public int Armor { get; private set; }

        public int MaxArmor => MaxHealth;

        public IntStat MaxHealthStat { get; }=new IntStat(6);

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Debug.Log("我死了，但是没写死亡逻辑");
            }
        }

        public void Heal(int healAmount)
        {
            Health += healAmount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public void AddArmor(int armorAmount)
        {
            Armor += armorAmount;
            if(Armor > MaxArmor) Armor = MaxArmor;
        }

        public void SubArmor(int armorAmount)
        {
            Armor -= armorAmount;
            if (Armor < 0) Armor = 0;
        }
    }
}