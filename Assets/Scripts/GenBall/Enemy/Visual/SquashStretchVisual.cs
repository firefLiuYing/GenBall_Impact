using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Executors;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.Enemy.Visual
{
    /// <summary>
    /// Velocity-driven squash & stretch visual deformation for sphere-based enemies.
    /// Operates on a child "Visual" Transform's localScale. Does NOT affect physics.
    /// Registered last in LogicUpdate order to read final velocity after all executors.
    /// </summary>
    public class SquashStretchVisual : IEntityLogicUpdate
    {
        private readonly Transform _visualTransform;
        private readonly RigidbodyMover _mover;
        private readonly ICharacterGroundDetect _groundDetect;
        private readonly EnemyConfigSo _config;
        private readonly BattleEntity _entity;

        private Vector3 _targetScale = Vector3.one;
        private Vector3 _scaleVelocity;
        private Vector3 _previousVelocity;
        private bool _wasGrounded;

        public SquashStretchVisual(Transform visualTransform, RigidbodyMover mover,
            ICharacterGroundDetect groundDetect, EnemyConfigSo config, BattleEntity entity)
        {
            _visualTransform = visualTransform;
            _mover = mover;
            _groundDetect = groundDetect;
            _config = config;
            _entity = entity;
            _targetScale = Vector3.one;
            _wasGrounded = true;
        }

        public void LogicUpdate(float deltaTime)
        {
            var velocity = _mover.Velocity;
            bool isGrounded = _groundDetect?.IsOnGround ?? false;
            float speed = velocity.magnitude;

            // Detect events
            bool justTookOff = _wasGrounded && !isGrounded && velocity.y > 1f;
            bool justLanded = !_wasGrounded && isGrounded;
            bool largeVelocityDelta = (velocity - _previousVelocity).magnitude > 8f;

            // Check dash executor phase
            var dashExecutor = _entity?.Get<EnemyDashExecutor>();
            bool isPreparing = dashExecutor?.CurrentPhase == EnemyDashExecutor.Phase.Prepare;

            if (isPreparing)
            {
                // Squash down during anticipation
                _targetScale = new Vector3(1f + (1f - _config.squashRatio) * 0.5f, _config.squashRatio, 1f + (1f - _config.squashRatio) * 0.5f);
            }
            else if (justTookOff || largeVelocityDelta)
            {
                // Stretch in movement direction
                if (Mathf.Abs(velocity.y) > Mathf.Abs(velocity.x) && Mathf.Abs(velocity.y) > Mathf.Abs(velocity.z))
                {
                    // Vertical movement: stretch Y, squash XZ
                    _targetScale = new Vector3(1f / Mathf.Sqrt(_config.stretchRatio), _config.stretchRatio, 1f / Mathf.Sqrt(_config.stretchRatio));
                }
                else if (speed > 0.5f)
                {
                    // Horizontal movement: stretch in direction
                    var dir = velocity.normalized;
                    float absX = Mathf.Abs(dir.x);
                    float absZ = Mathf.Abs(dir.z);
                    _targetScale = new Vector3(
                        Mathf.Lerp(1f, _config.stretchRatio, absX) * Mathf.Lerp(1f, 1f / Mathf.Sqrt(_config.stretchRatio), absZ),
                        Mathf.Lerp(1f, 1f / Mathf.Sqrt(_config.stretchRatio), absX + absZ),
                        Mathf.Lerp(1f, _config.stretchRatio, absZ) * Mathf.Lerp(1f, 1f / Mathf.Sqrt(_config.stretchRatio), absX)
                    );
                }
            }
            else if (justLanded)
            {
                // Squash on landing
                _targetScale = new Vector3(1f / Mathf.Sqrt(_config.squashRatio), _config.squashRatio, 1f / Mathf.Sqrt(_config.squashRatio));
            }
            else if (isGrounded && speed < 0.5f)
            {
                // Recover to normal when idle on ground
                _targetScale = Vector3.one;
            }

            // Smooth damp toward target
            _visualTransform.localScale = Vector3.SmoothDamp(
                _visualTransform.localScale, _targetScale,
                ref _scaleVelocity, 1f / _config.squashStretchRecoverySpeed);

            _previousVelocity = velocity;
            _wasGrounded = isGrounded;
        }
    }
}
