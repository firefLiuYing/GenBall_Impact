using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Yueyn.Base.ReferencePool;
using Yueyn.Utils;

namespace Yueyn.ObjectPool
{
    public partial class ObjectPoolManager
    {
        private sealed class ObjectPool<T> : ObjectPoolBase,IObjectPool<T> where T : ObjectBase
        {
            private  readonly MultiDictionary<string,Object<T>> _objects=new();
            private readonly Dictionary<object,Object<T>> _objectsMap=new();
            private readonly ReleaseObjectFilterCallback<T> _defaultFilterCallback;
            private readonly List<T> _cachedCanReleaseObjects=new();
            private readonly List<T> _cachedToReleaseObjects=new();
            private readonly bool _allowMutiSpawn;
            private float _autoReleaseInterval;
            private int _capacity;
            private int _priority;
            private float _autoReleaseTime;
            private float _expireTime;

            public ObjectPool(string name, bool allowMutiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority) : base(name)
            {
                _allowMutiSpawn = allowMutiSpawn;
                _autoReleaseInterval = autoReleaseInterval;
                _capacity = capacity;
                _expireTime=expireTime;
                _priority=priority;
            }
            public override Type ObjectType { get; }
            public override int Count { get; }
            public override int CanReleaseCount { get; }
            
            public override int Priority
            {
                get => _priority;
                set => _priority=value;
            }

            public bool AllowMultiSpawn=>_allowMutiSpawn;

            public float AutoReleaseInterval
            {
                get=>_autoReleaseTime;
                set => _autoReleaseTime=value;
            }

            public int Capacity
            {
                get => _capacity;
                set => _capacity=value;
            }

            public float ExpireTime
            {
                get => _expireTime;
                set => _expireTime=value;
            }
            public void Register([NotNull] T obj, bool spawned)
            {
                var internalObject=Object<T>.Create(obj,spawned);
                _objects.Add(obj.Name,internalObject);
                _objectsMap.Add(obj.Target,internalObject);
                if (Count > _capacity)
                {
                    Release();
                }
            }

            #region CanSpawn

