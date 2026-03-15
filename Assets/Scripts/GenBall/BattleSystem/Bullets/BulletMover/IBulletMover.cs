namespace GenBall.BattleSystem.Bullets.BulletMover
{
    public interface IBulletMover
    {
        public void Init(BulletState bulletState);
        public void Fire();
        public void Tick(float deltaTime);
    }
}