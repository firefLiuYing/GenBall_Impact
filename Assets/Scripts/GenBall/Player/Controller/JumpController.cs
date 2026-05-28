using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Config;
using GenBall.Player.Input;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player.Controller
{
    public class JumpController : CharacterControllerBase
    {
        private CharacterState _player;
        private InputHandler  _input;
        private PhysicsController _physics;
        private PlayerMover _mover;
        private bool _jumpCommandConsumed=true;
        private PlayerConfig _config;
        
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _input=characterState.GetComponentInChildren<InputHandler>();
            _physics=characterState.GetComponentInChildren<PhysicsController>();
            _mover=characterState.GetComponent<PlayerMover>();
            _config = SystemRepository.Instance.GetSystem<IConfigProvider>().GetConfig<PlayerConfig>();
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
                // �����ڼ�
                // Debug.Log($"{_input.JumpHoldTime}");
                if (_input.IsJumpPressed && _input.JumpHoldTime <= _config.longPressMaxTime)
                {
                    velocity.y += deltaTime * _pressedAcceleration;
                    // Debug.Log("�����ڼ�");
                }

                // �̰��ڼ��ɿ������
                else if (!_input.IsJumpPressed && _input.JumpHoldTime <= _config.shortPressJustifyTime)
                {
                    velocity.y += deltaTime * _pressedAcceleration;
                    // Debug.Log("�̰��ڼ��ɿ����");
                }

                // ��������ʱ��
                else if (_input.IsJumpPressed && _input.JumpHoldTime > _config.longPressMaxTime)
                {
                    _jumpCommandConsumed=true;
                    // Debug.Log("��������ʱ��");
                }

                // �̰�ʱ��������ɿ�
                else if (!_input.IsJumpPressed && _input.JumpHoldTime > _config.shortPressJustifyTime)
                {
                    _jumpCommandConsumed=true;
                    // Debug.Log("�̰�ʱ��������ɿ�");
                }
            }
            _player.HandleCommand(new MoveCommand(velocity));
        }
        
        private float _pressedAcceleration;     // ��סʱ��˥���ٶ�
        private float _initialVelocity;         // �������ٶ�
        
        private void InitArgs()
        {
            // ���㳤���̰���Ծ����Ҫ�Ĳ���
            // ���ٶ�
            _initialVelocity = 2 * _config.longPressJumpMaxHeight / _config.longPressMaxTime;
            // ��סʱ˥���ٶ�
            _pressedAcceleration = _initialVelocity / _config.longPressMaxTime;
            // �̰������������߶ȣ��м����
            float shortPressPeriodHeight = _initialVelocity * _config.shortPressJustifyTime -_pressedAcceleration * _config.shortPressJustifyTime * _config.shortPressJustifyTime / 2;
            // �̰��ɿ��ڼ�ʣ��Ҫ�����ĸ߶ȣ��м����
            float remainHeight=_config.shortPressJumpHeight-shortPressPeriodHeight;
            // �ɿ������ʱ��
            float remainTime=2 * remainHeight / (_initialVelocity - _config.shortPressJustifyTime * _pressedAcceleration);
            var releasedAcceleration = -(_initialVelocity - _config.shortPressJustifyTime * _pressedAcceleration)/remainTime;
            
            _pressedAcceleration=-_pressedAcceleration-releasedAcceleration;
            Debug.Log($"Pressed acceleration: {_pressedAcceleration} InitialVelocity: {_initialVelocity}");
        }
    }
}