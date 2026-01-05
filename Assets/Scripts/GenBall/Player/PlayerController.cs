using GenBall.Enemy;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Player
{
    public class PlayerController:ISingleton
    {
        public static PlayerController Instance => SingletonManager.GetSingleton<PlayerController>();
        
        // public readonly ActorInfo Actor = new();

        public Player Player { get;private set; }
        public static Vector3 PlayerFace=>Camera.main.transform.forward;
        public void Init(Player player)
        {
            Player = player;
            // todo gzp 后续修改为可配置
            // Actor.MaxHealth = 6;
            // Actor.Health = Actor.MaxHealth;
            // Actor.Armor = Actor.MaxHealth;
            // Actor.KillPoints = 0;


            // GameEntry.GetModule<EventManager>().Subscribe(EnemyDeadEventArgs.Index,OnEnemyDead);
        }
        
    }
}
