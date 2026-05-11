using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace Yueyn.Pool
{
    /// <summary>
    /// 对象池系统默认实现，基于 ReferencePool 提供 C# 引用池能力
    /// </summary>
    public sealed class PoolSystemDefault : IPoolSystem
    {
        private readonly Dictionary<Type, InternalPoolInfo> _poolInfos = new();

        #region ISystem

        public void Init() { }

        public void UnInit()
        {
            // 先清理所有已追踪类型的 ReferencePool 内部缓存
            var types = new List<Type>(_poolInfos.Keys);
            foreach (var type in types)
            {
                try { ReferencePool.RemoveAll(type); }
                catch (Exception ex) { Debug.LogWarning($"[PoolSystem] RemoveAll({type.Name}) failed: {ex.Message}"); }
            }
            _poolInfos.Clear();
        }

        #endregion

        #region C# 引用池

        public T Acquire<T>() where T : class, IReference, new()
        {
            var obj = ReferencePool.Acquire<T>();
            TrackAcquire(typeof(T));
            return obj;
        }

        public IReference Acquire(Type type)
        {
            var obj = ReferencePool.Acquire(type);
            TrackAcquire(type);
            return obj;
        }

        public void Release(IReference obj)
        {
            if (obj == null) return;
            TrackRelease(obj.GetType());
            ReferencePool.Release(obj);
        }

        public void PreCreate<T>(int count) where T : class, IReference, new()
        {
            ReferencePool.Add<T>(count);
        }

        public int GetUsingCount(Type type)
        {
            return _poolInfos.TryGetValue(type, out var info) ? info.UsingCount : 0;
        }

        public int GetUnusedCount(Type type)
        {
            return _poolInfos.TryGetValue(type, out var info) ? info.UnusedCount : 0;
        }

        public void RemoveAll(Type type)
        {
            ReferencePool.RemoveAll(type);
            if (_poolInfos.TryGetValue(type, out var info))
            {
                info.Reset();
            }
        }

        public void ClearAll()
        {
            // 清理所有已追踪类型的缓存
            var types = new List<Type>(_poolInfos.Keys);
            foreach (var type in types)
            {
                try { ReferencePool.RemoveAll(type); }
                catch (Exception ex) { Debug.LogWarning($"[PoolSystem] RemoveAll({type.Name}) failed: {ex.Message}"); }
            }
            _poolInfos.Clear();
        }

        #endregion

        #region 内部追踪

        private void TrackAcquire(Type type)
        {
            if (!_poolInfos.TryGetValue(type, out var info))
            {
                info = new InternalPoolInfo();
                _poolInfos[type] = info;
            }
            info.OnAcquire();
        }

        private void TrackRelease(Type type)
        {
            if (_poolInfos.TryGetValue(type, out var info))
            {
                info.OnRelease();
            }
        }

        private class InternalPoolInfo
        {
            public int UsingCount { get; private set; }
            public int UnusedCount { get; private set; }

            public void OnAcquire() => UsingCount++;
            public void OnRelease() => UsingCount--;

            public void Reset()
            {
                UsingCount = 0;
                UnusedCount = 0;
            }
        }

        #endregion
    }
}
