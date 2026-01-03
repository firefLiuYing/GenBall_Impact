using System;
using GenBall.Event;

namespace GenBall.BattleSystem
{
    public interface IEffectable:ILocalEventManager
    {
        public void AddEffect(IEffect effect);
        public bool RemoveEffect(IEffect effect);
    }
}