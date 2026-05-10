using System;
using Yueyn.Main;

namespace Yueyn.UI
{
    /// <summary>
    /// UI系统接口，管理所有UI页面的生命周期
    /// </summary>
    public interface IUISystem : ISystem
    {
        /// <summary>
        /// 同步打开UI页面（适用于小资源或已缓存的资源）
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="logic">Logic实例（必须提供）</param>
        /// <param name="param">初始化参数</param>
        /// <returns>打开的页面实例</returns>
        UIFormScript OpenForm(string prefabPath, UILogicBase logic, object param = null);

        /// <summary>
        /// 异步打开UI页面（适用于大资源，支持进度回调）
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="logic">Logic实例（必须提供）</param>
        /// <param name="param">初始化参数</param>
        /// <param name="onComplete">完成回调（参数为打开的页面实例，失败时为null）</param>
        /// <param name="onProgress">进度回调（0-1）</param>
        void OpenFormAsync(string prefabPath, UILogicBase logic, object param = null,
            Action<UIFormScript> onComplete = null, Action<float> onProgress = null);

        /// <summary>
        /// 按ID关闭指定页面
        /// </summary>
        /// <param name="formId">页面ID</param>
        /// <param name="immediate">是否立即销毁（不等待动画）</param>
        /// <returns>是否成功关闭</returns>
        bool CloseForm(int formId, bool immediate = false);

        /// <summary>
        /// 按类型关闭页面
        /// </summary>
        /// <typeparam name="T">页面类型</typeparam>
        /// <param name="immediate">是否立即销毁</param>
        /// <returns>是否成功关闭</returns>
        bool CloseFormByType<T>(bool immediate = false) where T : UIFormScript;

        /// <summary>
        /// 关闭所有弹窗UI（不包括常驻UI）
        /// </summary>
        /// <param name="immediate">是否立即销毁</param>
        void CloseAllPopups(bool immediate = false);

        /// <summary>
        /// 关闭所有UI（包括常驻UI）
        /// </summary>
        /// <param name="immediate">是否立即销毁</param>
        void CloseAllForms(bool immediate = false);

        /// <summary>
        /// 获取指定ID的页面
        /// </summary>
        UIFormScript GetForm(int formId);

        /// <summary>
        /// 获取指定类型的页面
        /// </summary>
        T GetForm<T>() where T : UIFormScript;

        /// <summary>
        /// 检查是否存在指定ID的页面
        /// </summary>
        bool HasForm(int formId);

        /// <summary>
        /// 检查是否存在指定类型的页面
        /// </summary>
        bool HasForm<T>() where T : UIFormScript;

        /// <summary>
        /// 暂停所有UI
        /// </summary>
        void PauseAllUI();

        /// <summary>
        /// 恢复所有UI
        /// </summary>
        void ResumeAllUI();
    }
}