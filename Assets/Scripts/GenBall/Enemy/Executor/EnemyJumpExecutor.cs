using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Enemy.Executor
{
    /// <summary>
    /// Execute layer: handles SPECIAL jumps (dodging, vaulting), NOT routine movement.
    /// Applies a simple upward impulse and tracks jumping state internally.
    /// All velocity writes go through RigidbodyMover.
    /// </summary>
    public class EnemyJumpExecutor : IJump, IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly float _jumpForce;

        private float _jumpStartTime;
        private bool _jumpActive;

        public bool IsJumping { get; private set; }

        public EnemyJumpExecutor(RigidbodyMover mover, float jumpForce)
        {
            _mover = mover;
            _jumpForce = jumpForce;
        }

        public void Jump(JumpCommand command)
        {
            if (command.Phase == JumpPhase.Cancel)
            {
                Cancel();
                return;
            }

            IsJumping = true;
            _jumpActive = true;
            _jumpStartTime = Time.time;

            var velocity = _mover.Velocity;
            velocity.y = _jumpForce;
            _mover.SetVelocity(velocity);
        }

        public void Cancel()
        {
            IsJumping = false;
            _jumpActive = false;
        }

        public void LogicUpdate(float deltaTime)
        {
            // Jump is a one-shot impulse; nothing to sustain per-frame.
            // IsJumping stays true until explicitly Cancel()ed or a new frame cycle resets it
            // (caller can check IsJumping to know if a special jump is in progress).
        }
    }
}
