using System.Runtime.InteropServices;

namespace GenBall.BattleSystem.Command
{
    public enum JumpPhase
    {
        Start,
        Cancel
    }

    [StructLayout(LayoutKind.Auto)]
    public struct JumpCommand : IArbitratedCommand
    {
        public JumpPhase Phase;

        public int InterruptPriority => 3;
        public int AntiInterruptPriority => 3;
        public bool Bufferable => Phase != JumpPhase.Cancel;

        public JumpCommand(JumpPhase phase = JumpPhase.Start)
        {
            Phase = phase;
        }
    }
}
