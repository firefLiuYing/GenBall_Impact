using GenBall.BattleSystem;
using GenBall.Utils.EntityCreator;

namespace GenBall.Enemy
{
    public interface IEnemy : IAttackable,IEntity
    {
        public void Initialize();
    }
}