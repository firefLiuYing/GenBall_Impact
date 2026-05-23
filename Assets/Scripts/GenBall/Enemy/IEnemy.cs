using GenBall.BattleSystem;

namespace GenBall.Enemy
{
    public interface IEnemy : IDamageable
    {
        public void Initialize();
    }
}