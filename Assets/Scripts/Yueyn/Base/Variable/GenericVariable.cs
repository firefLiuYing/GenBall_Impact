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
        public T Value { get; private set; } = default(T);
        public override object GetValue()=>Value;
        public override void SetValue(object value)=>Value = (T)value;

        public void PostValue(T value)
        {
            Value = value;
            foreach (var listener in _listeners)
            {
                listener.Invoke(value);
            }
        }

        public void PostValue() => PostValue(Value);
        public override void Clear()
        {
            Value=default(T);
            _listeners.Clear();
        }
    }
}