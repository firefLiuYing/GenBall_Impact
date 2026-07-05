namespace Yueyn.UI
{
    /// <summary>
    /// UI页面类型
    /// </summary>
    public enum UIFormType
    {
        /// <summary>
        /// 常驻UI，不进栈，始终显示（如MainHud、血条等）
        /// </summary>
        Persistent,

        /// <summary>
        /// 弹窗UI，列表管理，可以多层（如背包、暂停菜单等）
        /// </summary>
        Popup,

        /// <summary>
        /// 过场UI，独占显示，阻塞其他UI（如加载界面、启动画面等）
        /// </summary>
        Transition,

        /// <summary>
        /// 世界空间UI，3D定位在场景中（如存档点全息界面）
        /// Canvas.renderMode = WorldSpace，不使用 UICamera
        /// </summary>
        WorldSpace
    }
}
