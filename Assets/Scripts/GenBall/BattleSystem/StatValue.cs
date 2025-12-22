using System.Collections.Generic;

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
        protected override void Recalculate()
        {
            CurrentValue = BaseValue;
            foreach (var modifier in Modifiers)
            {
                CurrentValue += modifier.GetModifyValue(BaseValue);
            }
        }
    }

    public class FloatStat : StatValue<float>
    {
        protected override void Recalculate()
        {
            CurrentValue = BaseValue;
            foreach (var modifier in Modifiers)
            {
                CurrentValue += modifier.GetModifyValue(BaseValue);
            }
        }
    }
}