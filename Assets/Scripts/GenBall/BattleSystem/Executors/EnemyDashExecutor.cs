using GenBall.Enemy;
using System.Collections.Generic;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Enemy.Attack;
using GenBall.Enemy.Detect;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Executors
{
    /// <summary>
    /// Execute layer: dash attack FSM for enemies.
    /// States: Idle -> Prepare -> Fly -> Charge -> Dash -> Rebound -> Idle.
    /// Uses AttackTrigger for hit detection and Barrier for defense toggling.
    /// </summary>
    public class EnemyDashExecutor : IAttack, IEntityLogicUpdate
    {
        private enum State
        {
            Idle,
            Prepare,
            Fly,
            Charge,
            Dash,
            Rebound
        }

        public enum Phase { Idle, Prepare, Fly, Charge, Dash, Rebound }
        public Phase CurrentPhase
        {
            get
            {
                return _state switch
                {
                    State.Idle => Phase.Idle,
                    State.Prepare => Phase.Prepare,
                    State.Fly => Phase.Fly,
                    State.Charge => Phase.Charge,
                    State.Dash => Phase.Dash,
                    State.Rebound => Phase.Rebound,
                    _ => Phase.Idle
                };
            }
        }

        private readonly RigidbodyMover _mover;
        private readonly BattleEntity _entity;
        private readonly EnemyConfigSo _config;
        private readonly EnemyDetector _detector;
        private readonly AttackTrigger _attackTrigger;
        private readonly Barrier _barrier;

        private State _state = State.Idle;

        // Prepare state
        private float _prepareTimer;
        private Vector3 _targetPos;
        private float _targetFlyHeight;

        // Fly state
        private float _flyElapsed;

        // Charge state
        private float _chargeTimer;

        // Dash state
        private Vector3 _dashDirection;
        private Vector3 _dashStartPos;
        private const float MaxDashDistance = 8f;

        // Hardcoded constants (not yet in config)
        private const float UppingSpeed = 5f;
        private const float FlyMaxTime = 0.5f;

        // Ground detection
        private SphereCollider _cachedCollider;

        public bool IsAttacking => _state != State.Idle;

        private SphereCollider Collider
        {
            get
            {
                if (_cachedCollider == null)
                    _cachedCollider = _entity.GetComponentInChildren<SphereCollider>();
                return _cachedCollider;
            }
        }

        public EnemyDashExecutor(RigidbodyMover mover, BattleEntity entity,
            EnemyConfigSo config, EnemyDetector detector,
            AttackTrigger attackTrigger, Barrier barrier)
        {
            _mover = mover;
            _entity = entity;
            _config = config;
            _detector = detector;
            _attackTrigger = attackTrigger;
            _barrier = barrier;
        }

        public void Attack(AttackCommand command)
        {
            if (_state != State.Idle)
                return;

            TransitionTo(State.Prepare);
        }

        public void Cancel()
        {
            // Cleanup dash state if active
            if (_state == State.Dash)
            {
                if (_attackTrigger != null) _attackTrigger.OnHit -= OnHitTarget;
                _attackTrigger?.StopDetect();
                _barrier?.SetColliderEnable(true);
            }

            TransitionTo(State.Idle);
        }

        public void LogicUpdate(float deltaTime)
        {
            switch (_state)
            {
                case State.Idle:
                    break;

                case State.Prepare:
                    UpdatePrepare(deltaTime);
                    break;

                case State.Fly:
                    UpdateFly(deltaTime);
                    break;

                case State.Charge:
                    UpdateCharge(deltaTime);
                    break;

                case State.Dash:
                    UpdateDash(deltaTime);
                    break;

                case State.Rebound:
                    UpdateRebound(deltaTime);
                    break;
            }
        }

        // ================================================================
        // PREPARE STATE
        // ================================================================

        private void TransitionTo(State newState)
        {
            // Exit current state
            switch (_state)
            {
                case State.Fly:
                    _mover.SetVelocity(Vector3.zero);
                    break;

                case State.Dash:
                    if (_attackTrigger != null) _attackTrigger.OnHit -= OnHitTarget;
                    _attackTrigger?.StopDetect();
                    _barrier?.SetColliderEnable(true);
                    _mover.SetVelocity(Vector3.zero);
                    _mover.UseGravity = true;
                    break;

                case State.Rebound:
                    _mover.SetVelocity(Vector3.zero);
                    break;
            }

            _state = newState;

            // Enter new state
            switch (newState)
            {
                case State.Idle:
                    break;

                case State.Prepare:
                    EnterPrepare();
                    break;

                case State.Fly:
                    EnterFly();
                    break;

                case State.Charge:
                    EnterCharge();
                    break;

                case State.Dash:
                    EnterDash();
                    break;

                case State.Rebound:
                    EnterRebound();
                    break;
            }
        }

        // ================================================================
        // STATE ENTERS
        // ================================================================

        private void EnterPrepare()
        {
            _mover.UseGravity = true;
            _prepareTimer = 0f;
        }

        private void EnterFly()
        {
            _mover.UseGravity = false;
            _flyElapsed = 0f;
            _mover.SetVelocity(Vector3.zero);
        }

        private void EnterCharge()
        {
            _chargeTimer = 0f;
        }

        private void EnterDash()
        {
            _barrier?.SetColliderEnable(false);
            _mover.UseGravity = false; // Disable gravity during dash

            // Recompute target position at dash moment (player may have moved during Fly+Charge)
            if (_detector.HasTarget)
            {
                var currentTargetPos = _detector.CurrentTarget.transform.position;
                _targetPos = currentTargetPos + _config.dashHeightOffset * Vector3.up;
            }

            _dashStartPos = _entity.transform.position;
            _dashDirection = _targetPos - _dashStartPos;
            _dashDirection.Normalize();
            // Face the dash direction
            if (_dashDirection.sqrMagnitude > 0.001f)
                _mover.SetRotation(Quaternion.LookRotation(_dashDirection));
            _mover.SetVelocity(_config.dashSpeed * _dashDirection);
        }

        private void EnterRebound()
        {
            _mover.UseGravity = true;
            var horizontalDir = (_entity.transform.position - _targetPos).normalized;
            // Face away from target (rebound direction)
            if (horizontalDir.sqrMagnitude > 0.001f)
                _mover.SetRotation(Quaternion.LookRotation(horizontalDir));
            var reboundDir = (horizontalDir + Vector3.up * _config.reboundUpwardRatio).normalized;
            _mover.SetVelocity(reboundDir * _config.reboundForce);
        }

        // ================================================================
        // STATE UPDATES
        // ================================================================

        private void UpdatePrepare(float deltaTime)
        {
            if (!_detector.HasTarget)
            {
                TransitionTo(State.Idle);
                return;
            }

            if (!GroundDetection())
            {
                _prepareTimer = 0f;
                return;
            }

            _prepareTimer += deltaTime;
            if (_prepareTimer < _config.preparationTime)
                return;

            var targetPos = _detector.CurrentTarget.transform.position;
            _targetPos = targetPos + _config.dashHeightOffset * Vector3.up;

            // Always fly up before dashing — fast and consistent
            _targetFlyHeight = targetPos.y + _config.flyHeightOffset;
            TransitionTo(State.Fly);
        }

        private void UpdateFly(float deltaTime)
        {
            _flyElapsed += deltaTime;

            if (_flyElapsed >= FlyMaxTime || _entity.transform.position.y >= _targetFlyHeight)
            {
                TransitionTo(State.Charge);
                return;
            }

            float speed = Mathf.Lerp(0f, UppingSpeed, Mathf.Clamp01(_flyElapsed / _config.flyAccelTime));
            _mover.SetVelocity(speed * Vector3.up);
        }

        private void UpdateCharge(float deltaTime)
        {
            _chargeTimer += deltaTime;
            if (_chargeTimer < _config.chargingTime)
                return;

            TransitionTo(State.Dash);
        }

        private void UpdateDash(float deltaTime)
        {
            var col = Collider;
            float radius = col?.radius ?? 0.5f;

            // Manual player hit detection — use tag since Player collider may not be on "Player" layer
            var origin = _entity.transform.position + (col?.center ?? Vector3.zero);
            var detectRadius = Mathf.Max(radius * 1.5f, 1f);
            // Exclude enemy self-layers only, check everything else
            int excludeMask = (1 << LayerMask.NameToLayer("Orbis"))
                            | (1 << LayerMask.NameToLayer("OrbisAttack"))
                            | (1 << LayerMask.NameToLayer("Barrier"));
            // Check current overlap AND sweep along dash direction to prevent tunneling
            var hits = Physics.OverlapSphere(origin, detectRadius, ~excludeMask);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    OnHitTarget(hit.gameObject);
                    return;
                }
            }

            // SphereCast: sweep ahead by this frame's travel distance
            var travelDist = _config.dashSpeed * deltaTime;
            if (Physics.SphereCast(origin, detectRadius, _dashDirection, out var sweepHit, travelDist, ~excludeMask))
            {
                if (sweepHit.collider.CompareTag("Player"))
                {
                    OnHitTarget(sweepHit.collider.gameObject);
                    return;
                }
            }

            // Wall collision detection
            if (col != null)
            {
                int exclude = (1 << LayerMask.NameToLayer("Orbis"))
                            | (1 << LayerMask.NameToLayer("OrbisAttack"))
                            | (1 << LayerMask.NameToLayer("Barrier"))
                            | (1 << LayerMask.NameToLayer("Player"));
                if (Physics.SphereCast(origin, radius, _dashDirection, out var hit, radius + 0.3f, ~exclude))
                {
                    var bounceDir = Vector3.Reflect(_dashDirection, hit.normal);
                    _mover.SetVelocity(bounceDir * _config.dashSpeed * _config.wallBounceMultiplier);
                    _mover.UseGravity = true;
                    TransitionTo(State.Idle);
                    return;
                }
            }

            // Max distance limit — prevent flying across the entire map
            if (Vector3.Distance(_entity.transform.position, _dashStartPos) > MaxDashDistance)
            {
                TransitionTo(State.Idle);
                return;
            }

            if (GroundDetection(detectPlayer: false))
            {
                TransitionTo(State.Idle);
                return;
            }

            _mover.SetVelocity(_config.dashSpeed * _dashDirection);
        }

        private void UpdateRebound(float deltaTime)
        {
            if (!GroundDetection())
                return;

            TransitionTo(State.Idle);
        }

        // ================================================================
        // HIT DETECTION
        // ================================================================

        private void OnHitTarget(GameObject target)
        {
            var damageInfo = DamageInfo.Create(
                target,
                _config.attackDamage,
                new List<string> { "dash" },
                _dashDirection,
                0,
                _entity.gameObject);

            SystemRepository.Instance.GetSystem<IDamageSystem>()?.ApplyDamage(damageInfo);
            Debug.Log($"[Dash] Damage applied to Player: {_config.attackDamage} dmg, target={target.name}");

            TransitionTo(State.Rebound);
        }

        // ================================================================
        // GROUND DETECTION
        // ================================================================

        private bool GroundDetection(bool detectPlayer = true)
        {
            var col = Collider;
            if (col == null)
                return false;

            int exclude = (1 << LayerMask.NameToLayer("Orbis"))
                        | (1 << LayerMask.NameToLayer("OrbisAttack"))
                        | (1 << LayerMask.NameToLayer("Barrier"));
            if (!detectPlayer)
                exclude |= 1 << LayerMask.NameToLayer("Player");

            LayerMask mask = ~exclude;
            var origin = _entity.transform.position + col.center;
            return Physics.Raycast(origin, Vector3.down, col.radius + 0.01f, mask);
        }
    }
}
