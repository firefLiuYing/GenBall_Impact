using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: applies gravity and enforces ground constraint.
    /// All velocity writes go through RigidbodyMover for pause safety.
    /// </summary>
    public class PlayerGravityExecutor : IEntityLogicUpdate
    {
        private readonly Rigidbody _rigidbody;
        private readonly RigidbodyMover _mover;
        private readonly ICharacterGroundDetect _groundDetect;
        private readonly PlayerConfig _config;

        private float _gravityAccelerationRising;
        private float _gravityAccelerationFalling;
        private float _lastGroundedTime = -100f;

        public bool IsOnGround { get; private set; }
        public bool CanJump => Time.time - _lastGroundedTime <= _config.coyoteTime;

        public PlayerGravityExecutor(Rigidbody rigidbody, RigidbodyMover mover,
            ICharacterGroundDetect groundDetect, PlayerConfig config)
        {
            _rigidbody = rigidbody;
            _mover = mover;
            _groundDetect = groundDetect;
            _config = config;

            InitPhysics();
        }

        private void InitPhysics()
        {
            var initialVelocity = 2 * _config.longPressJumpMaxHeight / _config.longPressMaxTime;
            var pressedAcceleration = initialVelocity / _config.longPressMaxTime;

            float shortPressPeriodHeight = initialVelocity * _config.shortPressJustifyTime
                - pressedAcceleration * _config.shortPressJustifyTime * _config.shortPressJustifyTime / 2;
            float remainHeight = _config.shortPressJumpHeight - shortPressPeriodHeight;
            float remainTime = 2 * remainHeight / (initialVelocity - _config.shortPressJustifyTime * pressedAcceleration);

            _gravityAccelerationRising = -(initialVelocity - _config.shortPressJustifyTime * pressedAcceleration) / remainTime;
            _gravityAccelerationFalling = -_config.gravityAcceleration;
        }

        public void LogicUpdate(float deltaTime)
        {
            IsOnGround = _groundDetect?.IsOnGround ?? false;

            var velocity = _rigidbody.velocity;

            if (IsOnGround)
            {
                _lastGroundedTime = Time.time;
                velocity.y = 0;
            }
            else if (velocity.y > 0)
            {
                velocity.y += _gravityAccelerationRising * deltaTime;
            }
            else
            {
                velocity.y += _gravityAccelerationFalling * deltaTime;
            }

            velocity.y = Mathf.Max(-_config.maxDropVelocity, velocity.y);
            _mover.SetVelocity(velocity);
        }
    }
}
