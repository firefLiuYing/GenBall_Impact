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

        public WheelCommand(WheelAction action)
        {
            Action = action;
        }
    }
}
