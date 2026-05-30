using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: handles horizontal movement (XZ) from MoveCommand.
    /// Only touches XZ velocity — Y is managed by Jump and Gravity executors.
    /// All velocity reads/writes go through RigidbodyMover.
    /// </summary>
    public class PlayerMoveExecutor : IMove
    {
        private RigidbodyMover _mover;
        private float _speed;

        private MoveCommand _cachedCommand;
        private bool _commandConsumed = true;

        public Vector3 Velocity => _cachedCommand.Velocity;

        public void Init(RigidbodyMover mover, float speed)
        {
            _mover = mover;
            _speed = speed;
        }

        public void Move(MoveCommand moveCommand)
        {
            if (_commandConsumed)
            {
                _cachedCommand = moveCommand;
                _commandConsumed = false;
            }
            else if (moveCommand.Priority >= _cachedCommand.Priority)
            {
                _cachedCommand = moveCommand;
            }

            // Preserve current Y (managed by jump/gravity executors)
            Vector3 velocity = _mover.Velocity;

            if (!_commandConsumed)
            {
                velocity.x = _cachedCommand.Velocity.x * _speed;
                velocity.z = _cachedCommand.Velocity.z * _speed;
                _commandConsumed = true;
            }
            else
            {
                velocity.x = 0;
                velocity.z = 0;
            }

            _mover.SetVelocity(velocity);
        }
    }
}
