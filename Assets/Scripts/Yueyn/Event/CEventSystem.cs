using System;
using System.Collections.Generic;

namespace Yueyn.Event
{
    public class CEventSystem : IEventSystem, Yueyn.Main.IRenderUpdate
    {
        private readonly Dictionary<int, List<Delegate>> _handlers = new();
        private readonly Queue<Action> _pendingEvents = new();
        private Action<int> _defaultHandler;

        #region ISystem

        public void Init()
        {
            _handlers.Clear();
            _pendingEvents.Clear();
            _defaultHandler = null;
        }

        public void UnInit()
        {
            Clear();
            _handlers.Clear();
            _defaultHandler = null;
        }

        #endregion

        #region IRenderUpdate

        public void RenderUpdate(float deltaTime)
        {
            while (_pendingEvents.Count > 0)
                _pendingEvents.Dequeue().Invoke();
        }

        #endregion

        #region Subscribe

        public void Subscribe(int id, Action handler) { Add(id, handler); }
        public void Subscribe<T1>(int id, Action<T1> handler) { Add(id, handler); }
        public void Subscribe<T1, T2>(int id, Action<T1, T2> handler) { Add(id, handler); }
        public void Subscribe<T1, T2, T3>(int id, Action<T1, T2, T3> handler) { Add(id, handler); }
        public void Subscribe<T1, T2, T3, T4>(int id, Action<T1, T2, T3, T4> handler) { Add(id, handler); }

        #endregion

        #region Unsubscribe

        public void Unsubscribe(int id, Action handler) { Remove(id, handler); }
        public void Unsubscribe<T1>(int id, Action<T1> handler) { Remove(id, handler); }
        public void Unsubscribe<T1, T2>(int id, Action<T1, T2> handler) { Remove(id, handler); }
        public void Unsubscribe<T1, T2, T3>(int id, Action<T1, T2, T3> handler) { Remove(id, handler); }
        public void Unsubscribe<T1, T2, T3, T4>(int id, Action<T1, T2, T3, T4> handler) { Remove(id, handler); }

        #endregion

        #region Fire

        public void Fire(int id) => Enqueue(() => InvokeAll(id, d => { if (d is Action a) a(); }));
        public void Fire<T1>(int id, T1 a) => Enqueue(() => InvokeAll(id, d => { if (d is Action<T1> x) x(a); }));
        public void Fire<T1, T2>(int id, T1 a, T2 b) => Enqueue(() => InvokeAll(id, d => { if (d is Action<T1, T2> x) x(a, b); }));
        public void Fire<T1, T2, T3>(int id, T1 a, T2 b, T3 c) => Enqueue(() => InvokeAll(id, d => { if (d is Action<T1, T2, T3> x) x(a, b, c); }));
        public void Fire<T1, T2, T3, T4>(int id, T1 a, T2 b, T3 c, T4 d) => Enqueue(() => InvokeAll(id, x => { if (x is Action<T1, T2, T3, T4> y) y(a, b, c, d); }));

        #endregion

        #region FireNow

        public void FireNow(int id) => InvokeAll(id, d => { if (d is Action a) a(); });
        public void FireNow<T1>(int id, T1 a) => InvokeAll(id, d => { if (d is Action<T1> x) x(a); });
        public void FireNow<T1, T2>(int id, T1 a, T2 b) => InvokeAll(id, d => { if (d is Action<T1, T2> x) x(a, b); });
        public void FireNow<T1, T2, T3>(int id, T1 a, T2 b, T3 c) => InvokeAll(id, d => { if (d is Action<T1, T2, T3> x) x(a, b, c); });
        public void FireNow<T1, T2, T3, T4>(int id, T1 a, T2 b, T3 c, T4 d) => InvokeAll(id, x => { if (x is Action<T1, T2, T3, T4> y) y(a, b, c, d); });

        #endregion

        #region Check / Default / Clear

        public bool Check(int id, Delegate handler)
        {
            if (!_handlers.TryGetValue(id, out var list)) return false;
            for (int i = 0; i < list.Count; i++) if (list[i] == handler) return true;
            return false;
        }

        public void SetDefaultHandler(Action<int> handler) => _defaultHandler = handler;
        public void Clear() => _pendingEvents.Clear();

        #endregion

        #region Private

        private void Add(int id, Delegate handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!_handlers.TryGetValue(id, out var list))
            {
                list = new List<Delegate>();
                _handlers[id] = list;
            }
            list.Add(handler);
        }

        private void Remove(int id, Delegate handler)
        {
            if (!_handlers.TryGetValue(id, out var list)) return;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == handler) { list.RemoveAt(i); break; }
            }
            if (list.Count == 0) _handlers.Remove(id);
        }

        private void Enqueue(Action dispatch)
        {
            lock (_pendingEvents) { _pendingEvents.Enqueue(dispatch); }
        }

        private void InvokeAll(int id, Action<Delegate> invoker)
        {
            if (!_handlers.TryGetValue(id, out var list))
            {
                _defaultHandler?.Invoke(id);
                return;
            }
            var snapshot = new List<Delegate>(list);
            for (int i = 0; i < snapshot.Count; i++)
                invoker(snapshot[i]);
        }

        #endregion
    }
}
