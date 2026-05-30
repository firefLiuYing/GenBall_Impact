using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: handles variable-height jump via Rigidbody velocity.
    /// Variable height: holding jump applies reduced deceleration (higher jump),
    /// releasing early applies stronger deceleration (shorter jump).
    /// All velocity writes go through RigidbodyMover for pause safety.
    /// </summary>
    public class PlayerJumpExecutor : IJump, IEntityLogicUpdate
    {
        private readonly Rigidbody _rigidbody;
        private readonly RigidbodyMover _mover;
        private readonly ICharacterGroundDetect _groundDetect;

        private readonly float _longPressMaxTime;
        private readonly float _shortPressJustifyTime;

        private float _initialVelocity;
        private float _holdAcceleration;
        private float _releaseAcceleration;

        private enum InternalPhase { None, Holding, Released }
        private InternalPhase _phase = InternalPhase.None;
        private float _jumpStartTime;

        public bool IsJumping => _phase != InternalPhase.None;

        public PlayerJumpExecutor(Rigidbody rigidbody, RigidbodyMover mover, PlayerConfig config,
            ICharacterGroundDetect groundDetect)
        {
            _rigidbody = rigidbody;
            _mover = mover;
            _groundDetect = groundDetect;

            _longPressMaxTime = config.longPressMaxTime;
            _shortPressJustifyTime = config.shortPressJustifyTime;

            InitPhysics(config);
        }

        private void InitPhysics(PlayerConfig config)
        {
            _initialVelocity = 2 * config.longPressJumpMaxHeight / config.longPressMaxTime;
            var pressedAccel = _initialVelocity / config.longPressMaxTime;

            float shortPressPeriodHeight = _initialVelocity * config.shortPressJustifyTime
                - pressedAccel * config.shortPressJustifyTime * config.shortPressJustifyTime / 2;
            float remainHeight = config.shortPressJumpHeight - shortPressPeriodHeight;
            float remainTime = 2 * remainHeight / (_initialVelocity - config.shortPressJustifyTime * pressedAccel);
            var releaseAcceleration = -(_initialVelocity - config.shortPressJustifyTime * pressedAccel) / remainTime;

            // Combined acceleration applied during hold phase
            _holdAcceleration = -pressedAccel - releaseAcceleration;
            // Acceleration applied during release phase (before max time)
            _releaseAcceleration = -releaseAcceleration;
        }

        public void Jump(JumpCommand cmd)
        {
            switch (cmd.Phase)
            {
                case JumpPhase.Start:
                    if (_groundDetect.IsOnGround)
                    {
                        _phase = InternalPhase.Holding;
                        _jumpStartTime = Time.time;

                        var velocity = _rigidbody.velocity;
                        velocity.y = _initialVelocity;
                        _mover.SetVelocity(velocity);
                    }
                    break;

                case JumpPhase.Cancel:
                    if (_phase == InternalPhase.Holding)
                        _phase = InternalPhase.Released;
                    break;
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            if (_phase == InternalPhase.None)
                return;

            var velocity = _rigidbody.velocity;
            float elapsed = Time.time - _jumpStartTime;

            switch (_phase)
            {
                case InternalPhase.Holding:
                    if (elapsed <= _longPressMaxTime)
                    {
                        velocity.y += deltaTime * _holdAcceleration;
                    }
                    else
                    {
                        // Exceeded max hold time, start falling
                        velocity.y += deltaTime * _releaseAcceleration;
                    }
                    break;

                case InternalPhase.Released:
                    velocity.y += deltaTime * _releaseAcceleration;
                    break;
            }

            _mover.SetVelocity(velocity);

            // Landed
            if (_groundDetect.IsOnGround && elapsed > 0.1f)
            {
                _phase = InternalPhase.None;
                var v = _rigidbody.velocity;
                v.y = 0f;
                _mover.SetVelocity(v);
            }
        }
    }
}
