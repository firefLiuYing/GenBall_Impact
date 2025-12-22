namespace GenBall.BattleSystem
{
    public abstract class StatModifier<T> where T:struct
    {
        public abstract T GetModifyValue(T baseValue);
    }

    public class AddModifier<T> : StatModifier<T> where T : struct
    {
        public T AddValue;
        public override T GetModifyValue(T baseValue)=>AddValue;
    }
    
    public class FloatMultiplyModifier : StatModifier<float>
    {
        public float MultiplyValue;
        public override float GetModifyValue(float baseValue)=>baseValue*MultiplyValue;
    }
    
    public class IntMultiplyModifier : StatModifier<int>
    {
        public float MultiplyValue;

        public override int GetModifyValue(int baseValue)=>(int)(baseValue*MultiplyValue);
    }
    
}