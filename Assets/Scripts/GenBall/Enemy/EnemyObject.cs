using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.ObjectPool;

namespace GenBall.Enemy
{
    public class EnemyObject:ObjectBase
    {
        public override void OnSpawn()
        {
            if(Target is not GameObject go) return;
            go.SetActive(false);
        }

        public override void OnDespawn()
        {
            if(Target is not GameObject go) return;
            go.SetActive(false);
        }

        public static EnemyObject Create(string name, object target)
        {
            var enemyObject = ReferencePool.Acquire<EnemyObject>();
            enemyObject.Initialize(name, target);
            return enemyObject;
        }
        public override void Release(bool isShutdown)
        {
            
        }
    }
}