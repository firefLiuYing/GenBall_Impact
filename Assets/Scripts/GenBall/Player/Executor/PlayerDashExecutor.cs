using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.Framework.Entity;
using GenBall.Player.Controller;
using UnityEngine;

namespace GenBall.Player.Executor
{
    public class PlayerDashExecutor : IDash, IEntityLogicUpdate
    {
        private readonly Rigidbody _rigidbody;
        private readonly PlayerMover _playerMover;
        private readonly BattleEntity _entity;

        private readonly float _invincibleTime;
        private readonly float _endingTime;

        private float _dashSpeed;
        private float _dashStartTime;
        private Vector3 _dashDirection;

        public bool IsDashing { get; private set; }

        public PlayerDashExecutor(Rigidbody rigidbody, PlayerMover playerMover, PlayerConfig config, BattleEntity entity)
        {
            _rigidbody = rigidbody;
            _playerMover = playerMover;
            _entity = entity;

            _invincibleTime = config.invincibleTime;
            _endingTime = config.endingTime;
            _dashSpeed = config.dashSpeed;
        }

        public void Dash(DashCommand cmd)
        {
            IsDashing = true;
            _playerMover.LockHorizontal = true;
            _playerMover.LockVertical = true;

            _dashStartTime = Time.time;
            _dashDirection = cmd.Direction;
            _dashSpeed = cmd.Speed;

            _rigidbody.velocity = _dashDirection.normalized * _dashSpeed;

            // Enable invincibility during dash
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
                _rigidbody.velocity = _dashDirection.normalized * _dashSpeed;
            }
            else if (elapsed < _invincibleTime + _endingTime)
            {
                _rigidbody.velocity = _dashDirection.normalized * _dashSpeed;
            }
            else
            {
                IsDashing = false;
                _playerMover.LockHorizontal = false;
                _playerMover.LockVertical = false;

                // Disable invincibility
                var damageReceiver = _entity.Get<DamageReceiverComponent>();
                if (damageReceiver != null)
                    damageReceiver.IsInvincible = false;

                var currentVel = _rigidbody.velocity;
                currentVel.y = 0f;
                _rigidbody.velocity = currentVel;
            }
        }
    }
}
