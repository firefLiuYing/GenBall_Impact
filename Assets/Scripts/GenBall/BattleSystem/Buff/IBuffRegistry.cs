using System.Collections.Generic;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff
{
    public interface IBuffRegistry : ISystem
    {
        BuffObj AddBuff(AddBuffInfo info);
        void RemoveBuff(BuffObj buffObj);
        IReadOnlyCollection<BuffObj> ActiveBuffs { get; }
    }
}
