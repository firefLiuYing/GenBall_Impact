using UnityEngine;

namespace GenBall.BattleSystem.Command
{
    public interface IMove
    {
        public void Move(MoveCommand  moveCommand);
        public Vector3 Velocity { get; }
    }
}