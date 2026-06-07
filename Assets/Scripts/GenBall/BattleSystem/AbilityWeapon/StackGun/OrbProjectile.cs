using GenBall.BattleSystem;
using GenBall.BattleSystem.Framework;
using UnityEngine;
using Yueyn.Base.ReferencePool;

namespace GenBall.BattleSystem.AbilityWeapon.StackGun
{
    /// <summary>
    /// Attached to an Orbis GameObject when fired from the StackGun.
    /// Overrides AI movement via FixedUpdate velocity, flies in a straight line,
    /// deals damage + knockback on hit. On lifetime expiry, reverts to normal enemy.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class OrbProjectile : MonoBehaviour
    {
        private Vector3 _velocity;
        private float _armTimeRemaining;
        private float _lifetimeRemaining;
        private int _damage;
        private float _knockbackForce;
        private GameObject _owner;
        private Rigidbody _rb;
        private bool _armed;

        public void Initialize(Vector3 velocity, float armTime, float lifetime,
            int damage, float knockbackForce, GameObject owner)
        {
            _velocity = velocity;
            _armTimeRemaining = armTime;
            _lifetimeRemaining = lifetime;
            _damage = damage;
            _knockbackForce = knockbackForce;
            _owner = owner;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb != null)
            {
                _rb.isKinematic = false;
            }
        }

        private void FixedUpdate()
        {
            if (_rb != null)
            {
                _rb.velocity = _velocity;
            }
        }

        private void Update()
        {
            if (!_armed)
            {
                _armTimeRemaining -= Time.deltaTime;
                if (_armTimeRemaining <= 0f)
                    _armed = true;
            }

            _lifetimeRemaining -= Time.deltaTime;
            if (_lifetimeRemaining <= 0f)
            {
                RevertToEnemy();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_armed) return;

            var hitEntity = collision.gameObject.GetComponentInParent<BattleEntity>();
            if (hitEntity == null) return;

            // Don't damage the owner
            if (_owner != null && collision.gameObject == _owner) return;

            // Don't damage other Orbis projectiles
            if (collision.gameObject.GetComponent<OrbProjectile>() != null) return;

            DealDamage(hitEntity, collision);
            DestroyOrb();
        }

        private void DealDamage(BattleEntity hitEntity, Collision collision)
        {
            var damageReceiver = hitEntity.Get<DamageReceiverComponent>();
            if (damageReceiver == null) return;

            var knockDir = (collision.transform.position - transform.position).normalized;

            var info = DamageInfo.Create(
                defender: hitEntity.gameObject,
                damage: _damage,
                tags: null,
                direction: knockDir,
                impactForce: (int)_knockbackForce,
                attacker: _owner ?? gameObject);
            damageReceiver.TakeDamage(info);
            ReferencePool.Release(info);

            // Apply knockback force
            var hitRb = collision.gameObject.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddForce(knockDir * _knockbackForce, ForceMode.Impulse);
            }
        }

        private void RevertToEnemy()
        {
            // Remove projectile — AI resumes normal control
            Destroy(this);
        }

        private void DestroyOrb()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // Reset velocity when projectile is removed (revert case)
            if (_rb != null)
                _rb.velocity = Vector3.zero;
        }
    }
}
