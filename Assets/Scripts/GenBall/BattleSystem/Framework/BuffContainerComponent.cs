using System.Collections.Generic;
using GenBall.BattleSystem.Buff;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Shared IBuffContainer implementation for BattleEntity.
    /// Uses SortedSet with DefaultComparerBuff for priority-based ordering.
    /// </summary>
    public class BuffContainerComponent : IBuffContainer
    {
        private readonly SortedSet<IBuff> _buffs = new(new DefaultComparerBuff());

        public IReadOnlyList<IBuff> Buffs
        {
            get
            {
                var list = new List<IBuff>(_buffs.Count);
                list.AddRange(_buffs);
                return list;
            }
        }

        public void AddBuff(IBuff buff)
        {
            _buffs.Add(buff);
        }

        public void RemoveBuff(IBuff buff)
        {
            _buffs.Remove(buff);
        }
    }
}
