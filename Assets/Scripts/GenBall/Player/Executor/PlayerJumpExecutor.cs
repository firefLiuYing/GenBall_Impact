using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: handles variable-height jump.
    ///
    /// Short press (release within <see cref="PlayerConfig.shortPressJustifyTime"/>):
    ///   button release is deferred to shortPressJustifyTime, producing a fixed height.
    /// Long press (release after shortPressJustifyTime, up to longPressMaxTime):
    ///   actual release time is used, producing variable height.
    /// Long press auto-cutoff: when vertical velocity reaches 0 while holding past
    ///   shortPressJustifyTime, the hold ends (no more upward push).
    ///
    /// Only provides EXTRA acceleration beyond gravity (gravity is always applied
    /// by GravityExecutor). All velocity reads/writes go through RigidbodyMover.
    /// </summary>
    public class PlayerJumpExecutor : IJump, IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly ICharacterGroundDetect _groundDetect;

        private readonly float _longPressMaxTime;
        private readonly float _shortPressJustifyTime;

        private float _initialVelocity;
        private float _holdExtraAcceleration;
        private float _releaseExtraAcceleration;

        private enum InternalPhase { None, Holding, Released }
        private InternalPhase _phase = InternalPhase.None;
        private float _jumpStartTime;

        /// <summary>
        /// When > 0, the scheduled time (since jump start) at which to transition
        /// from Holding to Released. 0 means no release scheduled yet.
        /// </summary>
        private float _scheduledReleaseTime;

        public bool IsJumping => _phase != InternalPhase.None;

        public void Cancel()
        {
            _phase = InternalPhase.None;
            _scheduledReleaseTime = 0f;
        }

        public PlayerJumpExecutor(RigidbodyMover mover, PlayerConfig config,
            ICharacterGroundDetect groundDetect)
        {
            _mover = mover;
            _groundDetect = groundDetect;

            _longPressMaxTime = config.longPressMaxTime;
            _shortPressJustifyTime = config.shortPressJustifyTime;

            InitPhysics(config);
        }

        private void InitPhysics(PlayerConfig config)
        {
            _initialVelocity = 2 * config.longPressJumpMaxHeight / config.longPressMaxTime;

            // Total deceleration while holding (includes gravity in old code)
            var totalPressedAccel = _initialVelocity / config.longPressMaxTime;

            // Total deceleration after release to hit shortPressJumpHeight
            float shortPressPeriodHeight = _initialVelocity * config.shortPressJustifyTime
                - totalPressedAccel * config.shortPressJustifyTime * config.shortPressJustifyTime / 2;
            float remainHeight = config.shortPressJumpHeight - shortPressPeriodHeight;
            float remainTime = 2 * remainHeight
                / (_initialVelocity - config.shortPressJustifyTime * totalPressedAccel);
            var totalReleasedAccel = (_initialVelocity - config.shortPressJustifyTime * totalPressedAccel)
                / remainTime;

            // Subtract gravity — gravity executor always applies it.
            // These are the EXTRA accelerations this executor provides.
            _holdExtraAcceleration = totalPressedAccel - config.gravityAcceleration;
            _releaseExtraAcceleration = totalReleasedAccel - config.gravityAcceleration;
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
                        _scheduledReleaseTime = 0f;

                        var velocity = _mover.Velocity;
                        velocity.y = _initialVelocity;
                        _mover.SetVelocity(velocity);
                    }
                    break;

                case JumpPhase.Cancel:
                    if (_phase == InternalPhase.Holding && _scheduledReleaseTime <= 0f)
                    {
                        float elapsed = Time.time - _jumpStartTime;
                        _scheduledReleaseTime = Mathf.Max(elapsed, _shortPressJustifyTime);
                    }
                    break;
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            if (_phase == InternalPhase.None)
                return;

            var velocity = _mover.Velocity;
            float elapsed = Time.time - _jumpStartTime;

            switch (_phase)
            {
                case InternalPhase.Holding:
                {
                    bool shouldRelease = false;

                    if (_scheduledReleaseTime > 0f && elapsed >= _scheduledReleaseTime)
                        shouldRelease = true;
                    else if (elapsed >= _longPressMaxTime)
                        shouldRelease = true;
                    else if (_scheduledReleaseTime <= 0f && velocity.y <= 0f
                        && elapsed > _shortPressJustifyTime)
                        shouldRelease = true;

                    if (shouldRelease)
                    {
                        _phase = InternalPhase.Released;
                        velocity.y -= _releaseExtraAcceleration * deltaTime;
                    }
                    else
                    {
                        velocity.y -= _holdExtraAcceleration * deltaTime;
                    }
                    break;
                }

                case InternalPhase.Released:
                    velocity.y -= _releaseExtraAcceleration * deltaTime;
                    break;
            }

            _mover.SetVelocity(velocity);

            // Landed — gravity executor was already running, it will take over next frame
            if (_groundDetect.IsOnGround && elapsed > 0.1f)
            {
                _phase = InternalPhase.None;
                _scheduledReleaseTime = 0f;
                var v = _mover.Velocity;
                v.y = 0f;
                _mover.SetVelocity(v);
            }
        }
    }
}
