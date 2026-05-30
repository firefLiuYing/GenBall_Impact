using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Execute layer: handles dash via Rigidbody velocity.
    /// Uses RigidbodyMover for pause-safe velocity writes.
    /// </summary>
    public class PlayerDashExecutor : IDash, IEntityLogicUpdate
    {
        private readonly Rigidbody _rigidbody;
        private readonly RigidbodyMover _mover;
        private readonly BattleEntity _entity;

        private readonly float _invincibleTime;
        private readonly float _endingTime;

        private float _dashSpeed;
        private float _dashStartTime;
        private Vector3 _dashDirection;

        public bool IsDashing { get; private set; }

        public PlayerDashExecutor(Rigidbody rigidbody, RigidbodyMover mover, PlayerConfig config, BattleEntity entity)
        {
            _rigidbody = rigidbody;
            _mover = mover;
            _entity = entity;

            _invincibleTime = config.invincibleTime;
            _endingTime = config.endingTime;
            _dashSpeed = config.dashSpeed;
        }

        public void Dash(DashCommand cmd)
        {
            IsDashing = true;

            _dashStartTime = Time.time;
            _dashDirection = cmd.Direction;
            _dashSpeed = cmd.Speed;

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

            if (elapsed < _invincibleTime + _endingTime)
            {
                _mover.SetVelocity(_dashDirection.normalized * _dashSpeed);
            }
            else
            {
                IsDashing = false;

                var damageReceiver = _entity.Get<DamageReceiverComponent>();
                if (damageReceiver != null)
                    damageReceiver.IsInvincible = false;

                var currentVel = _rigidbody.velocity;
                currentVel.y = 0f;
                _mover.SetVelocity(currentVel);
            }
        }
    }
}
