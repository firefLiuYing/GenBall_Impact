using System;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Event
{
    public class CEventRouter:Singleton<CEventRouter>
    {
        private EventDispatcher _dispatcher;
        protected override void Init()
        {
            _dispatcher = new();
            Debug.Log("[EventRouter] Initialized");
        }
        
        #region Subscribe

        public void Subscribe(int id, Action handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1>(int id, Action<T1> handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1, T2>(int id, Action<T1, T2> handler) => _dispatcher.Subscribe(id, handler);
        public void Subscribe<T1, T2, T3>(int id, Action<T1, T2, T3> handler) => _dispatcher.Subscribe(id, handler);

        public void Subscribe<T1, T2, T3, T4>(int id, Action<T1, T2, T3, T4> handler) => _dispatcher.Subscribe(id, handler);

        #endregion

        #region Unsubscribe

        public void Unsubscribe(int id, Action handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1>(int id, Action<T1> handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1, T2>(int id, Action<T1, T2> handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1, T2, T3>(int id, Action<T1, T2, T3> handler) => _dispatcher.Unsubscribe(id, handler);
        public void Unsubscribe<T1, T2, T3, T4>(int id, Action<T1, T2, T3, T4> handler) => _dispatcher.Unsubscribe(id, handler);

        #endregion

        #region Fire

        public void Fire(int id) => _dispatcher.Fire(id);
        public void Fire<T1>(int id, T1 a) => _dispatcher.Fire(id,a);
        public void Fire<T1, T2>(int id, T1 a, T2 b) => _dispatcher.Fire(id,a,b);
        public void Fire<T1, T2, T3>(int id, T1 a, T2 b, T3 c) => _dispatcher.Fire(id,a,b,c);
        public void Fire<T1, T2, T3, T4>(int id, T1 a, T2 b, T3 c, T4 d) => _dispatcher.Fire(id,a,b,c,d);

        #endregion

        #region FireNow

        public void FireNow(int id) => _dispatcher.FireNow(id);
        public void FireNow<T1>(int id, T1 a) => _dispatcher.FireNow(id,a);
        public void FireNow<T1, T2>(int id, T1 a, T2 b) => _dispatcher.FireNow(id,a,b);
        public void FireNow<T1, T2, T3>(int id, T1 a, T2 b, T3 c) => _dispatcher.FireNow(id,a,b,c);
        public void FireNow<T1, T2, T3, T4>(int id, T1 a, T2 b, T3 c, T4 d) => _dispatcher.FireNow(id,a,b,c,d);

        #endregion

        #region Update / Check / Default / Clear

        /// <summary>
        /// 处理延迟事件队列（需要手动调用）
        /// </summary>
        public void Update() => _dispatcher.Update();

        public bool Check(int id, Delegate handler) => _dispatcher.Check(id, handler);

        public void SetDefaultHandler(Action<int> handler) => _dispatcher.SetDefaultHandler(handler);

        public void Clear() => _dispatcher.Clear();

        public void Dispose() => _dispatcher.Dispose();

        #endregion
    }
}