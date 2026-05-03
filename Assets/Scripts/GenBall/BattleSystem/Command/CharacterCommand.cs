using System.Runtime.InteropServices;
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
    public struct AttackCommand: ICommand
    {
        public readonly int AttackId;
        public AttackCommand(int attackId)
        {
            AttackId = attackId;
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