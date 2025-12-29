using System;

namespace GenBall.BattleSystem
{
    public interface IEffectable
    {
        public void AddEffect(IEffect effect);
        public bool RemoveEffect(IEffect effect);
        public void Subscribe(int id, EventHandler<EffectEventArgs> handler);
        public void Unsubscribe(int id, EventHandler<EffectEventArgs> handler);
    }
}