using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    [StructLayout(LayoutKind.Auto)]
    public struct StunCommand : IArbitratedCommand
    {
        public float Duration;

        public int InterruptPriority => 10;
        public int AntiInterruptPriority => 10;
        public bool Bufferable => false;

        public StunCommand(float duration)
        {
            Duration = duration;
        }
    }
}
