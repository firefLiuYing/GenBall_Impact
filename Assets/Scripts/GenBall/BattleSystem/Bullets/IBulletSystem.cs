using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    /// <summary>
    /// Bullet system interface. Handles bullet lifecycle — fire, recycle, pooling.
    /// </summary>
    public interface IBulletSystem : ISystem
    {
        /// <summary>
        /// Fire a bullet with the given runtime parameters.
        /// Looks up BulletConfig from BulletConfigCollection, creates BulletInstance from pool,
        /// and starts the bullet's lifecycle.
        /// </summary>
        void FireBullet(BulletFireParams fireParams);

        /// <summary>
        /// Recycle a bullet back to the pool by its instance ID.
        /// </summary>
        void RecycleBullet(int bulletId);

        // ── Legacy API for backward compatibility during migration ──
        // These are kept for NormalTriggerController (old weapon path).
        // Will be removed in Phase E cleanup.

        /// <summary>[Obsolete] Use FireBullet(BulletFireParams) instead.</summary>
        void FireBullet(BulletLaunchInfo info);

        /// <summary>[Obsolete] Use RecycleBullet(int bulletId) instead.</summary>
        void RecycleBullet(BulletState bulletState);
    }
}
