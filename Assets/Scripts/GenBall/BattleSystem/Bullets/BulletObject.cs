using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.ObjectPool;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletObject : ObjectBase
    {
        public static BulletObject Create(string name,object target)
        {
            var bulletObject = ReferencePool.Acquire<BulletObject>();
            bulletObject.Initialize(name,target);
            return bulletObject;
        }

        public override void OnSpawn()
        {
            if(Target is not GameObject go) return;
            go.SetActive(false);
        }

        public override void OnDespawn()
        {
            if(Target is not GameObject go) return;
            go.GetComponent<IBullet>().OnRecycle();
            go.SetActive(false);
        }

        public override void Release(bool isShutdown)
        {
            if(Target is not GameObject go) return;
            Object.Destroy(go);
        }
    }
}