using System.Runtime.InteropServices;
using UnityEngine;

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
        public Vector3 Velocity;
        public JumpPhase Phase;

        public int InterruptPriority => Phase == JumpPhase.Cancel ? int.MaxValue : 3;
        public int AntiInterruptPriority => Phase == JumpPhase.Cancel ? int.MaxValue : 3;
        public bool Bufferable => Phase != JumpPhase.Cancel;

        public JumpCommand(Vector3 velocity, JumpPhase phase = JumpPhase.Start)
        {
            Velocity = velocity;
            Phase = phase;
        }
    }
}
