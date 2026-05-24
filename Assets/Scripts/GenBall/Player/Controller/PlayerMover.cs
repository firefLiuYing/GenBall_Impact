using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Player.Controller
{
    public class PlayerMover : CharacterControllerBase, IMove
    {
        private RigidbodyMover _mover;
        private Rigidbody _rigidbody;

        private MoveCommand _cachedCommand;
        private bool _commandConsumed;
        public Vector3 Velocity=>_cachedCommand.Velocity;

        /// <summary>
        /// When set, Tick() preserves the current Y velocity (used by JumpExecutor).
        /// </summary>
        public bool LockVertical { get; set; }

        /// <summary>
        /// When set, Tick() preserves the current X/Z velocity (used by DashExecutor).
        /// </summary>
        public bool LockHorizontal { get; set; }
        public void Move(MoveCommand moveCommand)
        {
            if (_commandConsumed)
            {
                _cachedCommand=moveCommand;
                _commandConsumed = false;
            }
            // ���ȼ�ֵ����ĻḲ��С�ģ��������ȵ����ȼ�������������ȵ�
            else if (moveCommand.Priority >= _cachedCommand.Priority)
            {
                _cachedCommand = moveCommand;
            }
        }

        public override void Initialize(CharacterState characterState)
        {
            _mover=characterState.GetComponent<RigidbodyMover>();
            _rigidbody = _mover.GetComponent<Rigidbody>();
            _commandConsumed = true;
        }

        public override void Tick(float deltaTime)
        {
            Vector3 velocity;
            if (_commandConsumed)
            {
                velocity = Vector3.zero;
            }
            else
            {
                velocity = _cachedCommand.Velocity;
                _commandConsumed = true;
            }

            if (LockVertical || LockHorizontal)
            {
                var currentVel = _rigidbody.velocity;
                if (LockVertical)
                    velocity.y = currentVel.y;
                if (LockHorizontal)
                {
                    velocity.x = currentVel.x;
                    velocity.z = currentVel.z;
                }
            }

            _mover.SetVelocity(velocity);
        }
    }
}