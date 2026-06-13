using System.Runtime.InteropServices;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Command
{
    public interface ICommand
    {

    }

    /// <summary>
    /// Ч��ֻӦ�ó���һ֡
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct MoveCommand : ICommand
    {
        public Vector3 Velocity;
        /// <summary>
        /// ���ȼ�ֵ��ĻḲ��С��
        /// </summary>
        public readonly int Priority;

        public MoveCommand(Vector3 velocity, int priority=0)
        {
            Velocity = velocity;
            Priority = priority;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct RotateCommand : ICommand
    {
        public float HorizontalAngle;
        public float VerticalAngle;

        public RotateCommand(float horizontalAngle, float verticalAngle)
        {
            HorizontalAngle = horizontalAngle;
            VerticalAngle = verticalAngle;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct AttackCommand : IArbitratedCommand
    {
        public readonly int AttackId;
        public readonly ButtonState TriggerState;
        public int InterruptPriority { get; }
        public int AntiInterruptPriority { get; }
        public bool Bufferable => true;
        public bool BlocksMove => true;
        public bool BlocksRotate => false;
        public bool BlocksGravity => false;

        /// <summary>
        /// Default priorities (2/2) represent a normal attack.
        /// Heavy attacks override, e.g. new AttackCommand(id, interruptPriority: 3, antiInterruptPriority: 4).
        /// </summary>
        public AttackCommand(int attackId, ButtonState triggerState = ButtonState.Down,
            int interruptPriority = 2, int antiInterruptPriority = 2)
        {
            AttackId = attackId;
            TriggerState = triggerState;
            InterruptPriority = interruptPriority;
            AntiInterruptPriority = antiInterruptPriority;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct FaceDirectionCommand : ICommand
    {
        public readonly Vector3 Direction;
        public FaceDirectionCommand(Vector3 direction)
        {
            Direction = direction;
        }
    }
}
