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

        /// <summary>Block MoveCommand routing while this action is active.</summary>
        bool BlocksMove => true;

        /// <summary>Block RotateCommand routing while this action is active.</summary>
        bool BlocksRotate => false;

        /// <summary>Block gravity application while this action is active.</summary>
        bool BlocksGravity => false;
    }
}
