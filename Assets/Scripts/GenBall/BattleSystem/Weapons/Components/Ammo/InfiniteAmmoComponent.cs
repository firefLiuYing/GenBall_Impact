namespace GenBall.BattleSystem.Weapons.Components.Ammo
{
    public class InfiniteAmmoComponent : IAmmoSystem
    {
        public AmmoDisplayInfo GetDisplayInfo()
        {
            return new AmmoDisplayInfo { Type = AmmoDisplayType.Infinite };
        }
    }
}
