using GenBall.Event;
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
                var e=ValueChangeEventArgs<int>.Create("ActorInfo.MaxHealth",value);
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _maxHealth = value;
            }
        }
        
        private int _health;
        public int Health
        {
            get => _health;
            set
            {
                var e=ValueChangeEventArgs<int>.Create("ActorInfo.Health",value);
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _health = value;
            }
        }
        
        private int _killPoints;
        public int KillPoints
        {
            get => _killPoints;
            set
            {
                var e=ValueChangeEventArgs<int>.Create("ActorInfo.KillPoints",value);
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _killPoints = value;
            }
        }

        private int _armor;
        
        public int Armor
        {
            get => _armor;
            set
            {
                var e=ValueChangeEventArgs<int>.Create("ActorInfo.Armor",value);
                GameEntry.GetModule<EventManager>().Fire(this,e);
                _armor = value;
            }
        }
    }
}