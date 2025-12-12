namespace GenBall.Enemy.Attack
{
    public abstract class AttackModule : Module
    {
        public abstract void StartAttack();
        public abstract void StopAttack();
        public abstract bool CanAttack();
    }
}