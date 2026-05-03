using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Navigation;
using UnityEngine;

namespace GenBall.Enemy.Controller
{
    public class EnemyMover : CharacterControllerBase, IMove
    {
        private RigidbodyMover _rigidbodyMover;
        private INavigator _navigator;
        private MoveCommand _cachedCommand;
        private bool _commandConsumed = true;

        public Vector3 Velocity => _cachedCommand.Velocity;

        public void Move(MoveCommand moveCommand)
        {
            Debug.Log(moveCommand.Velocity);
            if (_commandConsumed)
            {
                _cachedCommand = moveCommand;
                _commandConsumed = false;
            }
            else if (moveCommand.Priority >= _cachedCommand.Priority)
            {
                _cachedCommand = moveCommand;
            }
        }

        public override void Initialize(CharacterState characterState)
        {
            _rigidbodyMover = characterState.GetComponent<RigidbodyMover>();
            characterState.TryGetComponent(out _navigator);
            _navigator ??= characterState.gameObject.AddComponent<DirectNavigator>();
            _commandConsumed = true;
        }

        public override void Tick(float deltaTime)
        {
            if (_commandConsumed)
            {
                _rigidbodyMover.SetVelocity(Vector3.zero);
                return;
            }

            var actualVelocity = _navigator.CalculateVelocity(
                _cachedCommand.Velocity, transform.position);
            _rigidbodyMover.SetVelocity(actualVelocity);
            _commandConsumed = true;
        }
    }
}
