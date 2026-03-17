using System.Runtime.InteropServices;
using UnityEngine;

namespace GenBall.BattleSystem.Command
{
    public interface ICommand
    {
        
    }

    /// <summary>
    /// 效果只应该持续一帧
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct MoveCommand : ICommand
    {
        public Vector3 Velocity;
        /// <summary>
        /// 优先级值大的会覆盖小的
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
        
    }
}