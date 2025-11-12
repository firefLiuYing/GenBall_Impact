using System;
using Yueyn.Base.EventPool;
using Yueyn.Main;

namespace Yueyn.Event
{
    public sealed class EventManager:IComponent
    {
        private readonly EventPool<GameEventArgs> _eventPool = new(EventPoolMode.AllowNoHandler|EventPoolMode.AllowMultiHandler);

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Subscribe(id, handler);
        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Unsubscribe(id, handler);
        public void Fire(object sender,GameEventArgs e)=>_eventPool.Fire(sender,e);
        public void FireNow(object sender,GameEventArgs e)=>_eventPool.FireNow(sender,e);
        public void SetDefaultHandler(EventHandler<GameEventArgs> handler)=>_eventPool.SetDefaultHandler(handler);
        public bool Check(int id,EventHandler<GameEventArgs> handler)=>_eventPool.Check(id,handler);
        public void OnRegister()
        {
            
        }

        public void OnUnregister()
        {
            
        }

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            _eventPool.Update(elapsedSeconds, realElapseSeconds);
        }

        public void Shutdown()
        {
            _eventPool.Shutdown();
        }
    }
}