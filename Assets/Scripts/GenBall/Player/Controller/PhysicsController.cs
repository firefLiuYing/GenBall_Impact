using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Config;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player.Controller
{
    public class PhysicsController : CharacterControllerBase
    {
        private ICharacterGroundDetect _groundDetect;
        private CharacterState _player;
        [SerializeField] private float coyoteTime=0.01f;
        public bool IsOnGround { get;private set; }
        public bool CanJump=>Time.time - _lastGroundedTime <= coyoteTime;
        private float _lastGroundedTime=-100f;
        private AppSettingsConfig _config;
        private PlayerMover _mover;
        
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _config = SystemRepository.Instance.GetSystem<IConfigProvider>().GetConfig<AppSettingsConfig>();
            InitArgs();
            _player.TryGetComponent(out _groundDetect);
            _mover = _player.GetComponent<PlayerMover>();
        }

        public override void Tick(float deltaTime)
        {
            IsOnGround = _groundDetect?.IsOnGround??false;
            var velocity = _mover.Velocity;
            if (IsOnGround)
            {
                _lastGroundedTime = Time.time;
                velocity.y = 0;
            }
            else if(velocity.y>0)
            {
                velocity.y +=  _gravityAccelerationRising* deltaTime;
            }
            else
            {
                velocity.y+=  _gravityAccelerationFalling * deltaTime;
            }
            velocity.y=Mathf.Max(-_config.maxDropVelocity, velocity.y);
            _player.HandleCommand(new MoveCommand(velocity));
        }
        
        private float _gravityAccelerationRising;
        private float _gravityAccelerationFalling;
        
        private void InitArgs()
        {
            // ���㳤���̰���Ծ����Ҫ�Ĳ���
            // ���ٶ�
            var initialVelocity = 2 * _config.longPressJumpMaxHeight / _config.longPressMaxTime;
            // ��סʱ˥���ٶ�
            var pressedAcceleration = initialVelocity / _config.longPressMaxTime;
            // �̰������������߶ȣ��м����
            float shortPressPeriodHeight = initialVelocity * _config.shortPressJustifyTime -pressedAcceleration * _config.shortPressJustifyTime * _config.shortPressJustifyTime / 2;
            // �̰��ɿ��ڼ�ʣ��Ҫ�����ĸ߶ȣ��м����
            float remainHeight=_config.shortPressJumpHeight-shortPressPeriodHeight;
            // �ɿ������ʱ��
            float remainTime=2 * remainHeight / (initialVelocity - _config.shortPressJustifyTime * pressedAcceleration);
            
            _gravityAccelerationRising = -(initialVelocity - _config.shortPressJustifyTime * pressedAcceleration)/remainTime;
            _gravityAccelerationFalling = -_config.gravityAcceleration;
            Debug.Log($"GravityAccelerationRising: {_gravityAccelerationRising} GravityAccelerationFalling: {_gravityAccelerationFalling}");
        }
    }
}