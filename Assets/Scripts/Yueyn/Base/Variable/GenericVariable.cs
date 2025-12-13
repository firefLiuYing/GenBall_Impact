using System;
using System.Collections.Generic;

namespace Yueyn.Base.Variable
{
    public class Variable<T> : Variable
    {
        public override Type Type=>typeof(T);

        public delegate void Listener(T value);

        private readonly List<Listener> _listeners = new();
        public void Observe(Listener listener)=>_listeners.Add(listener);
        public void Unobserve(Listener listener)=>_listeners.Remove(listener);
        public T Value { get; protected set; } = default(T);
        public override object GetValue()=>Value;
        public override void SetValue(object value)=>Value = (T)value;

        public void PostValue(T value)
        {
            Value = value;
            for (int i = 0; i < _listeners.Count; i++)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void PostValue() => PostValue(Value);
        public static Variable<T> Create() => ReferencePool.ReferencePool.Acquire<Variable<T>>();
        public override void Clear()
        {
            Value=default(T);
            _listeners.Clear();
        }
    }

    public class LiveDelegate<TDelegate> : Variable where TDelegate : Delegate
    {
        public override Type Type => typeof(TDelegate);
        private TDelegate _delegate;
        public TDelegate Value => _delegate;
        public override object GetValue()=>_delegate;
        public void SetDelegate(TDelegate @delegate)=>_delegate = @delegate;

        public override void SetValue(object value)
        {
            if (value is TDelegate @delegate)
            {
                _delegate = @delegate;
            }
            else
            {
                throw new Exception("TDelegate cannot be cast to TDelegate");
            }
        }

        public static LiveDelegate<TDelegate> Create()=>ReferencePool.ReferencePool.Acquire<LiveDelegate<TDelegate>>();
        public override void Clear()
        {
            _delegate = null;
        }
    }
}