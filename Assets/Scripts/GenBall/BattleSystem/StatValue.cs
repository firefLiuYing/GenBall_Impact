using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GenBall.BattleSystem
{
    public interface IStatValue
    {
        
    }

    public abstract class StatValue<T> : IStatValue where T:struct
    {
        public T BaseValue;
        public T CurrentValue;
        protected readonly List<StatModifier<T>> Modifiers = new();

        public StatValue(T baseValue)
        {
            SetBaseValue(baseValue);
        }

        public StatValue()
        {
            SetBaseValue(default(T));
        }

        public void SetBaseValue(T baseValue)
        {
            BaseValue = baseValue;
            Recalculate();
        }
        public void AddModifier(StatModifier<T> modifier)
        {
            Modifiers.Add(modifier);
            Recalculate();
        }

        public void RemoveModifier(StatModifier<T> modifier)
        {
            Modifiers.Remove(modifier);
            Recalculate();
        }

        protected abstract void Recalculate();
    }
    public class IntStat: StatValue<int>
    {
        public IntStat():base(0){}
        public IntStat(int baseValue):base(baseValue){}
        public Action<int> OnValueChange;
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
        public Action<float> OnValueChange;
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