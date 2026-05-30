using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Navigation;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Enemy.Executor
{
    /// <summary>
    /// Execute layer: jump-based locomotion for enemies (e.g., Blue Orbis).
    /// Caches the move direction then performs periodic impulse jumps with elevation angle.
    /// All velocity writes go through RigidbodyMover.
    /// </summary>
    public class EnemyJumpMoveExecutor : IMove, IEntityLogicUpdate
    {
        private readonly RigidbodyMover _mover;
        private readonly EnemyConfigSo _config;
        private readonly ICharacterGroundDetect _groundDetect;
        private readonly INavigator _navigator;

        private Vector3 _cachedDirection;
        private bool _hasDirection;
        private float _lastJumpTime = -100f;
        private float _currentJumpInterval;

        // Multi-frame launch for smooth takeoff (instead of instant impulse)
        private bool _isLaunching;
        private float _launchTimer;
        private Vector3 _launchTargetVelocity;
        private const float JumpLaunchTime = 0.12f;

        public Vector3 Velocity => _hasDirection ? _cachedDirection * _config.jumpForce : Vector3.zero;

        public EnemyJumpMoveExecutor(RigidbodyMover mover, EnemyConfigSo config,
            ICharacterGroundDetect groundDetect, INavigator navigator)
        {
            _mover = mover;
            _config = config;
            _groundDetect = groundDetect;
            _navigator = navigator;
        }

        public void Move(MoveCommand moveCommand)
        {
            var dir = moveCommand.Velocity;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.001f)
            {
                return;
            }

            _cachedDirection = dir.normalized;
            _hasDirection = true;
        }

        public void LogicUpdate(float deltaTime)
        {
            // Multi-frame launch acceleration for smooth takeoff
            if (_isLaunching)
            {
                _launchTimer += deltaTime;
                float t = Mathf.Clamp01(_launchTimer / JumpLaunchTime);
                // Ease-out: start fast, finish slower for spring-like feel
                float eased = 1f - (1f - t) * (1f - t);
                _mover.SetVelocity(Vector3.Lerp(Vector3.zero, _launchTargetVelocity, eased));

                if (t >= 1f)
                {
                    _isLaunching = false;
                    _mover.SetVelocity(_launchTargetVelocity);
                }
                return;
            }

            if (!_hasDirection)
                return;

            bool isGrounded = _groundDetect?.IsOnGround ?? false;

            if (!isGrounded)
                return;

            float cooldownRemaining = _currentJumpInterval - (Time.time - _lastJumpTime);
            if (cooldownRemaining > 0)
                return;

            Jump();
        }

        private void Jump()
        {
            _lastJumpTime = Time.time;
            _currentJumpInterval = _config.jumpInterval * (1f + Random.Range(-_config.jumpIntervalVariation, _config.jumpIntervalVariation));

            var dir = _cachedDirection;
            _hasDirection = false;

            // Random variation for organic feel
            float actualForce = _config.jumpForce * (1f + Random.Range(-_config.jumpForceVariation, _config.jumpForceVariation));
            float actualElevation = _config.jumpElevation + Random.Range(-_config.jumpElevationVariation, _config.jumpElevationVariation);

            float rad = Mathf.Deg2Rad * actualElevation;
            dir.y = Mathf.Sin(rad);
            dir.x *= Mathf.Cos(rad);
            dir.z *= Mathf.Cos(rad);

            _launchTargetVelocity = _navigator?.CalculateVelocity(dir * actualForce,
                _mover.transform.position) ?? dir * actualForce;

            // Face jump direction
            var flatDir = _cachedDirection; // original cached direction (horizontal only)
            if (flatDir.sqrMagnitude > 0.001f)
                _mover.SetRotation(Quaternion.LookRotation(flatDir));

            // Multi-frame launch — accelerate smoothly instead of instant impulse
            _isLaunching = true;
            _launchTimer = 0f;
        }
    }
}
