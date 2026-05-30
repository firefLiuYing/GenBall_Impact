namespace GenBall.BattleSystem.Weapons.Components.Ammo
{
    /// <summary>All ammo systems report their display state for UI.</summary>
    public interface IAmmoSystem
    {
        AmmoDisplayInfo GetDisplayInfo();
    }

    public enum AmmoDisplayType { Magazine, Heat, Charge, Infinite }

    public struct AmmoDisplayInfo
    {
        public AmmoDisplayType Type;
        public int CurrentValue;
        public int MaxValue;
        public float NormalizedValue;
    }
}
