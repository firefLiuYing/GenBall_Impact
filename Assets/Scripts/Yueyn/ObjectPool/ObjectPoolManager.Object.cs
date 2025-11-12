using System;
using JetBrains.Annotations;
using Yueyn.Base.ReferencePool;

namespace Yueyn.ObjectPool
{
    public partial class ObjectPoolManager
    {
        private sealed class Object<T> : IReference where T : ObjectBase
        {
            private T _object=null;
            private int _spawnCount=0;
            public string Name => _object.Name;

            public bool Locked
            {
                get => _object.Locked;
                set => _object.Locked=value;
            }

            public int Priority
            {
                get => _object.Priority;
                set => _object.Priority=value;
            }

            public bool CustomCanBeReleaseFlag => _object.CustomCanReleaseFlag;
            public DateTime LastUsedTime=> _object.LastUsedTime;
            public bool IsInUse=>_spawnCount>0;
            public int SpawnCount => _spawnCount;

            public static Object<T> Create([NotNull] T obj, bool spawned)
            {
                var internalObject=ReferencePool.Acquire<Object<T>>();
                internalObject._object=obj;
                internalObject._spawnCount=spawned?1:0;
                if (spawned)
                {
                    obj.OnSpawn();
                }
                return internalObject;
            }
            public void Clear()
            {
                _object=null;
                _spawnCount=0;
            }
            public T Peek=>_object;

            public T Spawn()
            {
                _spawnCount++;
                _object.LastUsedTime=DateTime.Now;
                _object.OnSpawn();
                return _object;
            }
            public void Despawn()
            {
                _object.OnDespawn();
                _object.LastUsedTime=DateTime.Now;
                _spawnCount--;
                if (_spawnCount < 0)
                {
                    throw new Exception($"Object {Name} spawn count is less than 0!");
                }
            }

            public void Release(bool isShutdown)
            {
                _object.Release(isShutdown);
                ReferencePool.Release(_object);
            }
        }
    }
}