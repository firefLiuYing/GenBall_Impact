using System;
using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// UI Logic基类
    /// 所有Logic必须继承此类
    /// </summary>
    public abstract class UILogicBase
    {
        /// <summary>
        /// Logic唯一ID（由UILogicManager分配）
        /// </summary>
        public int LogicId { get; private set; }

        /// <summary>
        /// 关联的UIFormScript
        /// </summary>
        protected UIFormScript Form { get; private set; }

        /// <summary>
        /// 设置Form引用（供子类在BindView中使用）
        /// </summary>
        protected void SetForm(UIFormScript form)
        {
            Form = form;
        }

        /// <summary>
        /// 预制体路径（子类必须提供）
        /// </summary>
        protected abstract string PrefabPath { get; }

        /// <summary>
        /// 设置Logic ID（由UILogicManager调用）
        /// </summary>
        internal void SetLogicId(int id)
        {
            LogicId = id;
        }

        /// <summary>
        /// 打开页面（外部调用入口）
        /// </summary>
        /// <param name="param">初始化参数</param>
        public void OpenForm(object param = null)
        {
            var uiSystem = Yueyn.Main.SystemRepository.Instance.GetSystem<IUISystem>();
            if (uiSystem == null)
            {
                Debug.LogError("[UILogicBase] UISystem not found!");
                return;
            }

            Form = uiSystem.OpenForm(PrefabPath, this, param);
        }

        /// <summary>
        /// 异步打开页面（外部调用入口）
        /// </summary>
        /// <param name="param">初始化参数</param>
        /// <param name="onComplete">完成回调（参数为打开的页面实例，失败时为null）</param>
        /// <param name="onProgress">进度回调（0-1）</param>
        public void OpenFormAsync(object param = null, Action<UIFormScript> onComplete = null, Action<float> onProgress = null)
        {
            var uiSystem = Yueyn.Main.SystemRepository.Instance.GetSystem<IUISystem>();
            if (uiSystem == null)
            {
                Debug.LogError("[UILogicBase] UISystem not found!");
                onComplete?.Invoke(null);
                return;
            }

            uiSystem.OpenFormAsync(PrefabPath, this, param,
                onComplete: (form) =>
                {
                    Form = form;
                    onComplete?.Invoke(form);
                },
                onProgress: onProgress);
        }

        /// <summary>
        /// 关闭页面
        /// </summary>
        /// <param name="immediate">是否立即销毁</param>
        public void CloseForm(bool immediate = false)
        {
            if (Form != null)
            {
                var uiSystem = Yueyn.Main.SystemRepository.Instance.GetSystem<IUISystem>();
                uiSystem?.CloseForm(Form.FormId, immediate);
            }
        }

        /// <summary>
        /// 绑定View（由UIFormScript调用）
        /// 子类重写此方法来获取具体的View引用
        /// </summary>
        internal abstract void BindView(UIFormScript form);

        /// <summary>
        /// 设置View数据（在BindView后调用）
        /// 子类重写此方法来设置View的初始数据
        /// </summary>
        /// <param name="param">初始化参数</param>
        public abstract void SetViewData(object param);

        // ===== 生命周期方法 =====

        /// <summary>
        /// 初始化（在UIFormScript.InternalInit时调用）
        /// </summary>
        public virtual void OnInit(object param) { }

        /// <summary>
        /// 进入（在UIFormScript.InternalOpen时调用）
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 退出（在UIFormScript.InternalClose时调用）
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 获得焦点（在UIFormScript.InternalFocus时调用）
        /// </summary>
        public virtual void OnFocus() { }

        /// <summary>
        /// 失去焦点（在UIFormScript.InternalUnfocus时调用）
        /// </summary>
        public virtual void OnUnfocus() { }

        /// <summary>
        /// 暂停（在UIFormScript.InternalPause时调用）
        /// </summary>
        public virtual void OnPause() { }

        /// <summary>
        /// 恢复（在UIFormScript.InternalResume时调用）
        /// </summary>
        public virtual void OnResume() { }

        /// <summary>
        /// 每帧更新（需要手动在子类中调用）
        /// </summary>
        public virtual void OnUpdate() { }
    }
}
