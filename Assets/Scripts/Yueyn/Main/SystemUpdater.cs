using System.Collections.Generic;
using Yueyn.Utils;

namespace Yueyn.Main
{
    /// <summary>
    /// 系统更新器（具体执行更新的类）
    /// 使用 SafeIterableList 工具类实现迭代安全
    /// </summary>
    public class SystemUpdater
    {
        // 使用 SafeIterableList 替代手动双缓冲
        private readonly SafeIterableList<ILogicUpdate> _logicUpdates = new();
        private readonly SafeIterableList<IFrameUpdate> _frameUpdates = new();
        private readonly SafeIterableList<ILateFrameUpdate> _lateFrameUpdates = new();

        // Game 作用域的更新列表
        private readonly SafeIterableList<ILogicUpdate> _gameLogicUpdates = new();
        private readonly SafeIterableList<IFrameUpdate> _gameFrameUpdates = new();
        private readonly SafeIterableList<ILateFrameUpdate> _gameLateFrameUpdates = new();

        // Framework 作用域的更新列表
        private readonly SafeIterableList<ILogicUpdate> _frameworkLogicUpdates = new();
        private readonly SafeIterableList<IFrameUpdate> _frameworkFrameUpdates = new();
        private readonly SafeIterableList<ILateFrameUpdate> _frameworkLateFrameUpdates = new();

        public void RegisterLogicUpdate(ILogicUpdate update)
        {
            _logicUpdates.Add(update);
            
            if (update.LogicUpdateScope == SystemScope.Game)
                _gameLogicUpdates.Add(update);
            else
                _frameworkLogicUpdates.Add(update);
        }

        public void RegisterFrameUpdate(IFrameUpdate update)
        {
            _frameUpdates.Add(update);
            
            if (update.FrameUpdateScope == SystemScope.Game)
                _gameFrameUpdates.Add(update);
            else
                _frameworkFrameUpdates.Add(update);
        }

        public void RegisterLateFrameUpdate(ILateFrameUpdate update)
        {
            _lateFrameUpdates.Add(update);
            
            if (update.LateFrameUpdateScope == SystemScope.Game)
                _gameLateFrameUpdates.Add(update);
            else
                _frameworkLateFrameUpdates.Add(update);
        }

        public void UnregisterLogicUpdate(ILogicUpdate update)
        {
            _logicUpdates.Remove(update);
            _gameLogicUpdates.Remove(update);
            _frameworkLogicUpdates.Remove(update);
        }

        public void UnregisterFrameUpdate(IFrameUpdate update)
        {
            _frameUpdates.Remove(update);
            _gameFrameUpdates.Remove(update);
            _frameworkFrameUpdates.Remove(update);
        }

        public void UnregisterLateFrameUpdate(ILateFrameUpdate update)
        {
            _lateFrameUpdates.Remove(update);
            _gameLateFrameUpdates.Remove(update);
            _frameworkLateFrameUpdates.Remove(update);
        }

        public void DoLogicUpdate(float deltaTime)
        {
            // Framework 作用域：不受暂停影响
            List<ILogicUpdate> frameworkSnapshot = _frameworkLogicUpdates.GetIterableSnapshot();
            foreach (var update in frameworkSnapshot)
            {
                update.LogicUpdate(deltaTime);
            }

            bool isPaused = false;
            // 后面会实现新的全局暂停方法
            // Game 作用域：受暂停影响
            if (!isPaused)
            {
                List<ILogicUpdate> gameSnapshot = _gameLogicUpdates.GetIterableSnapshot();
                foreach (var update in gameSnapshot)
                {
                    update.LogicUpdate(deltaTime);
                }
            }
        }

        public void DoFrameUpdate(float deltaTime)
        {
            // Framework 作用域：不受暂停影响
            List<IFrameUpdate> frameworkSnapshot = _frameworkFrameUpdates.GetIterableSnapshot();
            foreach (var update in frameworkSnapshot)
            {
                update.FrameUpdate(deltaTime);
            }

            bool isPaused = false;
            // 后面会实现新的全局暂停方法
            // Game 作用域：受暂停影响
            if (!isPaused)
            {
                List<IFrameUpdate> gameSnapshot = _gameFrameUpdates.GetIterableSnapshot();
                foreach (var update in gameSnapshot)
                {
                    update.FrameUpdate(deltaTime);
                }
            }
        }

        public void DoLateFrameUpdate(float deltaTime)
        {
            // Framework 作用域：不受暂停影响
            List<ILateFrameUpdate> frameworkSnapshot = _frameworkLateFrameUpdates.GetIterableSnapshot();
            foreach (var update in frameworkSnapshot)
            {
                update.LateFrameUpdate(deltaTime);
            }

            bool isPaused = false;
            // 后面会实现新的全局暂停方法
            // Game 作用域：受暂停影响
            if (!isPaused)
            {
                List<ILateFrameUpdate> gameSnapshot = _gameLateFrameUpdates.GetIterableSnapshot();
                foreach (var update in gameSnapshot)
                {
                    update.LateFrameUpdate(deltaTime);
                }
            }
        }
    }
}
