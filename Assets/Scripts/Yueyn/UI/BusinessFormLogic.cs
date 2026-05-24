using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 页面业务逻辑基类
    /// 与 UIFormScript 绑定，管理页面的业务逻辑。
    ///
    /// OnCreate → 自动打开 Form → OnFormCreated()
    /// OnDestroy → 自动关闭 Form → OnFormDestroying()
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

        // ===== 框架生命周期（sealed via base class） =====

        protected override void OnCreateInternal()
        {
            base.OnCreateInternal();
            var formId = UIManager.Instance.OpenForm(PrefabPath, FormType, InitParam);
            if (formId != -1)
            {
                BindForm(UIManager.Instance.GetForm(formId));
            }
            OnFormCreated();
            // 自动发现并初始化子 Part
            if (BoundForm != null)
                DiscoverChildPartLogics(BoundForm.transform);
        }

        protected override void OnDestroyInternal()
        {
            OnFormDestroying();
            if (BoundForm != null)
            {
                var formId = BoundForm.FormId;
                UnbindForm();
                UIManager.Instance.CloseForm(formId);
            }
            base.OnDestroyInternal();
        }

        // ===== 子类钩子（protected virtual） =====

        /// <summary>
        /// Form 创建并绑定后调用（供子类重写业务逻辑）
        /// </summary>
        protected virtual void OnFormCreated() { }

        /// <summary>
        /// Form 销毁前调用（供子类重写清理逻辑）
        /// </summary>
        protected virtual void OnFormDestroying() { }

        // ===== Form 绑定 / 解绑 =====

        private void BindForm(UIFormScript form)
        {
            if (BoundForm != null)
                UnbindForm();

            BoundForm = form;
            OnFormBound(form);
        }

        private void UnbindForm()
        {
            if (BoundForm == null)
                return;

            OnFormUnbound(BoundForm);
            BoundForm = null;
        }

        // ===== Form 生命周期回调（供子类重写） =====

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
        /// 关闭页面（通过销毁自身 Logic）
        /// </summary>
        public void CloseForm()
        {
            if (BoundForm != null)
                BusinessLogicManager.Instance.DestroyLogic(LogicId);
        }
    }
}
