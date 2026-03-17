using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Player.Input;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Player.Controller
{
    public class PlayerStateMachine : CharacterControllerBase
    {
        private InputHandler _input;
        private CharacterState _player;
        private Fsm<PlayerStateMachine> _fsm;
        private PlayerConfigSo _config;
        private PhysicsController _physics;
        private PlayerMover _mover;

        private void Awake()
        {
            _input=GetComponent<InputHandler>();
            _physics=GetComponent<PhysicsController>();
        }
        public override void Initialize(CharacterState characterState)
        {
            _config = PlayerConfigProvider.GetOrCreatePlayerConfigSo();
            _player=characterState;
            _mover=_player.GetComponent<PlayerMover>();
            _player = characterState;
            _input.OnViewInputChange += OnViewInputChange;
            
            _player.CanMove = true;
            _player.CanJump = true;
        }

        public override void Tick(float deltaTime)
        {
            var velocity = _mover.Velocity;
            velocity.x=_config.speed*_input.MoveDirection.x;
            velocity.z=_config.speed*_input.MoveDirection.z;
            
            _player.HandleCommand(new MoveCommand(velocity));
        }

        private void OnViewInputChange(Vector2 input)
        {
            // var rotationEulerAngles = _player.transform.rotation.eulerAngles;
            // // unity 沟槽欧拉角小巧思处理
            // if (rotationEulerAngles.x is > 180 and < 360)
            // {
            //     rotationEulerAngles.x -= 360;
            // }
            // rotationEulerAngles.y += input.x * _config.horizontalSensitivity;
            // rotationEulerAngles.x += -input.y * _config.verticalSensitivity;
            // rotationEulerAngles.x=Mathf.Clamp(rotationEulerAngles.x, -80, 80f);
            _player.HandleCommand(new RotateCommand(input.x*_config.horizontalSensitivity, -input.y*_config.verticalSensitivity));
        }

    }
}