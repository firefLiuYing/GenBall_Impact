using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GenBall.BattleSystem
{
    public interface IStatValue
    {
        public void ResetStat();
    }

    public abstract class StatValue<TStat> : IStatValue where TStat:struct
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
            Modifiers.Clear();
            Recalculate();
        }
        protected abstract void Recalculate();
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
    }
}