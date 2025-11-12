using Yueyn.Base.ReferencePool;

namespace Yueyn.Base.EventPool
{
    public partial class EventPool<T>
    {
        private sealed class Event : IReference
        {
            private object _sender;
            private T _eventArgs;

            public Event()
            {
                _sender=null;
                _eventArgs=null;
            }

            public object Sender => _sender;
            public T EventArgs => _eventArgs;

            public static Event Create(object sender, T eventArgs)
            {
                var eventNode = ReferencePool.ReferencePool.Acquire<Event>();
                eventNode._sender=sender;
                eventNode._eventArgs=eventArgs;
                return eventNode;
            }

            public void Clear()
            {
                _sender=null;
                _eventArgs=null;
            }
        }
    }
}