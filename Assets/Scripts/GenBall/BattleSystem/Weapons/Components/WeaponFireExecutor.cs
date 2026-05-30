using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Weapons.Components.Trigger;
using UnityEngine;

namespace GenBall.BattleSystem.Weapons.Components
{
    /// <summary>
    /// Weapon-internal Executor layer.
    /// Handles bullet spawning with spread calculation.
    /// Currently stubbed — will connect to IBulletSystem.FireBullet() after bullet migration.
    /// </summary>
    public class WeaponFireExecutor
    {
        private readonly BattleEntity _weapon;
        private readonly Transform _spawnPoint;

        private const string StatDamage = "Damage";

        public WeaponFireExecutor(BattleEntity weapon, Transform spawnPoint)
        {
            _weapon = weapon;
            _spawnPoint = spawnPoint;
        }

        /// <summary>
        /// Execute a fire request: calculate spread directions and spawn bullets.
        /// </summary>
        public void Fire(FireRequest request)
        {
            var stats = _weapon.Get<StatComponent>();
            float baseDamage = stats?.GetValue(StatDamage) ?? 10f;

            for (int i = 0; i < request.BulletCount; i++)
            {
                var direction = CalculateDirection(request);
                float damage = baseDamage * request.DamageMultiplier;

                // TODO: 子弹系统迁移后接入 IBulletSystem.FireBullet()
                // var launchInfo = BulletLaunchInfo.Create(model, source, spawnPos, direction);
                // SystemRepository.Instance.GetSystem<IBulletSystem>().FireBullet(launchInfo);
                Debug.Log($"[WeaponFireExecutor] Fire bullet {i + 1}/{request.BulletCount}: " +
                          $"damage={damage}, dir={direction}, spread={request.SpreadAngle}");
            }
        }

        private Vector3 CalculateDirection(FireRequest request)
        {
            // Get base forward direction
            Vector3 forward = _spawnPoint != null ? _spawnPoint.forward : Vector3.forward;

            if (request.SpreadAngle <= 0f)
                return forward;

            // Uniform cone spread
            float halfAngle = request.SpreadAngle * 0.5f;
            float randomYaw = UnityEngine.Random.Range(-halfAngle, halfAngle);
            float randomPitch = UnityEngine.Random.Range(-halfAngle, halfAngle);
            return Quaternion.Euler(randomPitch, randomYaw, 0f) * forward;
        }
    }
}
