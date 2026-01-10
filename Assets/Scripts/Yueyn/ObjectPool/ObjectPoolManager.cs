using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Utils;

namespace Yueyn.ObjectPool
{
    public sealed partial class ObjectPoolManager : MonoBehaviour,IComponent
    {
        public int Priority => 0;
        private const int DefaultCapacity = int.MaxValue;
        private const float DefaultExpireTime = float.MaxValue;
        private const int DefaultPriority = 0;
        private readonly Dictionary<TypeNamePair, ObjectPoolBase> _objectPools = new();
        private readonly List<ObjectPoolBase> _cachedAllObjectPools = new();
        
        public void Init()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            foreach (var objectPool in _objectPools.Values)
            {
                objectPool.Update(elapsedSeconds, realElapseSeconds);
            }
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            foreach (var objectPool in _objectPools.Values)
            {
                objectPool.Shutdown();
            }
            _objectPools.Clear();
            _cachedAllObjectPools.Clear();
        }

        #region CreateObjectPool
        public IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);
        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType)
            => InternalCreateObjectPool(objectType, string.Empty, false, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name)
            => InternalCreateObjectPool(objectType, name, false, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity)
            => InternalCreateObjectPool(objectType, string.Empty, false, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, expireTime, DefaultCapacity, expireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime)
            => InternalCreateObjectPool(objectType, string.Empty, false, expireTime, DefaultCapacity, expireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity)
            => InternalCreateObjectPool(objectType, name, false, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, expireTime, DefaultCapacity, expireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime)
            => InternalCreateObjectPool(objectType, name, false, expireTime, DefaultCapacity, expireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, expireTime, capacity, expireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime)
            => InternalCreateObjectPool(objectType, string.Empty, false, expireTime, capacity, expireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(String.Empty, false, DefaultExpireTime, capacity, DefaultExpireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, int priority)
            => InternalCreateObjectPool(objectType, String.Empty, false, DefaultExpireTime, capacity, DefaultExpireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, expireTime, DefaultCapacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, string.Empty, false, expireTime, DefaultCapacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, expireTime, capacity, expireTime, DefaultPriority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
            => InternalCreateObjectPool(objectType, name, false, expireTime, capacity, expireTime, DefaultPriority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, DefaultExpireTime, capacity, DefaultExpireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, int priority)
            => InternalCreateObjectPool(objectType, name, false, DefaultExpireTime, capacity, DefaultExpireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, expireTime, DefaultCapacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, name, false, expireTime, DefaultCapacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(int capacity, float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, false, expireTime, capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, int capacity, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, String.Empty, false, expireTime, capacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, int capacity, float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, expireTime, capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, int capacity, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, name, false, expireTime, capacity, expireTime, priority);

        public IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, false, autoReleaseInterval, capacity, expireTime, priority);

        public ObjectPoolBase CreateSingleSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, name, false, autoReleaseInterval, capacity, expireTime, priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>() where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType)
            => InternalCreateObjectPool(objectType, string.Empty, true, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, true, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name)
            => InternalCreateObjectPool(objectType, name, true, DefaultExpireTime, DefaultCapacity, DefaultExpireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity)
            => InternalCreateObjectPool(objectType, string.Empty, true, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, expireTime, DefaultCapacity, expireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime)
            => InternalCreateObjectPool(objectType, string.Empty, true, expireTime, DefaultCapacity, expireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, true, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity)
            => InternalCreateObjectPool(objectType, name, true, DefaultExpireTime, capacity, DefaultExpireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, true, expireTime, DefaultCapacity, expireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float expireTime)
            => InternalCreateObjectPool(objectType, name, true, expireTime, DefaultCapacity, expireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, expireTime, capacity, expireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, float expireTime)
            => InternalCreateObjectPool(objectType, string.Empty, true, expireTime, capacity, expireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, int priority) where T:ObjectBase
            => InternalCreateObjectPool<T>(String.Empty, true, DefaultExpireTime, capacity, DefaultExpireTime, priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, int capacity, int priority)
            => InternalCreateObjectPool(objectType, String.Empty, true, DefaultExpireTime, capacity, DefaultExpireTime, priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(float expireTime, int priority) where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, expireTime, DefaultCapacity, expireTime, priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, float expireTime, int priority)
            => InternalCreateObjectPool(objectType, string.Empty, true, expireTime, DefaultCapacity, expireTime, priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name, int capacity, float expireTime) where T : ObjectBase
            => InternalCreateObjectPool<T>(name, true, expireTime, capacity, expireTime, DefaultPriority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, int capacity, float expireTime)
            => InternalCreateObjectPool(objectType, name, true, expireTime, capacity, expireTime, DefaultPriority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name,int capacity,int priority) where T : ObjectBase
            =>InternalCreateObjectPool<T>(name,true,DefaultExpireTime,capacity,DefaultExpireTime,priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType,string name,int capacity,int priority)
            =>InternalCreateObjectPool(objectType,name,true,DefaultExpireTime,capacity,DefaultExpireTime,priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name,float expireTime, int priority) where T : ObjectBase
            =>InternalCreateObjectPool<T>(name,true,expireTime,DefaultCapacity,expireTime,priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type  objectType,string name,float expireTime,int priority)
            =>InternalCreateObjectPool(objectType,name,true,expireTime,DefaultCapacity,expireTime,priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(int capacity, float expireTime, int priority)
            where T : ObjectBase
            => InternalCreateObjectPool<T>(string.Empty, true, expireTime, capacity, expireTime, priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType,int capacity,float expireTime,int priority)
            =>InternalCreateObjectPool(objectType,String.Empty,true,expireTime,capacity,expireTime,priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name,int capacity,float expireTime, int priority) where T : ObjectBase
            =>InternalCreateObjectPool<T>(name,true,expireTime,capacity,expireTime,priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType,string name,int capacity,float expireTime,int priority)
            =>InternalCreateObjectPool(objectType,name,true,expireTime,capacity,expireTime,priority);
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>(string name,float autoReleaseInterval,int  capacity, float expireTime, int priority) where T : ObjectBase
            =>InternalCreateObjectPool<T>(name,true,autoReleaseInterval,capacity,expireTime,priority);
        public ObjectPoolBase CreateMultiSpawnObjectPool(Type objectType, string name, float autoReleaseInterval, int capacity, float expireTime, int priority) 
            => InternalCreateObjectPool(objectType, name, true, autoReleaseInterval, capacity, expireTime, priority);
        #endregion
        
        #region GetObjectPool
        
        public void GetAllObjectPools(bool sort, [NotNull] List<ObjectPoolBase> results)
        {
            results.Clear();
            results.AddRange(_objectPools.Values);
            if (sort)
            {
                results.Sort(ObjectPoolComparer);
            }
        }
        #endregion

        #region DestroyObjectPool

        public bool DestroyObjectPool<T>(IObjectPool<T> objectPool) where T : ObjectBase => InternalDestroyObjectPool(new TypeNamePair(typeof(T), objectPool.Name));
        public bool DestroyObjectPool([NotNull] ObjectPoolBase objectPool)=>InternalDestroyObjectPool(new TypeNamePair(objectPool.ObjectType, objectPool.Name));
        #endregion
        
        #region HasObjectPool

        public bool HasObjectPool<T>() => InternalHasObjectPool(new TypeNamePair(typeof(T)));
        public bool HasObjectPool([NotNull] Type objectPool)
        {
            if (!typeof(ObjectBase).IsAssignableFrom(objectPool))
            {
                throw new Exception("Object type is not an ObjectBase");
            }
            return InternalHasObjectPool(new TypeNamePair(objectPool, objectPool.Name));
        }
        public bool HasObjectPool<T>(string name) where T : ObjectBase => InternalHasObjectPool(new TypeNamePair(typeof(T), name));
        public bool HasObjectPool([NotNull] Type objectType, string name)
        {
            if (!typeof(ObjectBase).IsAssignableFrom(objectType))
            {
                throw new Exception("Object type is not an ObjectBase");
            }
            return InternalHasObjectPool(new TypeNamePair(objectType, name));
        }

        public bool HasObjectPool([NotNull] Predicate<ObjectPoolBase> condition)
        {
            foreach (var objectPool in _objectPools.Values)
            {
                if (condition(objectPool)) return true;
            }
            return false;
        }
        #endregion

        #region Release

        public void Release()
        {
            GetAllObjectPools(true, _cachedAllObjectPools);
            foreach (var objectPool in _cachedAllObjectPools)
            {
                objectPool.Release();
            }
        }
        public void ReleaseAllUnused()
        {
            GetAllObjectPools(true,_cachedAllObjectPools);
            foreach (var objectPool in _cachedAllObjectPools)
            {
                objectPool.ReleaseAllUnused();
            }
        }
        #endregion
        
        #region InternalFunctions
        private bool InternalHasObjectPool(TypeNamePair typeNamePair)=>_objectPools.ContainsKey(typeNamePair);
        private ObjectPoolBase InternalGetObjectPool(TypeNamePair typeNamePair) =>_objectPools.GetValueOrDefault(typeNamePair);

        private ObjectPool<T> InternalCreateObjectPool<T>(string name, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : ObjectBase
        {
            var typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasObjectPool<T>(name))
            {
                throw new Exception("Object pool already created");
            }
            var objectPool=new ObjectPool<T>(name,allowMultiSpawn,autoReleaseInterval,capacity,expireTime,priority);
            _objectPools.Add(typeNamePair, objectPool);
            return objectPool;
        }
        private ObjectPoolBase InternalCreateObjectPool([NotNull] Type objectType, string name, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            if (!typeof(ObjectBase).IsAssignableFrom(objectType))
            {
                throw new Exception("Object type is not an ObjectBase");
            }
            var typeNamePair = new TypeNamePair(objectType, name);
            if (HasObjectPool(objectType, name))
            {
                throw new Exception("Object pool already created");
            }
            var objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            var objectPool=(ObjectPoolBase)Activator.CreateInstance(objectPoolType, name,allowMultiSpawn, autoReleaseInterval, capacity, expireTime, priority);
            _objectPools.Add(typeNamePair, objectPool);
            return objectPool;
        }
        private bool InternalDestroyObjectPool(TypeNamePair typeNamePair)
        {
            if (!_objectPools.TryGetValue(typeNamePair, out var objectPool)) return false;
            objectPool.Shutdown();
            return _objectPools.Remove(typeNamePair);
        }
        private static int ObjectPoolComparer(ObjectPoolBase a, ObjectPoolBase b)=>a.Priority.CompareTo(b.Priority);
        #endregion
    }
}