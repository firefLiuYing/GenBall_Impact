using Yueyn.Base.ReferencePool;
using Yueyn.Event;
using Yueyn.Utils;

namespace GenBall.BattleSystem
{
    public abstract class EffectEventArgs : GameEventArgs
    {
        public static int GetEventId(string eventName) => eventName.GetHashCode();
    }

    public class EffectEventArgs<T> : EffectEventArgs
    {
        private string _eventName;
        public override int Id => GetEventId(_eventName);
        public string EventName=>_eventName;
        public T Args { get;private set; }

        public static EffectEventArgs<T> Create(string eventName, T args)
        {
            var e=ReferencePool.Acquire<EffectEventArgs<T>>();
            e._eventName = eventName;
            e.Args = args;
            return e;
        }
        public override void Clear()
        {
            _eventName = string.Empty;
            Args = default;
        }
    }

    public class EffectEventArgs<T1,T2> : EffectEventArgs
    {
        private string _eventName;
        public override int Id => GetEventId(_eventName);
        public string EventName => _eventName;
        public T1 Args1 { get;private set; }
        public T2 Args2 { get; private set; }

        public static EffectEventArgs<T1, T2> Create(string eventName, T1 arg1, T2 arg2)
        {
            var e=ReferencePool.Acquire<EffectEventArgs<T1, T2>>();
            e._eventName=eventName;
            e.Args1 = arg1;
            e.Args2 = arg2;
            return e;
        }

        public override void Clear()
        {
            _eventName = string.Empty;
            Args1 = default;
            Args2 = default;
        }
    }
}