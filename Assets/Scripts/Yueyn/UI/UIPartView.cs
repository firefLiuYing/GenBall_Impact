using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 子页面View基类
    /// 用于非全屏子页面的View层，继承自UIComponent
    /// 子页面通常是半屏、弹窗、提示等小型UI元素
    /// </summary>
    public abstract class UIPartView : UIComponent
    {
        [Header("Part View Configuration")]
        [SerializeField]
        [Tooltip("RectTransform引用（用于定位和大小调整）")]
        protected RectTransform _rectTransform;

        /// <summary>
        /// RectTransform引用
        /// </summary>
        public RectTransform RectTransform => _rectTransform;

        /// <summary>
        /// 初始化时自动获取RectTransform
        /// </summary>
        protected override void OnInit()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_rectTransform == null)
            {
                Debug.LogWarning($"[{GetType().Name}] RectTransform not found!");
            }
        }

        /// <summary>
        /// 子页面的分辨率适配逻辑（与全屏页面不同）
        /// 子类可以重写此方法实现自定义的分辨率适配
        /// </summary>
        /// <param name="resolution">当前分辨率</param>
        public override void OnResolutionChanged(Vector2 resolution)
        {
            // 默认的子页面适配逻辑
            // 例如：保持相对位置，调整大小等
            // 子类可以重写实现自定义逻辑
        }

        /// <summary>
        /// 设置子页面位置
        /// </summary>
        /// <param name="position">世界坐标位置</param>
        public virtual void SetPosition(Vector3 position)
        {
            if (_rectTransform != null)
            {
                _rectTransform.position = position;
            }
        }

        /// <summary>
        /// 设置子页面锚点位置
        /// </summary>
        /// <param name="anchoredPosition">锚点位置</param>
        public virtual void SetAnchoredPosition(Vector2 anchoredPosition)
        {
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = anchoredPosition;
            }
        }

        /// <summary>
        /// 设置子页面大小
        /// </summary>
        /// <param name="size">大小</param>
        public virtual void SetSize(Vector2 size)
        {
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = size;
            }
        }
    }
}
