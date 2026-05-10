using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Enemy.Controller
{
    public class EnemyFaceController : CharacterControllerBase, IFaceDirection
    {
        [SerializeField] private float rotateSpeed = 720f;
        private Quaternion _targetRotation;
        private bool _hasTarget;
        private RigidbodyMover _rigidbodyMover;

        public void Face(FaceDirectionCommand command)
        {
            if (command.Direction.sqrMagnitude < 0.001f)
            {
                _hasTarget = false;
                return;
            }

            var flatDir = new Vector3(command.Direction.x, 0, command.Direction.z);
            if (flatDir.sqrMagnitude < 0.001f)
            {
                _hasTarget = false;
                return;
            }

            _targetRotation = Quaternion.LookRotation(flatDir);
            _hasTarget = true;
        }

        public override void Initialize(CharacterState characterState)
        {
            _rigidbodyMover = GetComponent<RigidbodyMover>();
        }

        public override void Tick(float deltaTime)
        {
            if (!_hasTarget) return;
            _rigidbodyMover.SetRotation( Quaternion.RotateTowards(transform.rotation, _targetRotation, rotateSpeed * deltaTime));
        }
    }
}
