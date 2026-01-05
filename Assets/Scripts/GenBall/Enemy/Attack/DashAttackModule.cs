using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Attack
{
    public class DashAttackModule : AttackModule
    {
        [Header("伤害")] [SerializeField] private int damage;
        [Header("攻击距离")] [SerializeField] private float attackRange;
        [Header("准备时间（落地一定时间后才能开始撞击）")][SerializeField]private float preparationTime;
        [Header("上升速度")][SerializeField]private float uppingSpeed;
        [Header("上升高度偏移")][SerializeField][Tooltip("相对于Player世界坐标高度的偏移量，决定Orbis准备撞击前上升的高度")] private float flyHeightOffset;
        [Header("撞击高度偏移")][SerializeField][Tooltip("相对于Player世界坐标高度的偏移量，决定Orbis撞击时目标位置的高度")] private float dashHeightOffset;
        [Header("撞击速度")][SerializeField]private float dashSpeed;
        [Header("蓄力时间")] [SerializeField] private float chargingTime;
        [Header("反弹力")] [SerializeField] private float reboundForce;
        
        private Rigidbody _rigidbody;
        private SphereCollider _collider;

        private Fsm<DashAttackModule> _fsm;
        private readonly List<FsmState<DashAttackModule>> _states = new();

        private bool _canAttack;
        private Variable<bool> _shouldAttack;

        private float _targetFlyHeight;
        private Vector3 _targetPos;
        
        public override void Initialize()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _collider=GetComponentInChildren<SphereCollider>();
            
            _states.Clear();
            _states.Add(new IdleState());
            _states.Add(new PrepareState());
            _states.Add(new FlyState());
            _states.Add(new ChargeState());
            _states.Add(new DashState());
            _states.Add(new ReboundState());

            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm($"DashAttackFsm_{GetHashCode()}", this, _states);
            RegisterFsmDatas();
            RegisterEvents();

            // _fsm.PrintLog = true;
            _fsm.Start<IdleState>();
        }

        private void RegisterFsmDatas()
        {
            _shouldAttack = ReferencePool.Acquire<Variable<bool>>();
            _fsm.SetData("ShouldAttack",_shouldAttack);
        }

        private void RegisterEvents()
        {
            
        }

        public override void OnRecycle()
        {
            GameEntry.GetModule<FsmManager>().DestroyFsm(_fsm);
        }
        

        public override void StartAttack()
        {
            _shouldAttack.PostValue(true);
        }
        
        public override void StopAttack()
        {
            _shouldAttack.PostValue(false);
        }

        public override bool CanAttack() => _canAttack;

        private bool GroundDetection(bool detectPlayer=true)
        {
            int layerToExclude = (1<<LayerMask.NameToLayer("Orbis"))|(1<<LayerMask.NameToLayer("OrbisAttack"))|(1<<LayerMask.NameToLayer("Barrier"));
            if(!detectPlayer) layerToExclude|=1<<LayerMask.NameToLayer("Player");
            LayerMask layerMask=~layerToExclude;
            var origin = transform.position + _collider.center;
            return Physics.Raycast(origin,Vector3.down,_collider.radius+0.01f,layerMask);
        }


        private abstract class BaseState : FsmState<DashAttackModule>
        {
            private Fsm<DashAttackModule> _fsm;
            protected DashAttackModule Owner => _fsm.Owner;
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                _fsm = fsm;
            }

            protected void ChangeState<TState>() where TState : BaseState => _fsm.ChangeState<TState>();
            protected TData GetData<TData>(string name) where TData : Variable => _fsm.GetData<TData>(name);
        }
        private class IdleState : BaseState
        {
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                RegisterEvents();
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                // 待机时，如果player在攻击范围且在一定高度以下内就认为可以发起攻击
                Owner._canAttack = false;
                if(Owner.Owner.Target==null) return;
                var targetPos = Owner.Owner.Target.transform.position;
                var distance=Vector3.Distance(Owner.transform.position,targetPos);
                if(distance>Owner.attackRange) return;
                Owner._canAttack = true;
            }

            protected internal override void OnExit(Fsm<DashAttackModule> fsm, bool isShutdown = false)
            {
                UnregisterEvents();
            }

            private void RegisterEvents()
            {
                GetData<Variable<bool>>("ShouldAttack").Observe(OnShouldAttackChanged);
            }

            private void UnregisterEvents()
            {
                GetData<Variable<bool>>("ShouldAttack").Unobserve(OnShouldAttackChanged);
            }

            private void OnShouldAttackChanged(bool shouldAttack)
            {
                if(!shouldAttack) return;
                ChangeState<PrepareState>();
            }
        }

        private class PrepareState : BaseState
        {
            private float _curPrepareTime;
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbody.useGravity = true;
                _curPrepareTime = 0f;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                Owner._canAttack = false;
                if(Owner.Owner.Target==null) return;
                var targetPos = Owner.Owner.Target.transform.position;
                var distance=Vector3.Distance(Owner.transform.position,targetPos);
                if(distance>Owner.attackRange) return;
                Owner._canAttack = true;
                
                // var targetPos = Owner.Owner.Target.transform.position;
                var onGround = Owner.GroundDetection();
                if (!onGround)
                {
                    _curPrepareTime = 0f;
                    return;
                }
                _curPrepareTime += fixeDeltaTime;
                if (_curPrepareTime < Owner.preparationTime) return;
                Owner._targetPos=targetPos+Owner.dashHeightOffset*Vector3.up;
                Owner._targetFlyHeight = targetPos.y + Owner.flyHeightOffset;
                ChangeState<FlyState>();
            }
        }

        private class FlyState : BaseState
        {
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbody.useGravity = false;
                Owner._rigidbody.velocity =Owner.uppingSpeed * Vector3.up;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                if (Owner.transform.position.y >= Owner._targetFlyHeight)
                {
                    ChangeState<ChargeState>();
                    return;
                }
                Owner._rigidbody.velocity =Owner.uppingSpeed * Vector3.up;
            }

            protected internal override void OnExit(Fsm<DashAttackModule> fsm, bool isShutdown = false)
            {
                Owner._rigidbody.velocity=Vector3.zero;
            }
        }

        private class ChargeState : BaseState
        {
            private float _curChargeTime;
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                _curChargeTime = 0f;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                _curChargeTime += fixeDeltaTime;
                if(_curChargeTime<Owner.chargingTime) return;
                ChangeState<DashState>();
            }
        }

        private class DashState : BaseState
        {
            private Vector3 _direction;
            private Barrier  _barrier;
            private AttackCollider _attackCollider;
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                _barrier = Owner.Owner.GetModule<Barrier>();
                _attackCollider = Owner.Owner.GetModule<AttackCollider>();
                
                _barrier.SetColliderEnable(false);
                _attackCollider.SetFindCallback(OnHitPlayer);
                _attackCollider.StartDetect();
                
                _direction=Owner._targetPos-Owner.transform.position;
                _direction.Normalize();
                Owner._rigidbody.velocity=Owner.dashSpeed*_direction;
            }

            protected internal override void OnExit(Fsm<DashAttackModule> fsm, bool isShutdown = false)
            {
                _barrier.SetColliderEnable(true);
                _attackCollider.StopDetect();
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                var onGround = Owner.GroundDetection(false);
                if (onGround)
                {
                    ChangeState<PrepareState>();
                    return;
                }
                Owner._rigidbody.velocity=Owner.dashSpeed*_direction;
            }

            private void OnHitPlayer(Player.Player player)
            {
                // todo gzp 修改为可配置
                var attackInfo = AttackInfo.Create(Owner.Owner, Owner.damage, _direction, 0);
                var result = player.OnAttacked(attackInfo);
                ReferencePool.Release(attackInfo);
                // Debug.Log(result);
                if (result.Hit)
                {
                    ChangeState<ReboundState>();
                }
            }
        }
        private class ReboundState : BaseState
        {
            private Vector3 _reboundDirection;
            protected internal override void OnEnter(Fsm<DashAttackModule> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbody.useGravity = true;
                _reboundDirection=Owner.transform.position-Owner._targetPos;
                _reboundDirection.Normalize();
                Owner._rigidbody.AddForce(Owner.reboundForce*_reboundDirection,ForceMode.VelocityChange);
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttackModule> fsm, float fixeDeltaTime)
            {
                var onGround = Owner.GroundDetection();
                if(!onGround) return;
                ChangeState<PrepareState>();
            }
        }
    }
}