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
        public Quaternion Rotation;

        public RotateCommand(Quaternion rotation)
        {
            Rotation = rotation;
        }
    }
    
    [StructLayout(LayoutKind.Auto)]
    public struct AttackCommand: ICommand
    {
        
    }
}