using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// UI组件基类，用于页面内的可复用组件
    /// 组件会跟随页面的生命周期自动调用对应方法
    ///
    /// 设计原则：
    /// - 实现通用功能（如按钮点击、动画播放等）
    /// - 通过 internal 方法限制只能由框架调用，protected virtual 方法允许子类重写
    /// - 不包含业务逻辑（业务逻辑由第二层次处理）
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

        // ===== 框架方法（internal，只能由框架调用） =====

        /// <summary>
        /// 启动方法（由框架调用）
        /// </summary>
        internal void DoStart(UIFormScript form)
        {
            if (IsInitialized) return;

            Form = form;
            DoBusinessStart();
            IsInitialized = true;
        }

        /// <summary>
        /// 打开方法（由框架调用）
        /// </summary>
        internal void DoOpen()
        {
            if (IsOpen) return;

            DoBusinessOpen();
            IsOpen = true;
        }

        /// <summary>
        /// 关闭方法（由框架调用）
        /// </summary>
        internal void DoClose()
        {
            if (!IsOpen) return;

            DoBusinessClose();
            IsOpen = false;
        }

        /// <summary>
        /// 获得焦点方法（由框架调用）
        /// </summary>
        internal void DoFocus()
        {
            if (IsFocused) return;

            DoBusinessFocus();
            IsFocused = true;
        }

        /// <summary>
        /// 失去焦点方法（由框架调用）
        /// </summary>
        internal void DoUnfocus()
        {
            if (!IsFocused) return;

            DoBusinessUnfocus();
            IsFocused = false;
        }

        /// <summary>
        /// 暂停方法（由框架调用）
        /// </summary>
        internal void DoPause()
        {
            if (IsPaused) return;

            DoBusinessPause();
            IsPaused = true;
        }

        /// <summary>
        /// 恢复方法（由框架调用）
        /// </summary>
        internal void DoResume()
        {
            if (!IsPaused) return;

            DoBusinessResume();
            IsPaused = false;
        }

        // ===== 业务方法（protected virtual，供子类重写） =====

        /// <summary>
        /// 业务启动方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessStart() { }

        /// <summary>
        /// 业务打开方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessOpen() { }

        /// <summary>
        /// 业务关闭方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessClose() { }

        /// <summary>
        /// 业务获得焦点方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessFocus() { }

        /// <summary>
        /// 业务失去焦点方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessUnfocus() { }

        /// <summary>
        /// 业务暂停方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessPause() { }

        /// <summary>
        /// 业务恢复方法（供子类重写）
        /// </summary>
        protected virtual void DoBusinessResume() { }

        /// <summary>
        /// 分辨率变化（在屏幕分辨率改变时调用）
        /// </summary>
        public virtual void OnResolutionChanged(Vector2 resolution) { }
    }
}
