using System;
using System.Collections.Generic;
using Yueyn.Utils;

namespace Yueyn.Base.ReferencePool
{
    public partial class ReferencePool
    {
        private sealed class InternalPool
        {
            private readonly Type _referenceType;
            private readonly Queue<IReference> _pool;
            private int _usingReferenceCount;

            public InternalPool(Type referenceType)
            {
                _referenceType = referenceType;
                _pool = new Queue<IReference>();
                _usingReferenceCount = 0;
            }
            public int UsingReferenceCount => _usingReferenceCount;

            public int UnusedReferenceCount=>_pool.Count;
            public Type ReferenceType => _referenceType;

            public T Acquire<T>() where T : IReference, new()
            {
                if (_referenceType!=typeof(T))
                {
                    throw new Exception("Reference Pool: Reference type is not assignable to reference type.");
                }

                _usingReferenceCount++;
                lock (_pool)
                {
                    return _pool.IsNullOrEmpty()? new T() : (T)_pool.Dequeue();
                }
            }

            public IReference Acquire()
            {
                _usingReferenceCount++;
                lock (_pool)
                {
                    return _pool.IsNullOrEmpty()?Activator.CreateInstance(_referenceType) as IReference:_pool.Dequeue();
                }
            }

            public void Release(IReference reference)
            {
                if (_referenceType!=reference.GetType())
                {
                    throw new Exception("Reference Pool: Reference type is not assignable to reference type.");
                }
                reference.Clear();
                lock (_pool)
                {
                    if (_pool.Contains(reference))
                    {
                        throw new Exception("Reference Pool: Reference type is already released.");
                    }
                    _pool.Enqueue(reference);
                }
                _usingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (_referenceType!=typeof(T))
                {
                    throw new Exception("Reference Pool: Reference type is not assignable to reference type.");
                }

                lock (_pool)
                {
                    while (count > 0)
                    {
                        _pool.Enqueue(new T());
                        count--;
                    }
                }
            }

            public void Add(int count)
            {
                lock (_pool)
                {
                    while (count > 0)
                    {
                        _pool.Enqueue((IReference)Activator.CreateInstance(_referenceType));
                        count--;
                    }
                }
            }

            public void Remove(int count)
            {
                lock (_pool)
                {
                    count=Math.Min(count, _pool.Count);
                    while (count > 0)
                    {
                        _pool.Dequeue();
                        count--;
                    }
                }
            }

            public void RemoveAll()
            {
                lock (_pool)
                {
                    _pool.Clear();
                }
            }
        }
    }
}