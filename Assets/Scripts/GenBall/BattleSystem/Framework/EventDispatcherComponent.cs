using Yueyn.Event;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Entity-scoped event bus. Wraps Yueyn.Event.EventDispatcher for intra-entity component communication.
    /// Uses FireNow exclusively — all entity-internal events are immediate.
    /// </summary>
    public class EventDispatcherComponent
    {
        private readonly BattleEntity _entity;
        private readonly EventDispatcher _dispatcher = new();

        public EventDispatcherComponent(BattleEntity entity)
        {
            _entity = entity;
        }

        #region Subscribe

        public void Subscribe(int id, System.Action handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1>(int id, System.Action<T1> handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1, T2>(int id, System.Action<T1, T2> handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1, T2, T3>(int id, System.Action<T1, T2, T3> handler) => _dispatcher.Subscribe(id, handler);

        #endregion

        #region Unsubscribe

        public void Unsubscribe(int id, System.Action handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1>(int id, System.Action<T1> handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1, T2>(int id, System.Action<T1, T2> handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1, T2, T3>(int id, System.Action<T1, T2, T3> handler) => _dispatcher.Unsubscribe(id, handler);

        #endregion

        #region FireNow

        public void FireNow(int id) => _dispatcher.FireNow(id);
        public void FireNow<T1>(int id, T1 a) => _dispatcher.FireNow(id, a);
        public void FireNow<T1, T2>(int id, T1 a, T2 b) => _dispatcher.FireNow(id, a, b);
        public void FireNow<T1, T2, T3>(int id, T1 a, T2 b, T3 c) => _dispatcher.FireNow(id, a, b, c);

        #endregion
    }
}
