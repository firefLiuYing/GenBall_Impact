using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Player.Controller
{
    public class PlayerMover : CharacterControllerBase, IMove
    {
        private RigidbodyMover _mover;

        private MoveCommand _cachedCommand;
        private bool _commandConsumed;
        public Vector3 Velocity=>_cachedCommand.Velocity;
        public void Move(MoveCommand moveCommand)
        {
            if (_commandConsumed)
            {
                _cachedCommand=moveCommand;
                _commandConsumed = false;
            }
            // 优先级值更大的会覆盖小的，如果是相等的优先级，则后来覆盖先到
            else if (moveCommand.Priority >= _cachedCommand.Priority)
            {
                _cachedCommand = moveCommand;
            }
        }

        public override void Initialize(CharacterState characterState)
        {
            _mover=characterState.GetComponent<RigidbodyMover>();
            _commandConsumed = true;
        }

        public override void Tick(float deltaTime)
        {
            if (_commandConsumed)
            {
                _mover.SetVelocity(Vector3.zero);
            }
            else
            {
                _mover.SetVelocity(_cachedCommand.Velocity);
                _commandConsumed = true;
            }
        }
    }
}