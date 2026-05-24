using GenBall.Procedure.Execute;
using Yueyn.Event;
using Yueyn.UI;

namespace GenBall.UI
{
    /// <summary>
    /// 常驻业务逻辑，将启动流程全局事件映射为 UI 表单操作。
    /// SplashForm 身兼二职：启动画面 + 加载画面。
    /// </summary>
    public class SplashBusinessLogic : BusinessLogicBase
    {
        private int _splashFormLogicId = -1;
        private int _startFormLogicId = -1;

        protected override void OnCreateInternal()
        {
            CEventRouter.Instance.Subscribe(LaunchEventKey.SplashBegin, OnSplashBegin);
            CEventRouter.Instance.Subscribe(LaunchEventKey.SplashComplete, OnSplashComplete);
            CEventRouter.Instance.Subscribe(LaunchEventKey.StartFormBegin, OnStartFormBegin);
            CEventRouter.Instance.Subscribe(LaunchEventKey.GameLaunch, OnGameLaunch);
            CEventRouter.Instance.Subscribe<float>(LaunchEventKey.LoadingProgress, OnLoadingProgress);
            CEventRouter.Instance.Subscribe(LaunchEventKey.LoadingComplete, OnLoadingComplete);
        }

        protected override void OnDestroyInternal()
        {
            CEventRouter.Instance.Unsubscribe(LaunchEventKey.SplashBegin, OnSplashBegin);
            CEventRouter.Instance.Unsubscribe(LaunchEventKey.SplashComplete, OnSplashComplete);
            CEventRouter.Instance.Unsubscribe(LaunchEventKey.StartFormBegin, OnStartFormBegin);
            CEventRouter.Instance.Unsubscribe(LaunchEventKey.GameLaunch, OnGameLaunch);
            CEventRouter.Instance.Unsubscribe<float>(LaunchEventKey.LoadingProgress, OnLoadingProgress);
            CEventRouter.Instance.Unsubscribe(LaunchEventKey.LoadingComplete, OnLoadingComplete);
        }

        private void OnSplashBegin()
        {
            CloseExistingLogic(_splashFormLogicId);
            var logic = BusinessLogicManager.Instance.CreateLogic<SplashFormLogic>();
            _splashFormLogicId = logic.LogicId;
        }

        private void OnSplashComplete()
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.SplashForm_CloseRequest);
        }

        private void OnStartFormBegin()
        {
            CloseExistingLogic(_startFormLogicId);
            var logic = BusinessLogicManager.Instance.CreateLogic<StartFormLogic>();
            _startFormLogicId = logic.LogicId;
        }

        private void OnGameLaunch()
        {
            // 关闭主菜单
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.StartForm_CloseRequest);

            // 重新打开 SplashForm 作为加载画面
            CloseExistingLogic(_splashFormLogicId);
            var logic = BusinessLogicManager.Instance.CreateLogic<SplashFormLogic>();
            _splashFormLogicId = logic.LogicId;
        }

        private void OnLoadingProgress(float progress)
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.SplashForm_ProgressUpdate, progress);
        }

        private void OnLoadingComplete()
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow(UIEventKey.SplashForm_CloseRequest);
        }

        private static void CloseExistingLogic(int logicId)
        {
            if (logicId > 0 && BusinessLogicManager.Instance.HasLogic(logicId))
            {
                BusinessLogicManager.Instance.DestroyLogic(logicId);
            }
        }
    }
}
