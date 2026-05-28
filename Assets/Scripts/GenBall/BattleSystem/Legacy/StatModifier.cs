using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem
{
    public abstract class StatModifier<T>:IReference where T:struct
    {
        public abstract T GetModifyValue(T baseValue);
        public abstract void Clear();
    }

    public class AddModifier<T> : StatModifier<T> where T : struct
    {
        public T AddValue;
        public override T GetModifyValue(T baseValue)=>AddValue;
        public static StatModifier<T> Create(T baseValue)
        {
            var modifier = ReferencePool.Acquire<AddModifier<T>>();
            modifier.AddValue = baseValue;
            return modifier;
        }

        public override void Clear()
        {
            AddValue=default;
        }
    }
    
    public class FloatMultiplyModifier : StatModifier<float>
    {
        public float MultiplyValue;
        public override float GetModifyValue(float baseValue)=>baseValue*MultiplyValue;
        public static StatModifier<float> Create(float baseValue)
        {
            var  modifier = ReferencePool.Acquire<FloatMultiplyModifier>();
            modifier.MultiplyValue = baseValue;
            return modifier;
        }

        public override void Clear()
        {
            MultiplyValue=0;
        }
    }
    
    public class IntMultiplyModifier : StatModifier<int>
    {
        public float MultiplyValue;

        public override int GetModifyValue(int baseValue)=>(int)(baseValue*MultiplyValue);
        public static StatModifier<int> Create(int baseValue)
        {
            var modifier = ReferencePool.Acquire<IntMultiplyModifier>();
            modifier.MultiplyValue = baseValue;
            return modifier;
        }

        public override void Clear()
        {
            MultiplyValue=0;
        }
    }
    
}