using System;
using System.Collections.Generic;
using Yueyn.Utils;

namespace Yueyn.Base.EventPool
{
    public sealed partial class EventPool<T> where T:BaseEventArgs
    {
        private readonly MultiDictionary<int, EventHandler<T>> _eventHandlers;
        private readonly Queue<Event> _events;
        private readonly EventPoolMode _eventPoolMode;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _cachedNodes;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _tempNodes;
        
        private EventHandler<T> _defaultHandler;

        public EventPool(EventPoolMode mode)
        {
            _eventHandlers = new();
            _events = new();
            _cachedNodes = new();
            _tempNodes = new();
            _eventPoolMode = mode;
            _defaultHandler = null;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (_events)
            {
                while (_events.Count > 0)
                {
                    var eventNode=_events.Dequeue();
                    HandleEvent(eventNode.Sender,eventNode.EventArgs);
                    ReferencePool.ReferencePool.Release(eventNode);
                }
            }
        }

        public void Shutdown()
        {
            Clear();
            _defaultHandler=null;
            _eventHandlers.Clear();
            _cachedNodes.Clear();
            _tempNodes.Clear();
        }

        public void Clear()
        {
            lock (_events)
            {
                _events.Clear();
            }
        }

        public bool Check(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new Exception($"EventPool: No handler for {typeof(T).Name}");
            }

            return _eventHandlers.Contains(id, handler);
        }
        public void Subscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new Exception($"EventPool: No handler for {typeof(T).Name}");
            }

            if (!_eventHandlers.Contains(id))
            {
                _eventHandlers.Add(id, handler);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowMultiHandler) != EventPoolMode.AllowMultiHandler)
            {
                throw new Exception($"EventPool: Multi handler for {typeof(T).Name}");
            }
            else if ((_eventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowDuplicateHandler &&Check(id, handler))
            {
                throw new Exception($"EventPool: Duplicate handler for {typeof(T).Name}");
            }
            else
            {
                _eventHandlers.Add(id, handler);
            }
        }

        public void Unsubscribe(int id, EventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new Exception($"EventPool: No handler for {typeof(T).Name}");
            }

            if (_cachedNodes.Count > 0)
            {
                foreach (var pair in _cachedNodes)
                {
                    if (pair.Value != null && pair.Value.Value == handler)
                    {
                        _tempNodes.Add(pair.Key, pair.Value);
                    }
                }

                if (_tempNodes.Count > 0)
                {
                    foreach (var pair in _tempNodes)
                    {
                        _cachedNodes[pair.Key] = pair.Value;
                    }
                    _tempNodes.Clear();
                }
            }
            
            if (!_eventHandlers.Remove(id, handler))
            {
                throw new Exception($"EventPool: '{id}' not exists specified handler for {typeof(T).Name} ");
            }
        }

        /// <summary>
        /// 线程安全，会在下一帧触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        public void Fire(object sender, T e)
        {
            if (e == null)
            {
                throw new Exception($"EventPool: No event for {typeof(T).Name}");
            }
            Event eventNode=Event.Create(sender,e);
            lock (_events)
            {
                _events.Enqueue(eventNode);
            }
        }

        /// <summary>
        /// 立即触发，不是线程安全
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        public void FireNow(object sender, T e)
        {
            if(e==null)
            {
                throw new Exception($"EventPool: No event for {typeof(T).Name}");
            }
            HandleEvent(sender,e);
        }
        public void SetDefaultHandler(EventHandler<T> handler)=> _defaultHandler = handler;
        private void HandleEvent(object sender, T e)
        {
            bool noHandlerException = false;
            if (_eventHandlers.TryGetValue(e.Id, out var handlers))
            {
                var currentNode=handlers.First;
                while (currentNode != null)
                {
                    _cachedNodes[e]=currentNode.Next;
                    currentNode.Value?.Invoke(sender, e);
                    currentNode = currentNode.Next;
                }
                _cachedNodes.Remove(e);
            }
            else if (_defaultHandler != null)
            {
                _defaultHandler?.Invoke(sender, e);
            }
            else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
            {
                noHandlerException = true;
            }
            ReferencePool.ReferencePool.Release(e);
            if (noHandlerException)
            {
                throw new Exception($"EventPool: No handler for {typeof(T).Name}");
            }
        }
    }
}