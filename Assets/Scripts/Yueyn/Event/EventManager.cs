using System;
using UnityEngine;
using Yueyn.Base.EventPool;
using Yueyn.Main;

namespace Yueyn.Event
{
    public sealed class EventManager : MonoBehaviour, IComponent
    {
        public int Priority => 1;

        // 旧系统（保留兼容性）
        private readonly EventPool<GameEventArgs> _eventPool = new(EventPoolMode.AllowNoHandler | EventPoolMode.AllowMultiHandler);

        // 新系统
        // private readonly CEventSystem _newEventSystem = new();

        #region 旧系统 API（保持兼容）

        public void Subscribe(int id, EventHandler<GameEventArgs> handler) => _eventPool.Subscribe(id, handler);
        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler) => _eventPool.Unsubscribe(id, handler);
        public void Fire(object sender, GameEventArgs e) => _eventPool.Fire(sender, e);
        public void FireNow(object sender, GameEventArgs e) => _eventPool.FireNow(sender, e);
        public void SetDefaultHandler(EventHandler<GameEventArgs> handler) => _eventPool.SetDefaultHandler(handler);
        public bool Check(int id, EventHandler<GameEventArgs> handler) => _eventPool.Check(id, handler);

        #endregion

        #region IComponent

        public void Init()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            _eventPool.Update(elapsedSeconds, realElapseSeconds);
            // _newEventSystem.Update();
        }

        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
        }

        public void Shutdown()
        {
            _eventPool.Shutdown();
        }

        #endregion
    }
}