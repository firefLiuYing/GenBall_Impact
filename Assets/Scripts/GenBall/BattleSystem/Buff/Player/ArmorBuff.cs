using System;
using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.BattleSystem.Buff.Player
{
    /// <summary>
    /// PlayerµÄ»¤¼×
    /// </summary>
    public class ArmorBuff : BuffObj, ITriggerBeforeTakeDamage
    {
        private int _armor;
        private CharacterState _player;

        public int Armor
        {
            get => _armor;
            private set
            {
                _armor = value;
                OnArmorChange?.Invoke(_armor);
            }
        }

        public Action<int> OnArmorChange;
        public int MaxArmor { get; private set;}
        public override void OnAdd(AddBuffInfo addBuffInfo)
        {
            base.OnAdd(addBuffInfo);
            _player=Carrier.GetComponent<CharacterState>();
            _player.Stats.MaxHealth.OnValueChange+=OnMaxHealthChange;
            MaxArmor = _player.MaxHealth;
            Armor = MaxArmor;
        }

        public void TriggerBeforeTakeDamage(DamageInfo damageInfo)
        {
            var damageAmount = damageInfo.Damage.GetValue();
            if (Armor >= damageAmount)
            {
                damageInfo.Damage.AddDamage(-damageAmount);
                Armor -= damageAmount;
            }
            else
            {
                damageInfo.Damage.AddDamage(-Armor);
                Armor = 0;
            }
        }

        private void OnMaxHealthChange(int maxHealth)
        {
            MaxArmor=maxHealth;
        }
        public override void Clear()
        {
            base.Clear();
            _player.Stats.MaxHealth.OnValueChange-=OnMaxHealthChange;
            _player = null;
        }
    }
}