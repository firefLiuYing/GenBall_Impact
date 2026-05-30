using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Enemy.Executor
{
    /// <summary>
    /// Execute layer: always applies gravity and enforces ground constraint.
    /// Skips when the active action declares BlocksGravity.
    /// All velocity reads/writes go through RigidbodyMover.
    /// </summary>
    public class EnemyGravityExecutor : IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly EnemyConfigSo _config;
        private readonly CommandDispatcherComponent _dispatcher;
        private readonly ICharacterGroundDetect _groundDetect;

        public bool IsOnGround { get; private set; }

        public EnemyGravityExecutor(RigidbodyMover mover,
            EnemyConfigSo config,
            CommandDispatcherComponent dispatcher,
            ICharacterGroundDetect groundDetect)
        {
            _mover = mover;
            _config = config;
            _dispatcher = dispatcher;
            _groundDetect = groundDetect;
        }

        public void LogicUpdate(float deltaTime)
        {
            // Active action declares whether gravity should be blocked (e.g., dash fly phase)
            // Also respect RigidbodyMover.UseGravity — set to false by DashExecutor during attack
            if (_dispatcher.ActiveCommand is { BlocksGravity: true } || !_mover.UseGravity)
                return;

            IsOnGround = _groundDetect?.IsOnGround ?? false;

            var velocity = _mover.Velocity;

            if (IsOnGround)
            {
                // Only cushion downward velocity on landing.
                // Don't zero upward velocity — jump may have just started.
                if (velocity.y < 0)
                    velocity.y = 0;
            }
            else
            {
                velocity.y -= _config.gravityAcceleration * deltaTime;
            }

            velocity.y = Mathf.Max(-_config.maxDropVelocity, velocity.y);
            _mover.SetVelocity(velocity);
        }
    }
}
