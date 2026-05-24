namespace GenBall.UI
{
    /// <summary>
    /// UI 事件总线键定义
    /// 配合 UIManager.Instance.UIEventRouter 使用
    /// </summary>
    public static class UIEventKey
    {
        // ===== SplashForm 事件 =====

        /// <summary>SplashForm 已打开并绑定完成</summary>
        public const int SplashForm_Opened = 1;

        /// <summary>SplashForm 进度更新，参数 float progress (0-1)</summary>
        public const int SplashForm_ProgressUpdate = 2;

        /// <summary>请求关闭 SplashForm</summary>
        public const int SplashForm_CloseRequest = 3;

        /// <summary>SplashForm 已关闭</summary>
        public const int SplashForm_Closed = 4;

        // ===== StartForm 事件 =====

        /// <summary>开始新游戏</summary>
        public const int StartForm_NewGame = 10;

        /// <summary>继续上次游戏</summary>
        public const int StartForm_Continue = 11;

        /// <summary>加载存档（TODO: 存档选择 UI 完成后传入 int saveIndex）</summary>
        public const int StartForm_LoadGame = 12;

        /// <summary>请求关闭 StartForm</summary>
        public const int StartForm_CloseRequest = 13;

        // ===== GM Console (20-22) =====

        /// <summary>GM Console 提交命令</summary>
        public const int GMConsole_Submit = 20;

        /// <summary>GM Console 关闭请求</summary>
        public const int GMConsole_Close = 21;
    }
}