            public bool CanSpawn() => CanSpawn(String.Empty);
            public bool CanSpawn([NotNull] string name)
            {
                if (_objects.TryGetValue(name, out var objLinkList))
                {
                    foreach (var internalObject in objLinkList)
                    {
                        if (_allowMutiSpawn || !internalObject.IsInUse)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            #endregion
            
            #region Spawn

            public T Spawn() => Spawn(String.Empty);
            public T Spawn([NotNull] string name)
            {
                if (_objects.TryGetValue(name, out var objLinkList))
                {
                    foreach (var internalObject in objLinkList)
                    {
                        if (_allowMutiSpawn || !internalObject.IsInUse)
                        {
                            return internalObject.Spawn();
                        }
                    }
                }

                return null;
            }
            #endregion

            #region Despawn

            public void Despawn([NotNull] T obj)=> Despawn(obj.Target);
            public void Despawn([NotNull] object target)
            {
                var internalObject=GetObject(target);
                if (internalObject == null)
                {
                    throw new Exception($"Can not find object {target}");
                }
                internalObject.Despawn();
                if (Count > _capacity && internalObject.SpawnCount <= 0)
                {
                    Release();
                }
            }
            #endregion

            #region SetLocked
            public void SetLocked([NotNull] T obj, bool locked)=>SetLocked(obj.Target,locked);
            public void SetLocked([NotNull] object target, bool locked)
            {
                var internalObject=GetObject(target);
                if (internalObject == null)
                {
                    throw new Exception($"Can not find object {target}");
                }
                internalObject.Locked=locked;
            }
            #endregion

            #region SetPriority
            public void SetPriority([NotNull] T obj,int priority)=>SetPriority(obj.Target,priority);
            
            public void SetPriority([NotNull] object target, int priority)
            {
                var internalObject=GetObject(target);
                if (internalObject == null)
                {
                    throw new Exception($"Can not find target: {target}");
                }
                internalObject.Priority = priority;
            }
            #endregion

            #region Release

            public override void Release() => Release(Count - _capacity, _defaultFilterCallback);

            public override void Release(int toReleaseCount) => Release(toReleaseCount, _defaultFilterCallback);

            public void Release(ReleaseObjectFilterCallback<T> filterCallback)=>Release(Count-_capacity,filterCallback);
            public void Release(int toReleaseCount, [NotNull] ReleaseObjectFilterCallback<T> releaseObjectFilterCallback)
            {
                toReleaseCount=Math.Max(0, toReleaseCount);
                var expireTime = DateTime.MinValue;
                if (_expireTime < float.MaxValue)
                {
                    expireTime=DateTime.Now.AddSeconds(-_expireTime);
                }

                _autoReleaseTime = 0f;
                GetCanReleaseObjects(_cachedCanReleaseObjects);
                var toReleaseObjects = releaseObjectFilterCallback(_cachedCanReleaseObjects,toReleaseCount,expireTime);
                if(toReleaseObjects.IsNullOrEmpty()) return;
                foreach (var toReleaseObject in toReleaseObjects)
                {
                    ReleaseObject(toReleaseObject);
                }
            }
            public bool ReleaseObject([NotNull] T obj) => ReleaseObject(obj.Target);

            public bool ReleaseObject([NotNull] object target)
            {
                var internalObject=GetObject(target);
                if (internalObject == null) return false;
                if(internalObject.IsInUse||internalObject.Locked||!internalObject.CustomCanBeReleaseFlag) return false;
                _objects.Remove(internalObject.Name,internalObject);
                _objectsMap.Remove(internalObject.Peek.Target);
                internalObject.Release(false);
                ReferencePool.Release(internalObject);
                return true;
            }
            public override void ReleaseAllUnused()
            {
                _autoReleaseTime = 0f;
                GetCanReleaseObjects(_cachedCanReleaseObjects);
                foreach (var toReleaseObject in _cachedCanReleaseObjects)
                {
                    ReleaseObject(toReleaseObject);
                }
            }
            #endregion
            

            public override void Update(float elapsedTime, float realElapsedTime)
            {
                _autoReleaseTime+=realElapsedTime;
                if(_autoReleaseTime < _autoReleaseInterval) return;
                Release();
            }

            public override void Shutdown()
            {
                foreach (var internalObject in _objectsMap.Values)
                {
                    internalObject.Release(true);
                    ReferencePool.Release(internalObject);
                }
                _objects.Clear();
                _objectsMap.Clear();
                _cachedCanReleaseObjects.Clear();
                _cachedToReleaseObjects.Clear();
            }

            private Object<T> GetObject([NotNull] object target) => _objectsMap.GetValueOrDefault(target);
            private void GetCanReleaseObjects([NotNull] List<T> results)
            {
                results.Clear();
                foreach (var internalObject in _objectsMap.Values)
                {
                    if (internalObject.IsInUse || internalObject.Locked || !internalObject.CustomCanBeReleaseFlag)
                    {
                        continue;
                    }
                    results.Add(internalObject.Peek);
                }
            }
            private List<T> DefaultFilter(List<T> candidateObjects,int toReleaseCount,DateTime expireTime)
            {
                _cachedToReleaseObjects.Clear();
                if (expireTime > DateTime.MinValue)
                {
                    for (int i = candidateObjects.Count - 1; i >= 0; i--)
                    {
                        if (candidateObjects[i].LastUsedTime > expireTime) continue;
                        _cachedToReleaseObjects.Add(candidateObjects[i]);
                        candidateObjects.RemoveAt(i);
                    }
                    toReleaseCount-=_cachedToReleaseObjects.Count;
                }

                for (int i = 0; toReleaseCount > 0 && i < candidateObjects.Count; i++)
                {
                    for (int j = i + 1; j < candidateObjects.Count; j++)
                    {
                        if (candidateObjects[i].Priority > candidateObjects[j].Priority
                            || candidateObjects[i].Priority == candidateObjects[j].Priority &&
                            candidateObjects[i].LastUsedTime > candidateObjects[j].LastUsedTime)
                        {
                            (candidateObjects[i], candidateObjects[j]) = (candidateObjects[j], candidateObjects[i]);
                        }
                    }
                    _cachedToReleaseObjects.Add(candidateObjects[i]);
                    toReleaseCount--;
                }
                return _cachedToReleaseObjects;
            }
        }
    }
}