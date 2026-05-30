using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Weapons.Components.Trigger;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Components
{
    /// <summary>
    /// Weapon-internal Executor layer.
    /// Handles bullet spawning with spread calculation.
    /// Connects to IBulletSystem.FireBullet() with runtime-computed parameters.
    /// </summary>
    public class WeaponFireExecutor
    {
        private readonly BattleEntity _weapon;
        private readonly Transform _spawnPoint;
        private readonly BulletId _bulletConfigId;

        private const string StatDamage = "Damage";
        private const string StatBulletSpeed = "BulletSpeed";
        private const string StatBulletRadius = "BulletRadius";
        private const string StatExtraPenetrations = "ExtraPenetrations";
        private const string StatExtraBounces = "ExtraBounces";
        private const string StatSpeedMultiplier = "BulletSpeedMultiplier";

        public WeaponFireExecutor(BattleEntity weapon, Transform spawnPoint, BulletId bulletConfigId)
        {
            _weapon = weapon;
            _spawnPoint = spawnPoint;
            _bulletConfigId = bulletConfigId;
        }

        /// <summary>
        /// Execute a fire request: calculate spread directions and spawn bullets.
        /// </summary>
        public void Fire(FireRequest request)
        {
            var stats = _weapon.Get<StatComponent>();
            var bulletSystem = SystemRepository.Instance.GetSystem<IBulletSystem>();
            if (bulletSystem == null)
            {
                Debug.LogError("[WeaponFireExecutor] IBulletSystem not registered");
                return;
            }

            // All bullet params come from stats (base values initialized by WeaponEntityFactory, buffs applied via modifiers)
            float damage = stats?.GetValue(StatDamage) ?? 10f;
            float bulletSpeed = stats?.GetValue(StatBulletSpeed) ?? 50f;
            float bulletRadius = stats?.GetValue(StatBulletRadius) ?? 0.15f;
            float speedMultiplier = stats?.GetValue(StatSpeedMultiplier) ?? 1f;
            int extraPenetrations = stats != null ? (int)stats.GetValue(StatExtraPenetrations) : 0;
            int extraBounces = stats != null ? (int)stats.GetValue(StatExtraBounces) : 0;

            Vector3 logicOrigin = Camera.main != null
                ? Camera.main.transform.position
                : (_spawnPoint != null ? _spawnPoint.position : Vector3.zero);

            Vector3 visualOrigin = _spawnPoint != null
                ? _spawnPoint.position
                : logicOrigin;

            Vector3 baseForward = _spawnPoint != null ? _spawnPoint.forward : Vector3.forward;

            for (int i = 0; i < request.BulletCount; i++)
            {
                Vector3 direction = CalculateDirection(baseForward, request.SpreadAngle, i, request.BulletCount);
                float finalDamage = damage * request.DamageMultiplier;

                var fireParams = new BulletFireParams
                {
                    ConfigId = _bulletConfigId,
                    LogicOrigin = logicOrigin,
                    VisualOrigin = visualOrigin,
                    Direction = direction,
                    Source = _weapon.gameObject,
                    FinalDamage = (int)finalDamage,
                    FinalSpeed = bulletSpeed * speedMultiplier,
                    FinalRadius = bulletRadius,
                    ExtraPenetrations = extraPenetrations,
                    ExtraBounces = extraBounces,
                    SpeedMultiplier = speedMultiplier
                };
                bulletSystem.FireBullet(fireParams);
            }
        }

        private static Vector3 CalculateDirection(Vector3 forward, float spreadAngle, int index, int totalCount)
        {
            if (spreadAngle <= 0f || totalCount <= 1)
                return forward;

            // Uniform cone spread
            float halfAngle = spreadAngle * 0.5f;
            float randomYaw = Random.Range(-halfAngle, halfAngle);
            float randomPitch = Random.Range(-halfAngle, halfAngle);
            return Quaternion.Euler(randomPitch, randomYaw, 0f) * forward;
        }
    }
}
