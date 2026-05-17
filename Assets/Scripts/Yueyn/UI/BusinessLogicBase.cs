namespace Yueyn.UI
{
    /// <summary>
    /// 业务逻辑基类
    /// 可以是 BusinessFormLogic（绑定页面）或 BusinessPartLogic（子页面逻辑）
    /// </summary>
    public abstract class BusinessLogicBase
    {
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

        // ===== 生命周期（供子类重写） =====

        /// <summary>
        /// 创建时调用
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// 销毁时调用
        /// </summary>
        public virtual void OnDestroy() { }
    }
}
