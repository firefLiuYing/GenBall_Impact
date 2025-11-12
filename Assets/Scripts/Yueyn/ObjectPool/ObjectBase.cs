using System;
using Yueyn.Base.ReferencePool;

namespace Yueyn.ObjectPool
{
    public abstract class ObjectBase : IReference
    {
        private string _name=null;
        private object _target=null;
        private bool _locked=false;
        private int _priority=0;
        private DateTime _lastUsedTime=default(DateTime);
        public virtual bool CustomCanReleaseFlag => true;
        public string Name=>_name;
        public object Target => _target;
        public bool Locked
        {
            get => _locked;
            set => _locked=value;
        }

        public int Priority
        {
            get => _priority;
            set => _priority=value;
        }

        public DateTime LastUsedTime
        {
            get => _lastUsedTime;
            set => _lastUsedTime=value;
        }
        
        protected void Initialize(object target) => Initialize(null, target, false, 0);
        protected void Initialize(string name, object target) => Initialize(name, target, false, 0);
        protected void Initialize(string name, object target,int priority)=>Initialize(name,target,false,priority);
        protected void Initialize(string name, object target, bool locked, int priority)
        {
            _name = name??string.Empty;
            _target = target ?? throw new Exception("target is null");
            _locked = locked;
            _priority = priority;
            _lastUsedTime = DateTime.Now;;
        }
        public virtual void Clear()
        {
            _name = null;
            _target = null;
            _locked = false;    
            _priority = 0;
            _lastUsedTime = default(DateTime);
        }

        public virtual void OnSpawn()
        {
            
        }
        public virtual void OnDespawn()
        {
        }
        public abstract void Release(bool isShutdown);
    }
}