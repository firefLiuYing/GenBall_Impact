namespace GenBall.Procedure.Execute
{
    /// <summary>
    /// 启动流程全局事件键（配合 CEventRouter.Instance 使用）
    /// Procedure 层发射 → UI 层监听，互不依赖
    /// </summary>
    public static class LaunchEventKey
    {
        /// <summary>进入 Splash/Loading 阶段</summary>
        public const int SplashBegin = 1;

        /// <summary>Splash/Loading 阶段结束</summary>
        public const int SplashComplete = 2;

        /// <summary>进入主菜单阶段</summary>
        public const int StartFormBegin = 3;

        /// <summary>游戏启动</summary>
        public const int GameLaunch = 4;

        /// <summary>Loading progress update, parameter: float (0-1)</summary>
        public const int LoadingProgress = 5;

        /// <summary>Scene loading complete</summary>
        public const int LoadingComplete = 6;
    }
}
