using System;
using UnityEngine;

namespace GenBall.Enemy.Attack
{
    public class DashAttackModule : AttackModule
    {
        [Header("攻击距离")] [SerializeField] private float attackRange;
        [Header("准备时间（落地一定时间后才能开始撞击）")][SerializeField]private float preparationTime;
        [Header("上升速度")][SerializeField]private float uppingSpeed;
        [Header("撞击高度偏移（相对于Player世界坐标高度）")] [SerializeField] private float dashHeightOffset;
        [Header("撞击速度")][SerializeField]private float dashSpeed;
        [Header("蓄力时间")] [SerializeField] private float chargingTime;
        
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private bool _canAttack;
        private DashState _dashState;
        private float _curPreparationTime;
        private float _targetHeight;
        private Vector3 _dashDirection;
        private float _curChargingTime;
        private bool _attackJustified;
        
        public override void Initialize()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _collider=GetComponentInChildren<SphereCollider>();

            _canAttack = false;
            _curPreparationTime=0;
        }

        public override void ModuleUpdate(float deltaTime)
        {
            
        }

        public override void ModuleFixedUpdate(float fixedDeltaTime)
        {
            if(!_canAttack) return;
            // todo gzp 攻击判定
            switch (_dashState)
            {
                case DashState.Prepare:
                    var onGround = GroundDetection();
                    if(!onGround) return;
                    _curPreparationTime += fixedDeltaTime;
                    if(_curPreparationTime<preparationTime) return;
                    _targetHeight = GetTargetHeight();
                    _dashDirection = GetTargetDirection();
                    _dashState=DashState.Upping;
                    _rigidbody.useGravity=false;
                    return;
                case DashState.Upping:
                    if (_targetHeight > transform.position.y)
                    {
                        _rigidbody.velocity=uppingSpeed*Vector3.up;
                        return;
                    }
                    _rigidbody.velocity=Vector3.zero;
                    _dashState=DashState.Charging;
                    return;
                case DashState.Charging:
                    if (_curChargingTime < chargingTime)
                    {
                        _curChargingTime += fixedDeltaTime;
                        return;
                    }
                    _dashState=DashState.Shoot;
                    return;
                case DashState.Shoot:
                    _rigidbody.useGravity=true;
                    _attackJustified=false;
                    _rigidbody.AddForce(dashSpeed*_dashDirection,ForceMode.VelocityChange);
                    _dashState = DashState.Dashing;
                    return;
                default:
                    break;
            }
        }

        public override void OnRecycle()
        {
            
        }
        

        public override void StartAttack()
        {
            _canAttack=true;
            _curPreparationTime=0;
            _dashState=DashState.Prepare;
        }
        
        public override void StopAttack()
        {
            _rigidbody.useGravity = true;
            _curPreparationTime = 0;
            _dashDirection=Vector3.zero;
            _dashState=DashState.Prepare;
            _canAttack = false;
            _curChargingTime = 0;
        }

        public override bool CanAttack()
        {
            if (_dashState is DashState.Upping or DashState.Charging or DashState.Shoot) return true;
            var delta=Owner.Target.transform.position-transform.position;
            if (_dashState is DashState.Dashing)
            {
                delta.y=0;
                return delta.magnitude < attackRange;
            }
            var distance = delta.magnitude;
            if (distance > attackRange) return false;
            var targetHeight=GetTargetHeight();
            return targetHeight>=transform.position.y;
        }

        private void OnCollisionEnter(Collision other)
        {
            if(_dashState is not DashState.Dashing) return;
            if(_attackJustified) return;
            var target = other.gameObject.GetComponentInParent<Player.Player>();
            if (target != null)
            {
                _attackJustified=true;
                Debug.Log("打到人喽");
            }
        }

        private Vector3 GetTargetDirection()
        {
            if(Owner?.Target==null)  return Vector3.zero;
            var direction=Owner.Target.transform.position - transform.position;
            direction.y = 0;
            return direction.normalized;
        }
        private float GetTargetHeight()
        {
            if(Owner?.Target==null) return -Mathf.Infinity;
            var targetHeight=Owner.Target.transform.position.y;
            targetHeight += dashHeightOffset;
            return targetHeight;
        }
        private bool GroundDetection()
        {
            int layerToExclude = (1<<LayerMask.NameToLayer("Orbis"))|(1<<LayerMask.NameToLayer("OrbisAttack"))|(1<<LayerMask.NameToLayer("Barrier"));
            LayerMask layerMask=~layerToExclude;
            var origin = transform.position + _collider.center;
            return Physics.Raycast(origin,Vector3.down,_collider.radius+0.01f,layerMask);
        }

        private enum DashState
        {
            Prepare,Upping,Charging,Shoot,Dashing
        }
    }
}