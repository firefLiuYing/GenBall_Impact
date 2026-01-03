using System;
using GenBall.BattleSystem;
using Yueyn.Event;

namespace GenBall.Event
{
    public interface ILocalEventManager
    {
        public void Subscribe(int id, EventHandler<GameEventArgs> handler);
        public void Unsubscribe(int id, EventHandler<GameEventArgs> handler);
        public void FireEvent(object sender,GameEventArgs e);
        public void FireNow(object sender,GameEventArgs e);
    }
}