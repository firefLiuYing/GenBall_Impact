using GenBall.BattleSystem;
using GenBall.Utils.EntityCreator;

namespace GenBall.Map
{
    public interface IMapBlock : IEntity,IEffectable
    {
        public void EnterMapBlock();
        public void ExitMapBlock();
    }
}