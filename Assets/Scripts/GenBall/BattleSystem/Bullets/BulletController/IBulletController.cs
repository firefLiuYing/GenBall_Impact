namespace GenBall.BattleSystem.Bullets.BulletController
{
    public interface IBulletController
    {
        public void Init(BulletState bulletState);
        public void Fire();
        public void Tick(float deltaTime);
    }
}