using System.Collections.Generic;
using Yueyn.Main;
using Yueyn.Utils;

namespace GenBall.Framework.Entity
{
    public class EntityUpdateSystem : IEntityUpdateSystem, IFrameUpdate, ILogicUpdate
    {
        private readonly SafeIterableList<IEntityFrameUpdate> _frameUpdates = new();
        private readonly SafeIterableList<IEntityLogicUpdate> _logicUpdates = new();

        public SystemScope FrameUpdateScope => SystemScope.Game;
        public SystemScope LogicUpdateScope => SystemScope.Game;

        public void Init() { }
        public void UnInit()
        {
            _frameUpdates.Clear();
            _logicUpdates.Clear();
        }

        public void AddFrameUpdate(IEntityFrameUpdate entity) => _frameUpdates.Add(entity);
        public void RemoveFrameUpdate(IEntityFrameUpdate entity) => _frameUpdates.Remove(entity);
        public void AddLogicUpdate(IEntityLogicUpdate entity) => _logicUpdates.Add(entity);
        public void RemoveLogicUpdate(IEntityLogicUpdate entity) => _logicUpdates.Remove(entity);

        public void FrameUpdate(float deltaTime)
        {
            var snapshot = _frameUpdates.GetIterableSnapshot();
            foreach (var entity in snapshot)
                entity.FrameUpdate(deltaTime);
        }

        public void LogicUpdate(float deltaTime)
        {
            var snapshot = _logicUpdates.GetIterableSnapshot();
            foreach (var entity in snapshot)
                entity.LogicUpdate(deltaTime);
        }
    }
}
