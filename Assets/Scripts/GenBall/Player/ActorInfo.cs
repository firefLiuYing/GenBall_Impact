using GenBall.Event;
using GenBall.Event.Generated;
using GenBall.Utils.Attributes.InspectorButton;
using GenBall.Utils.Attributes.LiveData;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Player
{
    public class ActorInfo
    {
        private int _maxHealth;

        public int MaxHealth
        {
            get => _maxHealth;
            set
            {
                GameEntry.Event.FirePlayerMaxHealth(value);
                _maxHealth = value;
            }
        }
        
        private int _health;
        public int Health
        {
            get => _health;
            set
            {
                GameEntry.Event.FirePlayerHealth(value);
                _health = value;
            }
        }
        
        private int _killPoints;
        public int KillPoints
        {
            get => _killPoints;
            set
            {
                GameEntry.Event.FirePlayerKillPoints(value);
                _killPoints = value;
            }
        }

        private int _armor;
        
        public int Armor
        {
            get => _armor;
            set
            {
                GameEntry.Event.FirePlayerArmor(value);
                _armor = value;
            }
        }
    }
}