using System;
using Yueyn.Utils;

namespace Yueyn.ObjectPool
{
    public abstract class ObjectPoolBase
    {
        private readonly string _name;
        public ObjectPoolBase(string name)=> _name = name??string.Empty;

        public ObjectPoolBase() : this(null)
        {
        }
        public string Name => _name;
        public abstract int Priority { get;set; }
        public abstract Type ObjectType { get; }
        public string FullName=>new TypeNamePair(ObjectType, Name).ToString();
        public abstract int Count { get; }
        public abstract int CanReleaseCount { get; }
        
        public abstract void Release();
        public abstract void Release(int toReleaseCount);
        public abstract void ReleaseAllUnused();
        public abstract void Update(float elapsedTime,float realElapsedTime);
        public abstract void Shutdown();
        
    }
}