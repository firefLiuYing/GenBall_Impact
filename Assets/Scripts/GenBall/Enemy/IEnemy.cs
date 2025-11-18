using GenBall.BattleSystem;

namespace GenBall.Enemy
{
    public interface IEnemy : IAttackable
    {
        public void EnemyUpdate(float deltaTime);
        public void EnemyFixedUpdate(float fixedDeltaTime);
    }
}