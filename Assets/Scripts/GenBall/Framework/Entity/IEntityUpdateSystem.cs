using Yueyn.Main;

namespace GenBall.Framework.Entity
{
    public interface IEntityUpdateSystem : ISystem
    {
        void AddFrameUpdate(IEntityFrameUpdate entity);
        void RemoveFrameUpdate(IEntityFrameUpdate entity);
        void AddLogicUpdate(IEntityLogicUpdate entity);
        void RemoveLogicUpdate(IEntityLogicUpdate entity);
    }
}
