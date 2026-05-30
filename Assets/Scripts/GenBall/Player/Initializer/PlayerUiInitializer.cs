using System;
using System.Linq;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Buff.Player;
using GenBall.BattleSystem.Character;
using GenBall.Event.Generated;
using UnityEngine;

namespace GenBall.Player.Initializer
{
    [Obsolete]
    public class PlayerUiInitializer : CharacterInitializerBase
    {
        
        public override void Initialize(CharacterState characterState)
        {
            characterState.Stats.MaxHealth.OnValueChange+=UpdateMaxHealth;
            characterState.OnHealthChange+=UpdateHealth;
            UpdateMaxHealth(characterState.Stats.MaxHealth.CurrentValue);
            var armorBuff = characterState.GetBuffs<ArmorBuff>().FirstOrDefault();
            if (armorBuff != null)
            {
                armorBuff.OnArmorChange+=UpdateArmor;
                UpdateArmor(armorBuff.Armor);
            }
        }

        private void UpdateArmor(int armor)
        {
            GameEntry.Event.FirePlayerArmor(armor);
        }
        private void UpdateMaxHealth(int maxHealth)
        {
            GameEntry.Event.FirePlayerMaxHealth(maxHealth);
        }
        private void UpdateHealth(int health)
        {
            GameEntry.Event.FirePlayerHealth(health);
        }
    }
}