using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Enemy.Controller
{
    public class JumpMover : CharacterControllerBase, IMove
    {
        [SerializeField] private float jumpInterval = 0.5f;
        [SerializeField] private float jumpElevation = 45f;
        [SerializeField] private float jumpForce = 8f;

        private RigidbodyMover _rigidbodyMover;
        private CharacterState _characterState;
        private SphereCollider _collider;
        private bool _onGround;
        private float _onGroundTime;
        private Vector3 _moveDirection;
        private bool _hasDirection;

        public Vector3 Velocity => _hasDirection ? _moveDirection * jumpForce : Vector3.zero;

        public void Move(MoveCommand moveCommand)
        {
            if (!_onGround) return;
            var dir = moveCommand.Velocity;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.001f) return;
            _moveDirection = dir.normalized;
            _hasDirection = true;
        }

        public override void Initialize(CharacterState characterState)
        {
            _characterState = characterState;
            _rigidbodyMover = characterState.GetComponent<RigidbodyMover>();
            _collider = characterState.GetComponentInChildren<SphereCollider>();
            _onGroundTime = 0;
            _hasDirection = false;
            _onGround = false;
        }

        public override void Tick(float deltaTime)
        {
            GroundDetection();

            if (!_onGround)
            {
                _characterState.CanAttack = false;
                return;
            }

            _characterState.CanAttack = true;
            _onGroundTime += deltaTime;

            if (!_hasDirection) return;
            if (_onGroundTime < jumpInterval) return;

            Jump();
        }

        private void Jump()
        {
            _onGroundTime = 0;
            var dir = _moveDirection;
            float rad = Mathf.Deg2Rad * jumpElevation;
            dir.y = Mathf.Sin(rad);
            dir.x *= Mathf.Cos(rad);
            dir.z *= Mathf.Cos(rad);

            _rigidbodyMover.Constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbodyMover.AddForce(dir * jumpForce, ForceMode.Impulse);
            _hasDirection = false;
        }

        private void GroundDetection()
        {
            int exclude = (1 << LayerMask.NameToLayer("Orbis"))
                        | (1 << LayerMask.NameToLayer("OrbisAttack"))
                        | (1 << LayerMask.NameToLayer("Barrier"));
            LayerMask mask = ~exclude;
            var origin = transform.position + _collider.center;
            bool hit = Physics.Raycast(origin, Vector3.down, _collider.radius + 0.01f, mask);

            _rigidbodyMover.Constraints = RigidbodyConstraints.FreezeRotation;
            if (hit && !_onGround)
            {
                _rigidbodyMover.Constraints |= RigidbodyConstraints.FreezePositionX
                                             | RigidbodyConstraints.FreezePositionZ;
            }
            _onGround = hit;
        }
    }
}
