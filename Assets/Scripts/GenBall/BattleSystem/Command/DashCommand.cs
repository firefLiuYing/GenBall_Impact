using System.Runtime.InteropServices;
using UnityEngine;

namespace GenBall.BattleSystem.Command
{
    [StructLayout(LayoutKind.Auto)]
    public struct DashCommand : IArbitratedCommand
    {
        public Vector3 Direction;
        public float Speed;

        public int InterruptPriority => 5;
        public int AntiInterruptPriority => 5;
        public bool Bufferable => false;
        public bool BlocksRotate => true;
        public bool BlocksGravity => true;

        public DashCommand(Vector3 direction, float speed)
        {
            Direction = direction;
            Speed = speed;
        }
    }
}
