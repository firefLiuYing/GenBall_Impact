namespace GenBall.BattleSystem.Framework
{
    public enum ModifierType
    {
        FlatAdd,
        PercentAdd,
        Multiply
    }

    public class StatModifier
    {
        public ModifierType Type { get; }
        public float Value { get; }

        public StatModifier(ModifierType type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}
