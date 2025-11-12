using System;
using JetBrains.Annotations;

namespace Yueyn.ObjectPool
{
    public interface IObjectPool<T> where T : ObjectBase
    {
        public string Name { get; }
        public string FullName { get; }
        public Type ObjectType { get; }
        public int Count { get; }
        public int CanReleaseCount { get; }
        public bool AllowMultiSpawn { get; }
        public float AutoReleaseInterval { get; set; }
        public int Capacity{get;set;}
        public float ExpireTime{get;set;}
        public int Priority { get; set; }
        public void Register(T obj, bool spawned);
        public bool CanSpawn();
        public bool CanSpawn(string name);
        public T Spawn();
        public void Despawn(T obj);
        public void Despawn(object target);
        public void SetLocked(T obj, bool locked);
        public void SetLocked(object target, bool locked);
        public void SetPriority(T obj, int priority);
        public void SetPriority(object obj, int priority);
        public bool ReleaseObject(T obj);
        public bool ReleaseObject(object target);
        public void Release();
        public void Release(int toReleaseCount);
        public void Release(ReleaseObjectFilterCallback<T> filterCallback);
        public void Release(int toReleaseCount, ReleaseObjectFilterCallback<T> filterCallback);
        public void ReleaseAllUnused();
    }
}