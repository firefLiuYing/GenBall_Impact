namespace GenBall.BattleSystem.Weapons
{
    public interface IWeaponStats
    {
        public IntStat Damage { get; }
        public FloatStat ImpactForce { get; }
    }
}