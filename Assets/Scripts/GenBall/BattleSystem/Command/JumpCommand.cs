using System.Runtime.InteropServices;
using UnityEngine;

namespace GenBall.BattleSystem.Command
{
    [StructLayout(LayoutKind.Auto)]
    public struct JumpCommand : IArbitratedCommand
    {
        public Vector3 Velocity;

        public int InterruptPriority => 3;
        public int AntiInterruptPriority => 3;
        public bool Bufferable => true;

        public JumpCommand(Vector3 velocity)
        {
            Velocity = velocity;
        }
    }
}
