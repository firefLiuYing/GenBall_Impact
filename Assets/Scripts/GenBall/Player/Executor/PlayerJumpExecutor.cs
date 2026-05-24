using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using GenBall.Player.Controller;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player.Executor
{
    public class PlayerJumpExecutor : IJump, IEntityLogicUpdate
    {
        private readonly Rigidbody _rigidbody;
        private readonly PlayerMover _playerMover;
        private readonly InputHandler _input;
        private readonly ICharacterGroundDetect _groundDetect;

        private readonly float _longPressMaxTime;
        private readonly float _shortPressJustifyTime;

        private float _initialVelocity;
        private float _pressedAcceleration;

        public bool IsJumping { get; private set; }

        public PlayerJumpExecutor(Rigidbody rigidbody, PlayerMover playerMover, AppSettingsConfig config, InputHandler input, ICharacterGroundDetect groundDetect)
        {
            _rigidbody = rigidbody;
            _playerMover = playerMover;
            _input = input;
            _groundDetect = groundDetect;

            _longPressMaxTime = config.longPressMaxTime;
            _shortPressJustifyTime = config.shortPressJustifyTime;

            InitPhysics(config);
        }

        private void InitPhysics(AppSettingsConfig config)
        {
            _initialVelocity = 2 * config.longPressJumpMaxHeight / config.longPressMaxTime;
            var pressedAccel = _initialVelocity / config.longPressMaxTime;

            float shortPressPeriodHeight = _initialVelocity * config.shortPressJustifyTime
                - pressedAccel * config.shortPressJustifyTime * config.shortPressJustifyTime / 2;
            float remainHeight = config.shortPressJumpHeight - shortPressPeriodHeight;
            float remainTime = 2 * remainHeight / (_initialVelocity - config.shortPressJustifyTime * pressedAccel);
            var releasedAcceleration = -(_initialVelocity - config.shortPressJustifyTime * pressedAccel) / remainTime;

            _pressedAcceleration = -pressedAccel - releasedAcceleration;
        }

        public void Jump(JumpCommand cmd)
        {
            IsJumping = true;
            _playerMover.LockVertical = true;

            var velocity = _rigidbody.velocity;
            velocity.y = _initialVelocity;
            _rigidbody.velocity = velocity;
        }

        public void LogicUpdate(float deltaTime)
        {
            if (!IsJumping)
                return;

            var velocity = _rigidbody.velocity;

            if (_input.IsJumpPressed && _input.JumpHoldTime <= _longPressMaxTime)
            {
                velocity.y += deltaTime * _pressedAcceleration;
            }
            else if (!_input.IsJumpPressed && _input.JumpHoldTime <= _shortPressJustifyTime)
            {
                velocity.y += deltaTime * _pressedAcceleration;
            }
            else
            {
                IsJumping = false;
            }

            _rigidbody.velocity = velocity;

            if (_groundDetect.IsOnGround)
            {
                IsJumping = false;
            }

            if (!IsJumping)
            {
                _playerMover.LockVertical = false;
                var v = _rigidbody.velocity;
                v.y = 0f;
                _rigidbody.velocity = v;
            }
        }
    }
}
