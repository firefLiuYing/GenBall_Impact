using GenBall.BattleSystem;
using GenBall.Utils.EntityCreator;

namespace GenBall.Enemy
{
    public interface IEnemy : IInteractable,IEntity
    {
        public void Initialize();
    }
}