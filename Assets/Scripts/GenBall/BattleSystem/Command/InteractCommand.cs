using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    public enum InteractAction
    {
        Trigger,
        Next,
        Previous,
    }

    /// <summary>
    /// Trigger or scroll interactables. Instantaneous command — fires once per press/scroll.
    /// Priority: 0/0 — can be interrupted by anything, does not interrupt.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct InteractCommand : IArbitratedCommand
    {
        public InteractAction Action;

        public int InterruptPriority => 0;
        public int AntiInterruptPriority => 0;
        public bool Bufferable => false;
        public bool BlocksMove => true;
        public bool BlocksRotate => false;
        public bool BlocksGravity => false;

        public InteractCommand(InteractAction action)
        {
            Action = action;
        }
    }
}
