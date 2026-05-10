using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 根页面Logic基类
    /// 用于全屏页面的业务逻辑
    /// </summary>
    public abstract class UIFormLogic : UILogicBase
    {
        /// <summary>
        /// 根页面View引用
        /// </summary>
        protected UIFormView View { get; private set; }

        /// <summary>
        /// 绑定View（自动查找UIFormView）
        /// </summary>
        internal override void BindView(UIFormScript form)
        {
            SetForm(form);
            View = form.GetComponentInChildren<UIFormView>();

            if (View == null)
            {
                Debug.LogError($"[{GetType().Name}] UIFormView not found in FormScript!");
            }
        }
    }
}
