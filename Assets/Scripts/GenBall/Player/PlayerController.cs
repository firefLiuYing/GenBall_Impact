using System.Collections;
using System.Collections.Generic;
using GenBall.Enemy;
using GenBall.Utils.Singleton;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Event;

namespace GenBall.Player
{
    public class PlayerController:ISingleton
    {
        public static PlayerController Instance => SingletonManager.GetSingleton<PlayerController>();
        public readonly Variable<int> Health = ReferencePool.Acquire<Variable<int>>();
        public readonly Variable<int> MaxHealth = ReferencePool.Acquire<Variable<int>>();
        
        public readonly Variable<int> Kills=ReferencePool.Acquire<Variable<int>>();
        public void Init()
        {
            // todo gzp 后续修改为可配置
            MaxHealth.PostValue(6);
            Health.PostValue(MaxHealth.Value);
            Kills.PostValue(0);
            
            GameEntry.GetModule<EventManager>().Subscribe(EnemyDeadEventArgs.Index,OnEnemyDead);
        }

        private void OnEnemyDead(object sender, GameEventArgs eventArgs)
        {
            // todo gzp 可能会有范围判定
            Kills.PostValue(Kills.Value+1);
        }
    }
}
