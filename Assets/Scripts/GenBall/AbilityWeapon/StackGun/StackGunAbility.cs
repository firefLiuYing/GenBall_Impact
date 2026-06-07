using System.Collections.Generic;
using GenBall.BattleSystem.Framework;
using GenBall.Player;
using UnityEngine;

namespace GenBall.AbilityWeapon.StackGun
{
    public class StackGunAbility : IAbilityWeapon
    {
        private readonly Stack<OrbCaptureData> _orbStack = new();
        private readonly StackGunConfig _config = new();
        private BattleEntity _playerEntity;
        private bool _isShooting;

        public bool IsExhausted { get; private set; }
        public IAbilityWeaponConfig Config => _config;

        public void Activate(BattleEntity player)
        {
            _playerEntity = player;
            _orbStack.Clear();
            _isShooting = false;
            IsExhausted = false;
        }

        public void Deactivate()
        {
            _orbStack.Clear();
            _playerEntity = null;
        }

        public void HandlePrimary(ButtonState state)
        {
            if (state != ButtonState.Down) return;

            _isShooting = true; // locks out secondary absorption

            if (_orbStack.Count > 0)
            {
                FireOrb();
                IsExhausted = _orbStack.Count == 0;
            }
            else
            {
                IsExhausted = true;
            }
        }

        public void HandleSecondary(ButtonState state)
        {
            if (state != ButtonState.Down) return;
            if (_isShooting) return;
            if (_orbStack.Count >= _config.MaxCapacity) return;

            TryAbsorbOrb();
        }

        public void LogicUpdate(float deltaTime)
        {
        }

        private void TryAbsorbOrb()
        {
            if (_playerEntity == null) return;

            var playerPos = _playerEntity.transform.position;
            var playerForward = _playerEntity.transform.forward;

            var hits = Physics.OverlapSphere(playerPos, _config.AbsorbRadius);
            var halfAngleCos = Mathf.Cos(_config.AbsorbAngle * 0.5f * Mathf.Deg2Rad);
            var closestDist = float.MaxValue;
            GameObject closestOrb = null;

            foreach (var hit in hits)
            {
                // Skip self
                if (hit.gameObject == _playerEntity.gameObject) continue;

                // Must have BattleEntity (enemy/Orbis)
                var entity = hit.GetComponentInParent<BattleEntity>();
                if (entity == null) continue;

                // Skip already-absorbed (inactive) orbs
                if (!hit.gameObject.activeInHierarchy) continue;

                // Cone angle check
                var toTarget = (hit.transform.position - playerPos).normalized;
                if (Vector3.Dot(playerForward, toTarget) < halfAngleCos) continue;

                var dist = Vector3.Distance(playerPos, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestOrb = hit.gameObject;
                }
            }

            if (closestOrb != null)
            {
                AbsorbOrb(closestOrb);
            }
        }

        private void AbsorbOrb(GameObject orb)
        {
            var stats = orb.GetComponent<BattleEntity>()?.Get<StatComponent>();
            var remainingHp = stats != null ? (int)stats.GetValue("CurrentHealth") : 0;

            orb.SetActive(false);

            _orbStack.Push(new OrbCaptureData
            {
                OrbGameObject = orb,
                RemainingHp = remainingHp,
            });

            Debug.Log($"[StackGun] Absorbed orb (HP={remainingHp}), stack size={_orbStack.Count}");
        }

        private void FireOrb()
        {
            if (_playerEntity == null) return;

            var data = _orbStack.Pop();
            var orb = data.OrbGameObject;

            // Position at muzzle
            var muzzlePos = _playerEntity.transform.position
                + _playerEntity.transform.forward * 1.5f
                + Vector3.up * 0.5f;
            orb.transform.position = muzzlePos;
            orb.transform.rotation = Quaternion.LookRotation(_playerEntity.transform.forward);

            orb.SetActive(true);

            // Add projectile behaviour
            var projectile = orb.GetComponent<OrbProjectile>();
            if (projectile == null)
                projectile = orb.AddComponent<OrbProjectile>();

            var aimDir = _playerEntity.transform.forward;
            projectile.Initialize(
                velocity: aimDir * _config.ProjectileSpeed,
                armTime: _config.ArmTime,
                lifetime: _config.ProjectileLifetime,
                damage: _config.BaseDamage,
                knockbackForce: _config.KnockbackForce,
                owner: _playerEntity.gameObject);

            Debug.Log($"[StackGun] Fired orb (HP={data.RemainingHp}), stack remaining={_orbStack.Count}");
        }
    }
}
