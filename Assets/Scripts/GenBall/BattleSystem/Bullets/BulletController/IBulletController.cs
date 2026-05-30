namespace GenBall.BattleSystem.Bullets.BulletController
{
    [System.Obsolete("Replaced by IDetectionStrategy + IHitBehavior + IMovementModifier. Will be removed in Phase E cleanup.")]
    public interface IBulletController
    {
        public void Init(BulletState bulletState);
        public void Fire();
        public void Tick(float deltaTime);
    }
}