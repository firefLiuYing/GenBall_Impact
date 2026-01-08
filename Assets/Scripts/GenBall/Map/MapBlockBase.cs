using System;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.EventPool;
using Yueyn.Event;

namespace GenBall.Map
{
    public abstract class MapBlockBase : MonoBehaviour, IMapBlock
    {
        private readonly EventPool<GameEventArgs> _eventPool=new(EventPoolMode.AllowNoHandler|EventPoolMode.AllowMultiHandler);
        
        public void EnterMapBlock()
        {
            throw new NotImplementedException();
        }

        public void ExitMapBlock()
        {
            throw new NotImplementedException();
        }
        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Subscribe(id, handler);

        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler)=>_eventPool.Unsubscribe(id, handler);

        public void FireEvent(object sender, GameEventArgs e)=>_eventPool.Fire(sender, e);

        public void FireNow(object sender, GameEventArgs e)=>_eventPool.FireNow(sender, e);

        public void AddEffect(IEffect effect)
        {
            throw new NotImplementedException();
        }

        public bool RemoveEffect(IEffect effect)
        {
            throw new NotImplementedException();
        }

    }
}