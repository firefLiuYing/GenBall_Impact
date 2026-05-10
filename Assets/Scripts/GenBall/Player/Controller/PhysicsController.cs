using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using UnityEngine;

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
        private PlayerConfigSo _config;
        private PlayerMover _mover;
        
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            #if UNITY_EDITOR
            _config = PlayerConfigProvider.GetOrCreatePlayerConfigSo();
            #else
            _config = null;
            #endif
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
            // 计算长按短按跳跃所需要的参数
            // 初速度
            var initialVelocity = 2 * _config.longPressJumpMaxHeight / _config.longPressMaxTime;
            // 按住时衰减速度
            var pressedAcceleration = initialVelocity / _config.longPressMaxTime;
            // 短按过程中上升高度，中间变量
            float shortPressPeriodHeight = initialVelocity * _config.shortPressJustifyTime -pressedAcceleration * _config.shortPressJustifyTime * _config.shortPressJustifyTime / 2;
            // 短按松开期间剩余要上升的高度，中间变量
            float remainHeight=_config.shortPressJumpHeight-shortPressPeriodHeight;
            // 松开后减速时间
            float remainTime=2 * remainHeight / (initialVelocity - _config.shortPressJustifyTime * pressedAcceleration);
            
            _gravityAccelerationRising = -(initialVelocity - _config.shortPressJustifyTime * pressedAcceleration)/remainTime;
            _gravityAccelerationFalling = -_config.gravityAcceleration;
            Debug.Log($"GravityAccelerationRising: {_gravityAccelerationRising} GravityAccelerationFalling: {_gravityAccelerationFalling}");
        }
    }
}