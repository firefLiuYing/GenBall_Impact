using GenBall.BattleSystem;
using GenBall.Utils.EntityCreator;

namespace GenBall.Map
{
    public interface IMapBlock : IEntity
    {
        public void SetIndex(int index);
    }
}