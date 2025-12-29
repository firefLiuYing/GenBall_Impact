using GenBall.BattleSystem.Weapons;

namespace GenBall.Accessory
{
    public interface IEnhanceEffect
    {
        public EnhanceType EnhanceType { get; }
        public void Apply(IWeapon weapon);
        public void Remove(IWeapon weapon);
    }
    public enum EnhanceType
    {
        Undefined,
        
        AdditiveBonus,          // 数值加算
        MultiplicativeBonus,    // 数值乘算
        
        
    }
}