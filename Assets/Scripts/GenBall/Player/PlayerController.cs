using GenBall.Accessory;
using GenBall.Enemy;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Player
{
    public class PlayerController:ISingleton
    {
        public static PlayerController Instance => SingletonManager.GetSingleton<PlayerController>();

        public readonly ActorInfo Actor = new();
        public void Init()
        {
            // todo gzp 后续修改为可配置
            Actor.MaxHealth = 6;
            Actor.Health = Actor.MaxHealth;
            Actor.Armor = Actor.MaxHealth;
            Actor.KillPoints = 0;


            GameEntry.GetModule<EventManager>().Subscribe(EnemyDeadEventArgs.Index,OnEnemyDead);
        }

        public void ApplyDamage(int damage)
        {
            if (Actor.Armor >= damage)
            {
                Actor.Armor -= damage;
            }
            else
            {
                damage-=Actor.Armor;
                Actor.Armor = 0;
                Actor.Health -= damage;
            }
        }
        

        private void OnEnemyDead(object sender, GameEventArgs eventArgs)
        {
            // todo gzp 可能会有范围判定
            if(eventArgs is not EnemyDeadEventArgs args) return;
            Actor.KillPoints += args.KillPoints;
        }
    }
}
