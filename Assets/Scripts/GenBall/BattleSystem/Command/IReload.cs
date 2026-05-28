namespace GenBall.BattleSystem.Command
{
    public interface IReload
    {
        bool IsReloading { get; }
        void Reload(ReloadCommand cmd);
    }
}
