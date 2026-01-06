using System;

namespace GenBall.BattleSystem.Accessory
{
    public interface IAccessory:IEffect
    {
        public int Load { get; }
    }
}