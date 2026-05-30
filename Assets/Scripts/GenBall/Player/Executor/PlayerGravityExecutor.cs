using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: always applies gravity and enforces ground constraint.
    /// Skips when the active action declares <see cref="IArbitratedCommand.BlocksGravity"/>.
    /// All velocity reads/writes go through RigidbodyMover.
    /// </summary>
    public class PlayerGravityExecutor : IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly ICharacterGroundDetect _groundDetect;
        private readonly PlayerConfig _config;
        private readonly CommandDispatcherComponent _dispatcher;

        private float _gravityAcceleration;
        private float _lastGroundedTime = -100f;

        public bool IsOnGround { get; private set; }
        public bool CanJump => Time.time - _lastGroundedTime <= _config.coyoteTime;

        public PlayerGravityExecutor(RigidbodyMover mover,
            ICharacterGroundDetect groundDetect, PlayerConfig config,
            CommandDispatcherComponent dispatcher)
        {
            _mover = mover;
            _groundDetect = groundDetect;
            _config = config;
            _dispatcher = dispatcher;

            _gravityAcceleration = -config.gravityAcceleration;
        }

        public void LogicUpdate(float deltaTime)
        {
            // Active action declares whether gravity should be blocked (e.g., dash)
            if (_dispatcher.ActiveCommand is { BlocksGravity: true })
                return;

            IsOnGround = _groundDetect?.IsOnGround ?? false;

            var velocity = _mover.Velocity;

            if (IsOnGround)
            {
                _lastGroundedTime = Time.time;
                // Only cushion downward velocity on landing.
                // Don't zero upward velocity — jump may have just started.
                if (velocity.y < 0)
                    velocity.y = 0;
            }
            else
            {
                velocity.y += _gravityAcceleration * deltaTime;
            }

            velocity.y = Mathf.Max(-_config.maxDropVelocity, velocity.y);
            _mover.SetVelocity(velocity);
        }
    }
}
