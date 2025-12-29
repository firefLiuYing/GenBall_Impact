using GenBall.BattleSystem.Weapons;

namespace GenBall.Accessory
{
    public class AdditiveBonusEnhance : IEnhanceEffect
    {
        public EnhanceType EnhanceType => EnhanceType.AdditiveBonus;
        
        public void Apply(IWeapon weapon)
        {
            
        }

        public void Remove(IWeapon weapon)
        {
            
        }
    }
}