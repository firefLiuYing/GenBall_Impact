using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// UI组件基类，用于页面内的可复用组件
    /// 组件会跟随页面的生命周期自动调用对应方法
    /// </summary>
    public abstract class UIComponent : MonoBehaviour
    {
        /// <summary>
        /// 优先级，数值越小越先执行（用于控制初始化顺序）
        /// </summary>
        public virtual int Priority => 0;

        /// <summary>
        /// 所属的UI页面
        /// </summary>
        protected UIFormScript Form { get; private set; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// 是否有焦点
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// 是否已暂停
        /// </summary>
        public bool IsPaused { get; private set; }

        // ===== 内部方法（由UIFormScript调用） =====

        internal void InternalInit(UIFormScript form)
        {
            if (IsInitialized) return;

            Form = form;
            OnInit();
            IsInitialized = true;
        }

        internal void InternalOpen()
        {
            if (IsOpen) return;

            OnOpen();
            IsOpen = true;
        }

        internal void InternalClose()
        {
            if (!IsOpen) return;

            OnClose();
            IsOpen = false;
        }

        internal void InternalFocus()
        {
            if (IsFocused) return;

            OnFocus();
            IsFocused = true;
        }

        internal void InternalUnfocus()
        {
            if (!IsFocused) return;

            OnUnfocus();
            IsFocused = false;
        }

        internal void InternalPause()
        {
            if (IsPaused) return;

            OnPause();
            IsPaused = true;
        }

        internal void InternalResume()
        {
            if (!IsPaused) return;

            OnResume();
            IsPaused = false;
        }

        // ===== 子类重写的生命周期方法 =====

        /// <summary>
        /// 初始化（在页面Init时调用）
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 打开（在页面Open时调用）
        /// </summary>
        protected virtual void OnOpen() { }

        /// <summary>
        /// 关闭（在页面Close时调用）
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 获得焦点（在页面Focus时调用）
        /// </summary>
        protected virtual void OnFocus() { }

        /// <summary>
        /// 失去焦点（在页面Unfocus时调用）
        /// </summary>
        protected virtual void OnUnfocus() { }

        /// <summary>
        /// 暂停（在页面Pause时调用）
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// 恢复（在页面Resume时调用）
        /// </summary>
        protected virtual void OnResume() { }

        /// <summary>
        /// 分辨率变化（在屏幕分辨率改变时调用）
        /// </summary>
        public virtual void OnResolutionChanged(Vector2 resolution) { }
    }
}
