namespace Yueyn.UI
{
    /// <summary>
    /// 页面业务逻辑基类
    /// 与 UIFormScript 绑定，管理页面的业务逻辑
    /// </summary>
    public abstract class BusinessFormLogic : BusinessPartLogicContainer
    {
        /// <summary>
        /// 预制体路径（子类必须实现）
        /// </summary>
        public abstract string PrefabPath { get; }

        /// <summary>
        /// UI 类型（默认为 Popup）
        /// </summary>
        public virtual UIFormType FormType => UIFormType.Popup;

        /// <summary>
        /// 初始化参数（可选）
        /// </summary>
        public virtual object InitParam => null;

        /// <summary>
        /// 绑定的 UIFormScript
        /// </summary>
        public UIFormScript BoundForm { get; private set; }

        // ===== 绑定 Form =====

        /// <summary>
        /// 绑定 UIFormScript（由 BusinessLogicManager 调用）
        /// </summary>
        internal void BindForm(UIFormScript form)
        {
            if (BoundForm != null)
            {
                UnbindForm();
            }

            BoundForm = form;

            // 监听 Form 的生命周期
            SubscribeFormEvents();

            // 调用绑定回调
            OnFormBound(form);
        }

        /// <summary>
        /// 解绑 UIFormScript
        /// </summary>
        internal void UnbindForm()
        {
            if (BoundForm == null)
                return;

            // 取消监听
            UnsubscribeFormEvents();

            // 调用解绑回调
            OnFormUnbound(BoundForm);

            BoundForm = null;
        }

        // ===== 监听 Form 事件 =====

        private void SubscribeFormEvents()
        {
            // 这里可以监听 Form 的事件（如果需要）
        }

        private void UnsubscribeFormEvents()
        {
            // 取消监听
        }

        // ===== 生命周期回调（供子类重写） =====

        /// <summary>
        /// Form 绑定时调用
        /// </summary>
        protected virtual void OnFormBound(UIFormScript form) { }

        /// <summary>
        /// Form 解绑时调用
        /// </summary>
        protected virtual void OnFormUnbound(UIFormScript form) { }

        // ===== 便捷方法 =====

        /// <summary>
        /// 关闭页面
        /// </summary>
        public void CloseForm()
        {
            if (BoundForm != null)
            {
                BusinessLogicManager.Instance.DestroyLogic(LogicId);
            }
        }
    }
}
