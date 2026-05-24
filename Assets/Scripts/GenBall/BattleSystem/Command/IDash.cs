namespace GenBall.BattleSystem.Command
{
    public interface IDash
    {
        void Dash(DashCommand command);
        bool IsDashing { get; }
    }
}
