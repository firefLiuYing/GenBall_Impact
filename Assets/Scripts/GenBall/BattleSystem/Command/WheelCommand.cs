namespace GenBall.BattleSystem.Command
{
    public enum WheelAction
    {
        Open,
        Confirm,
        Cancel,
    }

    public struct WheelCommand : IArbitratedCommand
    {
        public readonly WheelAction Action;
        public int InterruptPriority => 5;
        public int AntiInterruptPriority => 5;
        public bool Bufferable => false;
        public bool BlocksMove => true;
        public bool BlocksRotate => true;
        public bool BlocksGravity => false;
        // Open stays active (blocking rotation). Confirm/Cancel clear immediately.
        public System.Func<bool> CompletionCheck { get; }

        public WheelCommand(WheelAction action)
        {
            Action = action;
            CompletionCheck = action == WheelAction.Open ? null : () => false;
        }
    }
}
