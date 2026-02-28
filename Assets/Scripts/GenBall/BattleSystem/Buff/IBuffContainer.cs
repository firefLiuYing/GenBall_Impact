using System;
using System.Collections.Generic;
using System.Linq;

namespace GenBall.BattleSystem.Buff
{
    public interface IBuffContainer
    {
        public IReadOnlyList<IBuff> Buffs { get; }
        public void AddBuff(IBuff buff);
        public void RemoveBuff(IBuff buff);
    }

    public static class BuffContainerExtensions
    {
        public static List<T> GetBuffs<T>(this IBuffContainer buffContainer) where T : IBuff
        {
            var result=new  List<T>();
            foreach (var buff in buffContainer.Buffs)
            {
                if (buff is T buffT)
                {
                    result.Add(buffT);
                }
            }
            return result;
        }

        public static List<BuffObj> GetBuffs(this IBuffContainer buffContainer, BuffId buffId)
        {
            var buffObjs = buffContainer.GetBuffs<BuffObj>();
            var result = new List<BuffObj>();
            foreach (var buff in buffObjs)
            {
                if (buff.BuffId == buffId)
                {
                    result.Add(buff);
                }
            }
            return result;
        }
    }
}