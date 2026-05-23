using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    public interface IBulletSystem : ISystem
    {
        void FireBullet(BulletLaunchInfo info);
        void RecycleBullet(BulletState bulletState);
    }
}
