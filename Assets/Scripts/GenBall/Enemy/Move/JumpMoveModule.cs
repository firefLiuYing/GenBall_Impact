using UnityEngine;

namespace GenBall.Enemy.Move
{
    public class JumpMoveModule : MoveModule
    {
        [Header("跳跃停顿时间")] [SerializeField] private float jumpInterval;
        [Header("跳跃仰角")] [SerializeField] private float jumpElevation;
        [Header("跳跃力度")] [SerializeField] private float jumpForce;
        private Rigidbody _rigidbody;
        private SphereCollider _collider; 
        private bool _onGround;
        private float _onGroundTime;
        private bool _canMove;
        private Vector3 _target;
        public override void Initialize()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<SphereCollider>();
            _onGroundTime = 0;
            _canMove = false;
        }

        public override void ModuleUpdate(float deltaTime)
        {
            
        }

        public override void ModuleFixedUpdate(float fixedDeltaTime)
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
            
            _rigidbody.AddForce(direction*jumpForce, ForceMode.Impulse);
        }
        public override void OnRecycle()
        {
            
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
            int layerToExclude = LayerMask.NameToLayer("Orbis");
            LayerMask layerMask=~(1<<layerToExclude);
            var origin = transform.position + _collider.center;
            var hit=Physics.Raycast(origin,Vector3.down,_collider.radius+0.01f,layerMask);
            _onGround = hit;
        }
    }
}