using GenBall.BattleSystem;
using GenBall.Utils.EntityCreator;

namespace GenBall.Enemy
{
    public interface IEnemy : IDamageable,IEntity
    {
        public void Initialize();
    }
}