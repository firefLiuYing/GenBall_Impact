using System;
using System.Collections.Generic;

namespace Yueyn.Base.ReferencePool
{
    public static partial class ReferencePool
    {
        private static readonly Dictionary<Type, InternalPool> _pools = new();

        public static T Acquire<T>() where T : class, IReference, new() => GetInternalPool(typeof(T)).Acquire<T>();

        public static IReference Acquire(Type type)
        {
            if (!type.IsValidPoolType())
            {
                throw new Exception("Reference Pool: type is not valid");
            }
            return GetInternalPool(type).Acquire();
        } 

        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new Exception("Reference Pool: reference is null");
            }
            GetInternalPool(reference.GetType()).Release(reference);
        }

        public static void Add<T>(int count) where T : class, IReference, new() =>GetInternalPool(typeof(T)).Add<T>(count);

        public static void Add(Type type, int count)
        {
            if (!type.IsValidPoolType())
            {
                throw new Exception("Reference Pool: type is not valid");
            }

            GetInternalPool(type).Add(count);
        }

        public static void Remove<T>(int count) where T : class, IReference, new() =>GetInternalPool(typeof(T)).Remove(count);

        public static void Remove(Type type, int count)
        {
            if (!type.IsValidPoolType())
            {
                throw new Exception("Reference Pool: type is not valid");
            }
            GetInternalPool(type).Remove(count);
        }

        public static void RemoveAll<T>() where T : class, IReference, new() => GetInternalPool(typeof(T)).RemoveAll();

        public static void RemoveAll(Type type)
        {
            if (!type.IsValidPoolType())
            {
                throw new Exception("Reference Pool: type is not valid");
            }
            GetInternalPool(type).RemoveAll();
        }
        private static bool IsValidPoolType(this Type type) => type is { IsClass: true } && typeof(IReference).IsAssignableFrom(type);
        private static InternalPool GetInternalPool(Type type)
        {
            if (type == null)
            {
                throw new Exception("Reference Pool: type is null");
            }
            InternalPool pool=null;
            lock (_pools)
            {
                if (!_pools.TryGetValue(type, out pool))
                {
                    pool = new(type);
                    _pools.Add(type, pool);
                }
            }
            return pool;
        }
    }
}