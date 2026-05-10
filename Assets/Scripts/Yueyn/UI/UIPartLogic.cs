using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 子页面Logic基类
    /// 用于非全屏子页面的业务逻辑
    /// </summary>
    public abstract class UIPartLogic : UILogicBase
    {
        /// <summary>
        /// 子页面View引用
        /// </summary>
        protected UIPartView View { get; private set; }

        /// <summary>
        /// 绑定View（需要子类指定具体的PartView类型）
        /// </summary>
        internal override void BindView(UIFormScript form)
        {
            SetForm(form);
            // 子类需要调用BindView<T>来绑定具体的PartView
        }

        /// <summary>
        /// 绑定指定类型的PartView（子类在BindView中调用）
        /// </summary>
        protected void BindView<TView>(UIFormScript form) where TView : UIPartView
        {
            SetForm(form);
            View = form.GetComponentInChildren<TView>();

            if (View == null)
            {
                Debug.LogError($"[{GetType().Name}] UIPartView<{typeof(TView).Name}> not found in FormScript!");
            }
        }

        /// <summary>
        /// 显示子页面
        /// </summary>
        public virtual void Show()
        {
            if (View != null)
            {
                View.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 隐藏子页面
        /// </summary>
        public virtual void Hide()
        {
            if (View != null)
            {
                View.gameObject.SetActive(false);
            }
        }
    }
}
