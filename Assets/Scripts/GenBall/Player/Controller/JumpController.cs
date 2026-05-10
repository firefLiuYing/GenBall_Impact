using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player.Controller
{
    public class JumpController : CharacterControllerBase
    {
        private CharacterState _player;
        private InputHandler  _input;
        private PhysicsController _physics;
        private PlayerMover _mover;
        private bool _jumpCommandConsumed=true;
        private PlayerConfigSo _config;
        
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _input=characterState.GetComponentInChildren<InputHandler>();
            _physics=characterState.GetComponentInChildren<PhysicsController>();
            _mover=characterState.GetComponent<PlayerMover>();
            #if UNITY_EDITOR
            _config = PlayerConfigProvider.GetOrCreatePlayerConfigSo();
            #else
            _config = null;
            #endif
            InitArgs();
        }

        public override void Tick(float deltaTime)
        {
            var velocity = _mover.Velocity;
            if (_player.CanJump && _physics.CanJump && _input.ConsumeBufferedJump())
            {
                velocity.y = _initialVelocity;
                _jumpCommandConsumed = false;
                _player.HandleCommand(new MoveCommand(velocity));
                return;
            }

            if (!_jumpCommandConsumed)
            {
                // 长按期间
                // Debug.Log($"{_input.JumpHoldTime}");
                if (_input.IsJumpPressed && _input.JumpHoldTime <= _config.longPressMaxTime)
                {
                    velocity.y += deltaTime * _pressedAcceleration;
                    // Debug.Log("长按期间");
                }

                // 短按期间松开的情况
                else if (!_input.IsJumpPressed && _input.JumpHoldTime <= _config.shortPressJustifyTime)
                {
                    velocity.y += deltaTime * _pressedAcceleration;
                    // Debug.Log("短按期间松开情况");
                }

                // 超出长按时间
                else if (_input.IsJumpPressed && _input.JumpHoldTime > _config.longPressMaxTime)
                {
                    _jumpCommandConsumed=true;
                    // Debug.Log("超出长按时间");
                }

                // 短按时间结束后松开
                else if (!_input.IsJumpPressed && _input.JumpHoldTime > _config.shortPressJustifyTime)
                {
                    _jumpCommandConsumed=true;
                    // Debug.Log("短按时间结束后松开");
                }
            }
            _player.HandleCommand(new MoveCommand(velocity));
        }
        
        private float _pressedAcceleration;     // 按住时的衰减速度
        private float _initialVelocity;         // 起跳初速度
        
        private void InitArgs()
        {
            // 计算长按短按跳跃所需要的参数
            // 初速度
            _initialVelocity = 2 * _config.longPressJumpMaxHeight / _config.longPressMaxTime;
            // 按住时衰减速度
            _pressedAcceleration = _initialVelocity / _config.longPressMaxTime;
            // 短按过程中上升高度，中间变量
            float shortPressPeriodHeight = _initialVelocity * _config.shortPressJustifyTime -_pressedAcceleration * _config.shortPressJustifyTime * _config.shortPressJustifyTime / 2;
            // 短按松开期间剩余要上升的高度，中间变量
            float remainHeight=_config.shortPressJumpHeight-shortPressPeriodHeight;
            // 松开后减速时间
            float remainTime=2 * remainHeight / (_initialVelocity - _config.shortPressJustifyTime * _pressedAcceleration);
            var releasedAcceleration = -(_initialVelocity - _config.shortPressJustifyTime * _pressedAcceleration)/remainTime;
            
            _pressedAcceleration=-_pressedAcceleration-releasedAcceleration;
            Debug.Log($"Pressed acceleration: {_pressedAcceleration} InitialVelocity: {_initialVelocity}");
        }
    }
}