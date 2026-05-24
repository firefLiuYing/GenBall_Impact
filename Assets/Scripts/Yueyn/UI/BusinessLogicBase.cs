namespace Yueyn.UI
{
    /// <summary>
    /// 业务逻辑基类
    ///
    /// 生命周期体系：public 方法 sealed（框架控制）→ protected virtual 方法（子类重写）
    /// - OnCreate() → OnCreateInternal()
    /// - OnDestroy() → OnDestroyInternal()
    /// </summary>
    public abstract class BusinessLogicBase
    {
        private bool _isCreated;
        private bool _isDestroyed;

        /// <summary>
        /// Logic 唯一 ID
        /// </summary>
        public int LogicId { get; private set; }

        /// <summary>
        /// 设置 Logic ID（由 BusinessLogicManager 调用）
        /// </summary>
        internal void SetLogicId(int id)
        {
            LogicId = id;
        }

        // ===== 框架生命周期（public，防重复调用） =====

        /// <summary>
        /// 创建（由 BusinessLogicManager 调用）
        /// </summary>
        public void OnCreate()
        {
            if (_isCreated) return;
            _isCreated = true;
            OnCreateInternal();
        }

        /// <summary>
        /// 销毁（由 BusinessLogicManager 调用）
        /// </summary>
        public void OnDestroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;
            OnDestroyInternal();
        }

        // ===== 子类生命周期钩子（protected virtual） =====

        /// <summary>
        /// 创建时子类逻辑（供子类重写）
        /// </summary>
        protected virtual void OnCreateInternal() { }

        /// <summary>
        /// 销毁时子类逻辑（供子类重写）
        /// </summary>
        protected virtual void OnDestroyInternal() { }
    }
}
