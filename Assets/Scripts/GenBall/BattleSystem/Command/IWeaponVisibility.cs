namespace GenBall.BattleSystem.Command
{
    public interface IWeaponVisibility
    {
        void Execute(WeaponVisibilityCommand cmd);
        bool IsTransitioning { get; }
    }
}
