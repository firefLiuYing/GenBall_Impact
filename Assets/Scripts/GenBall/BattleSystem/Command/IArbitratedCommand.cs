namespace GenBall.BattleSystem.Command
{
    /// <summary>
    /// Action-type command that participates in priority arbitration.
    /// Continuous commands (Move, Rotate) do NOT implement this.
    /// </summary>
    public interface IArbitratedCommand : ICommand
    {
        int InterruptPriority { get; }
        int AntiInterruptPriority { get; }
        bool Bufferable { get; }
    }
}
