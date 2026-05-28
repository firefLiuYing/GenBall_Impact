using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public interface IStatValue
    {
        public void ResetStat();
    }

    public abstract class StatValue<TStat> :IReference, IStatValue where TStat:struct
    {
        public TStat BaseValue;
        public TStat CurrentValue;
        protected readonly List<StatModifier<TStat>> Modifiers = new();
        public Action<TStat>  OnValueChange;

        public StatValue(TStat baseValue)
        {
            SetBaseValue(baseValue);
        }

        public StatValue()
        {
            SetBaseValue(default(TStat));
        }

        public void SetBaseValue(TStat baseValue)
        {
            BaseValue = baseValue;
            Recalculate();
        }
        public void AddModifier(StatModifier<TStat> modifier)
        {
            Modifiers.Add(modifier);
            Recalculate();
        }

        public void RemoveModifier(StatModifier<TStat> modifier)
        {
            Modifiers.Remove(modifier);
            Recalculate();
        }

        public void ResetStat()
        {
            OnValueChange = null;
            BaseValue = default(TStat);
            foreach (var modifier in Modifiers)
            {
                modifier.Clear();
            }
            Modifiers.Clear();
            Recalculate();
        }
        protected abstract void Recalculate();
        public void Clear()
        {
            ResetStat();
        }
    }
    public class IntStat: StatValue<int>
    {
        public IntStat():base(0){}
        public IntStat(int baseValue):base(baseValue){}
        protected override void Recalculate()
        {
            CurrentValue = BaseValue;
            foreach (var modifier in Modifiers)
            {
                CurrentValue += modifier.GetModifyValue(BaseValue);
            }
            OnValueChange?.Invoke(CurrentValue);
        }

        public static IntStat Create(int baseValue=0)
        {
            var stat=ReferencePool.Acquire<IntStat>();
            stat.SetBaseValue(baseValue);
            return stat;
        }
    }

    public class FloatStat : StatValue<float>
    {
        public FloatStat():base(0){}
        public FloatStat(float baseValue):base(baseValue){}
        protected override void Recalculate()
        {
            CurrentValue = BaseValue;
            foreach (var modifier in Modifiers)
            {
                CurrentValue += modifier.GetModifyValue(BaseValue);
            }
            OnValueChange?.Invoke(CurrentValue);
        }

        public static FloatStat Create(float baseValue=0)
        {
            var stat=ReferencePool.Acquire<FloatStat>();
            stat.SetBaseValue(baseValue);
            return stat;
        }
    }
}