// [OBSOLETE] 旧 Player 体系 — 已迁移到 BattleEntity (DamageReceiverComponent + StatComponent)。
// UI 事件桥接在 DamageReceiverComponent 中处理，PlayerEntityFactory 中标记 isPlayer=true。
// 此文件将在 Phase E 清理时删除。

using GenBall.BattleSystem;
using GenBall.BattleSystem.Generated;
using GenBall.Event.Generated;
using UnityEngine;

namespace GenBall.Player
{
    public partial class Player:IDamageable,IHealable,IArmor
    {
        [SerializeField] private int baseMaxHealth;
        private void InitHealth()
        {
            MaxHealthStat.OnValueChange += OnMaxHealthChange;
            MaxHealthStat.SetBaseValue(baseMaxHealth);
            Health = MaxHealth;
            Armor = MaxHealth;
        }
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
        
        public int Health
        {
            get=>_health;
            private set
            {
                _health = value;
                GameEntry.Event.FirePlayerHealth(_health);
                if (_health <= 0)
                {
                    // todo gzp ս��ϵͳ�ع���ǵû�������
                    
                    // this.FireEventEntityDeath();
                }
            }
        }
        private int _health;

        public int MaxHealth => MaxHealthStat.CurrentValue;

        public int Armor
        {
            get=>_armor;
            private set
            {
                _armor = value;
                GameEntry.Event.FirePlayerArmor(_armor);
            }
        }

        private int _armor;

        public int MaxArmor => MaxHealth;

        public readonly IntStat MaxHealthStat = new IntStat();

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Debug.Log("�����ˣ�����ûд�����߼�");
            }
        }

        public void Die(DeathInfo deathInfo)
        {
            // todo gzp ս��ϵͳ�ع���ǵû�������
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

        private void OnMaxHealthChange(int value)=>GameEntry.Event.FirePlayerMaxHealth(value);
    }
}