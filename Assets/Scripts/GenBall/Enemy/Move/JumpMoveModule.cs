using GenBall.BattleSystem;
using GenBall.BattleSystem.Generated;
using GenBall.BattleSystem.Mover;
using UnityEngine;

namespace GenBall.Enemy.Move
{
    public class JumpMoveModule : MoveModule
    {
        [Header("跳跃停顿时间")] [SerializeField] private float jumpInterval;
        [Header("跳跃仰角")] [SerializeField] private float jumpElevation;
        [Header("跳跃力度")] [SerializeField] private float jumpForce;
        // private Rigidbody _rigidbody;
        private SphereCollider _collider; 
        private bool _onGround;
        private float _onGroundTime;
        private bool _canMove;
        private Vector3 _target;
        private RigidbodyMover _rigidbodyMover;
        public override void Initialize()
        {
            // _rigidbody=GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<SphereCollider>();
            _rigidbodyMover=GetComponent<RigidbodyMover>();
            _onGroundTime = 0;
            _canMove = false;
            
            RegisterEvents();
        }


        private void OnFixedUpdate(float fixedDeltaTime)
        {
            GroundDetection();
            if(!_onGround) return;
            _onGroundTime+= fixedDeltaTime;
            if(!_canMove) return;
            var direction = _target - transform.position;
            direction.y = 0;
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
            if (_onGroundTime >= jumpInterval)
            {
                Jump(direction);
            }
        }

        private void Jump(Vector3 direction)
        {
            _onGroundTime = 0;
            // 通过仰角调整跳跃方向
            direction.y = Mathf.Sin(Mathf.Deg2Rad * jumpElevation);
            direction.x*=Mathf.Cos(Mathf.Deg2Rad * jumpElevation);
            direction.z*=Mathf.Sin(Mathf.Deg2Rad * jumpElevation);
            
            _rigidbodyMover.Constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbodyMover.AddForce(direction*jumpForce, ForceMode.Impulse);
        }
        public override void OnRecycle()
        {
            UnregisterEvents();
        }

        public override void MoveTo(Vector3 target)
        {
            _canMove = true;
            _target = target;
        }

        public override void StopMove()
        {
            _canMove = false;
        }

        private void GroundDetection()
        {
            int layerToExclude = (1<<LayerMask.NameToLayer("Orbis"))|(1<<LayerMask.NameToLayer("OrbisAttack"))|(1<<LayerMask.NameToLayer("Barrier"));
            LayerMask layerMask=~layerToExclude;
            var origin = transform.position + _collider.center;
            var hit=Physics.Raycast(origin,Vector3.down,_collider.radius+0.01f,layerMask);
            _rigidbodyMover.Constraints = RigidbodyConstraints.FreezeRotation;
            if (hit&&!_onGround)
            {
                _rigidbodyMover.Constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }
            _onGround = hit;
        }

        private void RegisterEvents()
        {
            Owner.SubscribeSystemFixedUpdate(OnFixedUpdate);
        }

        private void UnregisterEvents()
        {
            Owner.UnsubscribeSystemFixedUpdate(OnFixedUpdate);
        }
    }
}