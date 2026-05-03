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
        private static class BuffListPool<T>
        {
            private static readonly Stack<List<T>> SPool = new();

            public static List<T> Rent()
            {
                var list = SPool.Count > 0 ? SPool.Pop() : new List<T>();
                list.Clear();
                return list;
            }

            public static void Return(List<T> list)
            {
                list.Clear();
                SPool.Push(list);
            }
        }

        public static List<T> GetBuffs<T>(this IBuffContainer buffContainer) where T : IBuff
        {
            var result=BuffListPool<T>.Rent();
            foreach (var buff in buffContainer.Buffs)
            {
                if (buff is T buffT)
                {
                    result.Add(buffT);
                }
            }
            return result;
        }

        public static void GetBuffs<T>(this IBuffContainer buffContainer, out List<T> buffs) where T : IBuff
        {
            buffs = BuffListPool<T>.Rent();
            foreach (var buff in buffContainer.Buffs)
            {
                if (buff is T buffT)
                    buffs.Add(buffT);
            }
        }

        public static List<BuffObj> GetBuffs(this IBuffContainer buffContainer, BuffId buffId)
        {
            var buffObjs = buffContainer.GetBuffs<BuffObj>();
            var result = BuffListPool<BuffObj>.Rent();
            foreach (var buff in buffObjs)
            {
                if (buff.BuffId == buffId)
                {
                    result.Add(buff);
                }
            }
            return result;
        }

        public static void GetBuffs(this IBuffContainer buffContainer, BuffId buffId, out List<BuffObj> buffs)
        {
            buffs = BuffListPool<BuffObj>.Rent();
            foreach (var buff in buffContainer.Buffs)
            {
                if (buff is BuffObj obj && obj.BuffId == buffId)
                    buffs.Add(obj);
            }
        }

        public static void ReleaseBuffList<T>(this List<T> list) where T : IBuff
        {
            BuffListPool<T>.Return(list);
        }
    }
}