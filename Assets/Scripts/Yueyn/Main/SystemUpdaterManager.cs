using Yueyn.Utils;

namespace Yueyn.Main
{
    /// <summary>
    /// 系统更新管理器，负责调度系统更新并管理暂停逻辑
    /// </summary>
    public class SystemUpdaterManager : Singleton<SystemUpdaterManager>
    {
        private SystemUpdater _gameUpdater;
        private SystemUpdater _frameworkUpdater;
        private bool _isPaused;
        
        protected override void Init()
        {
            _gameUpdater = new SystemUpdater();
            _frameworkUpdater = new SystemUpdater();
            _isPaused = false;
        }
        
        /// <summary>
        /// 注册系统到对应的更新器
        /// </summary>
        public void RegisterSystem(ISystem system)
        {
            if (system is ILogicUpdate logicUpdate)
            {
                var updater = logicUpdate.LogicUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.RegisterLogicUpdate(logicUpdate);
            }
            
            if (system is IFrameUpdate frameUpdate)
            {
                var updater = frameUpdate.FrameUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.RegisterFrameUpdate(frameUpdate);
            }
            
            if (system is ILateFrameUpdate lateFrameUpdate)
            {
                var updater = lateFrameUpdate.LateFrameUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.RegisterLateFrameUpdate(lateFrameUpdate);
            }
        }
        
        /// <summary>
        /// 注销系统
        /// </summary>
        public void UnregisterSystem(ISystem system)
        {
            if (system is ILogicUpdate logicUpdate)
            {
                var updater = logicUpdate.LogicUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.UnregisterLogicUpdate(logicUpdate);
            }
            
            if (system is IFrameUpdate frameUpdate)
            {
                var updater = frameUpdate.FrameUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.UnregisterFrameUpdate(frameUpdate);
            }
            
            if (system is ILateFrameUpdate lateFrameUpdate)
            {
                var updater = lateFrameUpdate.LateFrameUpdateScope == SystemScope.Game ? _gameUpdater : _frameworkUpdater;
                updater.UnregisterLateFrameUpdate(lateFrameUpdate);
            }
        }
        
        /// <summary>
        /// 逻辑更新（FixedUpdate）
        /// </summary>
        public void LogicUpdate(float deltaTime)
        {
            _frameworkUpdater.DoLogicUpdate(deltaTime);
            if (!_isPaused)
            {
                _gameUpdater.DoLogicUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// 帧更新（Update）
        /// </summary>
        public void FrameUpdate(float deltaTime)
        {
            _frameworkUpdater.DoFrameUpdate(deltaTime);
            if (!_isPaused)
            {
                _gameUpdater.DoFrameUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// 延迟帧更新（LateUpdate）
        /// </summary>
        public void LateFrameUpdate(float deltaTime)
        {
            _frameworkUpdater.DoLateFrameUpdate(deltaTime);
            if (!_isPaused)
            {
                _gameUpdater.DoLateFrameUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// 暂停游戏逻辑
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }
        
        /// <summary>
        /// 恢复游戏逻辑
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }
        
        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused => _isPaused;
    }
}
