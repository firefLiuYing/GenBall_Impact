using System.Collections.Generic;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Mover;
using GenBall.Enemy.Controller;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.Attack
{
    public class DashAttack : MonoBehaviour, IEnemyAttack
    {
        [SerializeField] private int attackId;
        [SerializeField] private int damage;
        [SerializeField] private float preparationTime;
        [SerializeField] private float uppingSpeed;
        [SerializeField] private float flyHeightOffset;
        [SerializeField] private float dashHeightOffset;
        [SerializeField] private float dashSpeed;
        [SerializeField] private float chargingTime;
        [SerializeField] private float reboundForce;

        private CharacterState _owner;
        private RigidbodyMover _rigidbodyMover;
        private SphereCollider _collider;
        private AttackTrigger _attackTrigger;
        private Barrier _barrier;
        private EnemyDetectController _detect;

        private Fsm<DashAttack> _fsm;
        private bool _shouldAttack;
        private float _targetFlyHeight;
        private Vector3 _targetPos;

        public int AttackId => attackId;
        public bool CanExecute => !IsExecuting;
        public bool IsExecuting { get; private set; }

        public void Init(CharacterState owner)
        {
            _owner = owner;
            _rigidbodyMover = owner.GetComponent<RigidbodyMover>();
            _collider = owner.GetComponentInChildren<SphereCollider>();
            _barrier = owner.GetComponentInChildren<Barrier>();
            _attackTrigger = owner.GetComponentInChildren<AttackTrigger>();
            _detect = owner.GetComponentInChildren<EnemyDetectController>();

            _attackTrigger.Init(owner.gameObject);

            var states = new List<FsmState<DashAttack>>
            {
                new IdleState(),
                new PrepareState(),
                new FlyState(),
                new ChargeState(),
                new DashState(),
                new ReboundState()
            };
            _fsm = GameEntry.Fsm.CreateFsm($"DashAttack_{GetHashCode()}", this, states);
            _fsm.Start<IdleState>();
        }

        public void Execute()
        {
            _shouldAttack = true;
        }

        public void Cancel()
        {
            _shouldAttack = false;
        }

        public void Tick(float deltaTime)
        {
            _fsm.FixedUpdate(deltaTime);
        }

        private void OnDestroy()
        {
            if (_fsm != null && !_fsm.IsDestroyed)
                GameEntry.Fsm.DestroyFsm(_fsm);
        }

        private bool GroundDetection(bool detectPlayer = true)
        {
            int exclude = (1 << LayerMask.NameToLayer("Orbis"))
                        | (1 << LayerMask.NameToLayer("OrbisAttack"))
                        | (1 << LayerMask.NameToLayer("Barrier"));
            if (!detectPlayer) exclude |= 1 << LayerMask.NameToLayer("Player");
            LayerMask mask = ~exclude;
            var origin = transform.position + _collider.center;
            return Physics.Raycast(origin, Vector3.down, _collider.radius + 0.01f, mask);
        }

        private abstract class BaseState : FsmState<DashAttack>
        {
            private Fsm<DashAttack> _fsm;
            protected DashAttack Owner => _fsm.Owner;

            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                _fsm = fsm;
            }

            protected void ChangeState<TState>() where TState : BaseState
                => _fsm.ChangeState<TState>();
        }

        private class IdleState : BaseState
        {
            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);
                Owner.IsExecuting = false;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                if (!Owner._shouldAttack) return;
                Owner._shouldAttack = false;
                Owner.IsExecuting = true;
                ChangeState<PrepareState>();
            }
        }

        private class PrepareState : BaseState
        {
            private float _curPrepareTime;

            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbodyMover.UseGravity = true;
                _curPrepareTime = 0f;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                if (!Owner._detect.HasTarget)
                {
                    ChangeState<IdleState>();
                    return;
                }

                if (!Owner.GroundDetection())
                {
                    _curPrepareTime = 0f;
                    return;
                }

                _curPrepareTime += fixedDeltaTime;
                if (_curPrepareTime < Owner.preparationTime) return;

                var targetPos = Owner._detect.CurrentTarget.transform.position;
                Owner._targetPos = targetPos + Owner.dashHeightOffset * Vector3.up;
                Owner._targetFlyHeight = targetPos.y + Owner.flyHeightOffset;
                ChangeState<FlyState>();
            }
        }

        private class FlyState : BaseState
        {
            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbodyMover.UseGravity = false;
                Owner._rigidbodyMover.SetVelocity(Owner.uppingSpeed * Vector3.up);
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                if (Owner.transform.position.y >= Owner._targetFlyHeight)
                {
                    ChangeState<ChargeState>();
                    return;
                }
                Owner._rigidbodyMover.SetVelocity(Owner.uppingSpeed * Vector3.up);
            }

            protected internal override void OnExit(Fsm<DashAttack> fsm, bool isShutdown = false)
            {
                Owner._rigidbodyMover.SetVelocity(Vector3.zero);
            }
        }

        private class ChargeState : BaseState
        {
            private float _curChargeTime;

            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);
                _curChargeTime = 0f;
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                _curChargeTime += fixedDeltaTime;
                if (_curChargeTime < Owner.chargingTime) return;
                ChangeState<DashState>();
            }
        }

        private class DashState : BaseState
        {
            private Vector3 _direction;

            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);

                Owner._barrier.SetColliderEnable(false);
                Owner._attackTrigger.OnHit += OnHitTarget;
                Owner._attackTrigger.StartDetect();

                _direction = Owner._targetPos - Owner.transform.position;
                _direction.Normalize();
                Owner._rigidbodyMover.SetVelocity(Owner.dashSpeed * _direction);
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                if (Owner.GroundDetection(false))
                {
                    ChangeState<PrepareState>();
                    return;
                }
                Owner._rigidbodyMover.SetVelocity(Owner.dashSpeed * _direction);
            }

            protected internal override void OnExit(Fsm<DashAttack> fsm, bool isShutdown = false)
            {
                Owner._attackTrigger.OnHit -= OnHitTarget;
                Owner._attackTrigger.StopDetect();
                Owner._barrier.SetColliderEnable(true);
            }

            private void OnHitTarget(GameObject target)
            {
                var damageInfo = DamageInfo.Create(
                    target, Owner.damage,
                    new List<string> { "dash" },
                    _direction, 0, Owner._owner.gameObject);
                DamageSystem.Instance.ApplyDamage(damageInfo);
                ChangeState<ReboundState>();
            }
        }

        private class ReboundState : BaseState
        {
            protected internal override void OnEnter(Fsm<DashAttack> fsm)
            {
                base.OnEnter(fsm);
                Owner._rigidbodyMover.UseGravity = true;
                var reboundDirection = Owner.transform.position - Owner._targetPos;
                reboundDirection.Normalize();
                Owner._rigidbodyMover.AddForce(Owner.reboundForce * reboundDirection, ForceMode.VelocityChange);
            }

            protected internal override void OnFixedUpdate(Fsm<DashAttack> fsm, float fixedDeltaTime)
            {
                if (!Owner.GroundDetection()) return;
                ChangeState<PrepareState>();
            }
        }
    }
}
