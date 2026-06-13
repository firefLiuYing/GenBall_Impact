using GenBall.Event;
using Yueyn.Event;
using Yueyn.UI;

namespace GenBall.UI
{
    /// <summary>
    /// 常驻业务逻辑，将启动流程全局事件映射为 UI 表单操作。
    /// LoadingForm 身兼二职：启动加载画面 + 场景加载画面。
    /// </summary>
    public class LoadingBusinessLogic : BusinessLogicBase
    {
        private int _loadingFormLogicId = -1;
        private int _startFormLogicId = -1;
        private bool _inGameUICreated;

        protected override void OnCreateInternal()
        {
            CEventRouter.Instance.Subscribe((int)GlobalEventId.StartupLoadingBegin, OnStartupLoadingBegin);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.StartupLoadingComplete, OnStartupLoadingComplete);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.StartFormBegin, OnStartFormBegin);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.GameLaunch, OnGameLaunch);
            CEventRouter.Instance.Subscribe<float>((int)GlobalEventId.LoadingProgress, OnLoadingProgress);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);
        }

        protected override void OnDestroyInternal()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.StartupLoadingBegin, OnStartupLoadingBegin);
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.StartupLoadingComplete, OnStartupLoadingComplete);
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.StartFormBegin, OnStartFormBegin);
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.GameLaunch, OnGameLaunch);
            CEventRouter.Instance.Unsubscribe<float>((int)GlobalEventId.LoadingProgress, OnLoadingProgress);
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.LoadingComplete, OnLoadingComplete);
        }

        private void OnStartupLoadingBegin()
        {
            CloseExistingLogic(_loadingFormLogicId);
            var logic = BusinessLogicManager.Instance.CreateLogic<LoadingFormLogic>();
            _loadingFormLogicId = logic.LogicId;
        }

        private void OnStartupLoadingComplete()
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.LoadingForm_CloseRequest);
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
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.StartForm_CloseRequest);

            // 重新打开 LoadingForm 作为加载画面
            CloseExistingLogic(_loadingFormLogicId);
            var logic = BusinessLogicManager.Instance.CreateLogic<LoadingFormLogic>();
            _loadingFormLogicId = logic.LogicId;
        }

        private void OnLoadingProgress(float progress)
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.LoadingForm_ProgressUpdate, progress);
        }

        private void OnLoadingComplete()
        {
            Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.LoadingForm_CloseRequest);

            // Create InGameUIBusinessLogic on first scene load so it can receive SceneReady.
            // Teleport re-triggers LoadingComplete, but InGameUIBusinessLogic persists
            // (created under FrameworkBase DontDestroyOnLoad), so guard against duplicates.
            if (!_inGameUICreated)
            {
                BusinessLogicManager.Instance.CreateLogic<InGameUIBusinessLogic>();
                _inGameUICreated = true;
            }
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
