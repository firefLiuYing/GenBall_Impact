using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: handles dash.
    /// - Invincible phase (invincibleTime): player is invincible, moves at dash speed.
    /// - Ending phase (endingTime): player is vulnerable, stops moving (recovery).
    /// - Can be initiated in air — clears vertical velocity.
    /// - Cancels active jump on activation.
    /// All velocity reads/writes go through RigidbodyMover.
    /// </summary>
    public class PlayerDashExecutor : IDash, IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly BattleEntity _entity;
        private readonly IJump _jumpExecutor;

        private readonly float _invincibleTime;
        private readonly float _endingTime;

        private float _dashSpeed;
        private float _dashStartTime;
        private Vector3 _dashDirection;

        public bool IsDashing { get; private set; }

        public PlayerDashExecutor(RigidbodyMover mover, PlayerConfig config, BattleEntity entity,
            IJump jumpExecutor)
        {
            _mover = mover;
            _entity = entity;
            _jumpExecutor = jumpExecutor;

            _invincibleTime = config.invincibleTime;
            _endingTime = config.endingTime;
            _dashSpeed = config.dashSpeed;
        }

        public void Dash(DashCommand cmd)
        {
            // Cancel active jump
            _jumpExecutor?.Cancel();

            IsDashing = true;
            _dashStartTime = Time.time;
            _dashDirection = cmd.Direction;
            _dashSpeed = cmd.Speed;

            // Set dash velocity (y = 0 — clear vertical for in-air dash)
            _mover.SetVelocity(_dashDirection.normalized * _dashSpeed);

            var damageReceiver = _entity.Get<DamageReceiverComponent>();
            if (damageReceiver != null)
                damageReceiver.IsInvincible = true;
        }

        public void LogicUpdate(float deltaTime)
        {
            if (!IsDashing)
                return;

            float elapsed = Time.time - _dashStartTime;

            if (elapsed < _invincibleTime)
            {
                // Invincible dash phase: maintain dash velocity
                var vel = _mover.Velocity;
                vel.x = _dashDirection.normalized.x * _dashSpeed;
                vel.z = _dashDirection.normalized.z * _dashSpeed;
                vel.y = 0f;
                _mover.SetVelocity(vel);
            }
            else if (elapsed < _invincibleTime + _endingTime)
            {
                // Ending phase: stop movement (recovery), still invincible has ended
                var vel = _mover.Velocity;
                vel.x = 0f;
                vel.z = 0f;
                vel.y = 0f;
                _mover.SetVelocity(vel);
            }
            else
            {
                // Dash complete
                IsDashing = false;

                var damageReceiver = _entity.Get<DamageReceiverComponent>();
                if (damageReceiver != null)
                    damageReceiver.IsInvincible = false;

                // Don't zero y — let gravity take over if airborne.
                // Only zero x/z (dash movement stops).
                var currentVel = _mover.Velocity;
                currentVel.x = 0f;
                currentVel.z = 0f;
                _mover.SetVelocity(currentVel);
            }
        }
    }
}
