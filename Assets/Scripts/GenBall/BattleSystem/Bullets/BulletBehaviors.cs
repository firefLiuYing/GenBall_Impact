using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    // ============================================================
    // Hit Result
    // ============================================================

    /// <summary>
    /// Result of a hit detection check.
    /// </summary>
    public struct HitResult
    {
        public GameObject Target;
        public Vector3 Point;
        public Vector3 Normal;
        public int TargetId;
    }

    // ============================================================
    // Detection Strategy
    // ============================================================

    /// <summary>
    /// Strategy for detecting hits along the bullet's path.
    /// </summary>
    public interface IDetectionStrategy
    {
        /// <summary>
        /// Detect hits along the bullet's trajectory.
        /// </summary>
        /// <param name="position">Current bullet position.</param>
        /// <param name="direction">Normalized movement direction.</param>
        /// <param name="radius">Bullet collision radius.</param>
        /// <param name="speed">Bullet speed.</param>
        /// <param name="deltaTime">Time step.</param>
        /// <param name="targetMask">Layer mask for valid targets.</param>
        /// <param name="hitTargetIds">Set of already-hit target instance IDs to filter out.</param>
        /// <returns>Hit result if something was hit, null otherwise.</returns>
        HitResult? Detect(Vector3 position, Vector3 direction, float radius, float speed,
            float deltaTime, LayerMask targetMask, HashSet<int> hitTargetIds);
    }

    /// <summary>
    /// Single raycast along velocity direction. Fast, for most bullets.
    /// </summary>
    public class RayDetection : IDetectionStrategy
    {
        public HitResult? Detect(Vector3 position, Vector3 direction, float radius, float speed,
            float deltaTime, LayerMask targetMask, HashSet<int> hitTargetIds)
        {
            float distance = speed * deltaTime;
            if (Physics.Raycast(position, direction, out var hit, distance, targetMask))
            {
                int id = hit.collider.gameObject.GetInstanceID();
                if (hitTargetIds.Contains(id))
                    return null;

                return new HitResult
                {
                    Target = hit.collider.gameObject,
                    Point = hit.point,
                    Normal = hit.normal,
                    TargetId = id
                };
            }
            return null;
        }
    }

    /// <summary>
    /// Sphere cast along velocity. For larger projectiles.
    /// </summary>
    public class SphereCastDetection : IDetectionStrategy
    {
        public HitResult? Detect(Vector3 position, Vector3 direction, float radius, float speed,
            float deltaTime, LayerMask targetMask, HashSet<int> hitTargetIds)
        {
            float distance = speed * deltaTime;
            if (Physics.SphereCast(position, radius, direction, out var hit, distance, targetMask))
            {
                int id = hit.collider.gameObject.GetInstanceID();
                if (hitTargetIds.Contains(id))
                    return null;

                return new HitResult
                {
                    Target = hit.collider.gameObject,
                    Point = hit.point,
                    Normal = hit.normal,
                    TargetId = id
                };
            }
            return null;
        }
    }

    // ============================================================
    // Hit Behavior
    // ============================================================

    /// <summary>
    /// Behavior executed when a bullet hits a target.
    /// Returns true if the bullet should continue flying, false to recycle.
    /// Behaviors are executed in order as a chain.
    /// </summary>
    public interface IHitBehavior
    {
        /// <returns>True if bullet should continue flying, false to stop and recycle.</returns>
        bool OnHit(BulletInstance bullet, HitResult hit);
    }

    /// <summary>
    /// Apply damage to the hit target and stop the bullet.
    /// </summary>
    public class DealDamageBehavior : IHitBehavior
    {
        public bool OnHit(BulletInstance bullet, HitResult hit)
        {
            ApplyDamage(bullet, hit);
            return false;
        }

        protected void ApplyDamage(BulletInstance bullet, HitResult hit)
        {
            var damageSystem = SystemRepository.Instance.GetSystem<IDamageSystem>();
            if (damageSystem == null) return;

            var damageInfo = DamageInfo.Create(
                hit.Target,
                bullet.FinalDamage,
                new List<string> { "Bullet" },
                bullet.Direction,
                0,
                bullet.Source);

            damageSystem.ApplyDamage(damageInfo);
        }
    }

    /// <summary>
    /// Apply damage and continue flying. Bullet stops after max penetrations.
    /// </summary>
    public class PenetrateBehavior : IHitBehavior
    {
        private readonly int _maxPenetrations;

        public PenetrateBehavior(int maxPenetrations)
        {
            _maxPenetrations = maxPenetrations;
        }

        public bool OnHit(BulletInstance bullet, HitResult hit)
        {
            // Apply damage via DealDamageBehavior's logic
            var damageSystem = SystemRepository.Instance.GetSystem<IDamageSystem>();
            if (damageSystem != null)
            {
                var damageInfo = DamageInfo.Create(
                    hit.Target,
                    bullet.FinalDamage,
                    new List<string> { "Bullet" },
                    bullet.Direction,
                    0,
                    bullet.Source);
                damageSystem.ApplyDamage(damageInfo);
            }

            bullet.CurrentPenetrationCount++;
            return bullet.CurrentPenetrationCount < _maxPenetrations;
        }
    }

    /// <summary>
    /// Apply damage, reflect velocity off the hit normal, and continue.
    /// Bullet stops after max bounces.
    /// </summary>
    public class BounceBehavior : IHitBehavior
    {
        private readonly int _maxBounces;
        private readonly float _damping;

        public BounceBehavior(int maxBounces, float damping = 0.7f)
        {
            _maxBounces = maxBounces;
            _damping = damping;
        }

        public bool OnHit(BulletInstance bullet, HitResult hit)
        {
            // Apply damage
            var damageSystem = SystemRepository.Instance.GetSystem<IDamageSystem>();
            if (damageSystem != null)
            {
                var damageInfo = DamageInfo.Create(
                    hit.Target,
                    bullet.FinalDamage,
                    new List<string> { "Bullet" },
                    bullet.Direction,
                    0,
                    bullet.Source);
                damageSystem.ApplyDamage(damageInfo);
            }

            // Reflect velocity
            bullet.Velocity = Vector3.Reflect(bullet.Velocity, hit.Normal) * _damping;
            bullet.Direction = bullet.Velocity.normalized;

            bullet.CurrentBounceCount++;
            return bullet.CurrentBounceCount < _maxBounces;
        }
    }

    /// <summary>
    /// Apply AOE damage around the hit point, then stop the bullet.
    /// </summary>
    public class AOEDamageBehavior : IHitBehavior
    {
        private readonly float _aoeRadius;
        private readonly int _aoeDamage;
        private readonly LayerMask _targetMask;

        public AOEDamageBehavior(float aoeRadius, int aoeDamage, LayerMask targetMask)
        {
            _aoeRadius = aoeRadius;
            _aoeDamage = aoeDamage;
            _targetMask = targetMask;
        }

        public bool OnHit(BulletInstance bullet, HitResult hit)
        {
            // First apply direct damage
            var damageSystem = SystemRepository.Instance.GetSystem<IDamageSystem>();
            if (damageSystem != null)
            {
                var damageInfo = DamageInfo.Create(
                    hit.Target,
                    bullet.FinalDamage,
                    new List<string> { "Bullet" },
                    bullet.Direction,
                    0,
                    bullet.Source);
                damageSystem.ApplyDamage(damageInfo);

                // AOE damage to nearby targets
                var colliders = Physics.OverlapSphere(hit.Point, _aoeRadius, _targetMask);
                foreach (var col in colliders)
                {
                    if (col.gameObject == hit.Target) continue;
                    if (col.gameObject.GetInstanceID() == bullet.Source?.GetInstanceID()) continue;

                    var aoeInfo = DamageInfo.Create(
                        col.gameObject,
                        _aoeDamage,
                        new List<string> { "Bullet", "AOE" },
                        (col.transform.position - hit.Point).normalized,
                        0,
                        bullet.Source);
                    damageSystem.ApplyDamage(aoeInfo);
                }
            }

            return false;
        }
    }

    // ============================================================
    // Movement Modifier
    // ============================================================

    /// <summary>
    /// Modifies the bullet's movement each logic tick.
    /// Applied before position update.
    /// </summary>
    public interface IMovementModifier
    {
        void Apply(BulletInstance bullet, float deltaTime);
    }

    /// <summary>
    /// Apply downward gravity acceleration to the bullet.
    /// </summary>
    public class GravityModifier : IMovementModifier
    {
        private readonly float _gravity;

        public GravityModifier(float gravity)
        {
            _gravity = gravity;
        }

        public void Apply(BulletInstance bullet, float deltaTime)
        {
            bullet.Velocity += Vector3.down * _gravity * deltaTime;
            bullet.Direction = bullet.Velocity.normalized;
        }
    }
}
