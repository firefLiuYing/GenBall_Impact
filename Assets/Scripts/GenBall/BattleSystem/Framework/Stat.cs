using System.Collections.Generic;

namespace GenBall.BattleSystem.Framework
{
    public class Stat
    {
        public float BaseValue { get; private set; }
        public float FinalValue { get; private set; }
        private readonly List<StatModifier> _modifiers = new();

        public Stat(float baseValue = 0f)
        {
            BaseValue = baseValue;
            FinalValue = baseValue;
        }

        public void SetBaseValue(float value)
        {
            BaseValue = value;
            Recalculate();
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Recalculate();
        }

        public void RemoveModifier(StatModifier modifier)
        {
            _modifiers.Remove(modifier);
            Recalculate();
        }

        /// <summary>
        /// Calculate: (Base + flatSum) * (1 + percentSum) * multiplyProduct
        /// </summary>
        private void Recalculate()
        {
            float flatSum = 0f;
            float percentSum = 0f;
            float multiplyProduct = 1f;

            foreach (var m in _modifiers)
            {
                switch (m.Type)
                {
                    case ModifierType.FlatAdd:
                        flatSum += m.Value;
                        break;
                    case ModifierType.PercentAdd:
                        percentSum += m.Value;
                        break;
                    case ModifierType.Multiply:
                        multiplyProduct *= m.Value;
                        break;
                }
            }

            FinalValue = (BaseValue + flatSum) * (1f + percentSum) * multiplyProduct;
        }
    }
}
