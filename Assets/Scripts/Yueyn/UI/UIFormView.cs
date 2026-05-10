using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 根页面View基类
    /// 用于全屏页面的View层，继承自UIComponent
    /// 子类需要实现具体的UI元素引用和交互逻辑
    /// </summary>
    public abstract class UIFormView : UIComponent
    {
        [Header("Form View Configuration")]
        [SerializeField]
        [Tooltip("页面类型")]
        protected UIFormType _formType = UIFormType.Popup;

        [SerializeField]
        [Tooltip("是否响应焦点事件（常驻UI通常设为false）")]
        protected bool _respondToFocus = true;

        /// <summary>
        /// 页面类型
        /// </summary>
        public UIFormType FormType => _formType;

        /// <summary>
        /// 是否响应焦点事件
        /// </summary>
        public bool RespondToFocus => _respondToFocus;

        /// <summary>
        /// 全屏页面的分辨率适配逻辑
        /// 子类可以重写此方法实现自定义的分辨率适配
        /// </summary>
        /// <param name="resolution">当前分辨率</param>
        public override void OnResolutionChanged(Vector2 resolution)
        {
            // 默认的全屏页面适配逻辑
            // 例如：保持16:9比例，超宽屏裁剪等
            // 子类可以重写实现自定义逻辑
        }
    }
}
