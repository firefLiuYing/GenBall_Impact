using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.ObjectPool;

namespace GenBall.Utils.EntityCreator
{
    public partial class EntityCreator<TEntityInterface> where TEntityInterface:IEntity
    {
        private class EntityObject : ObjectBase
        {
            public override void OnSpawn()
            {
                if(Target is not GameObject go) return;
                go.SetActive(false);
            }

            public override void OnDespawn()
            {
                if(Target is not GameObject go) return;
                go.GetComponent<IEntity>().OnRecycle();
                // Debug.Log($"{go.name} has been destroyed");
                go.SetActive(false);
            }
            public static EntityObject Create(string name, object target)
            {
                var entityObject=ReferencePool.Acquire<EntityObject>();
                entityObject.Initialize(name, target);
                return entityObject;
            }
            public override void Release(bool isShutdown)
            {
            
            }
        }
    }
    
}