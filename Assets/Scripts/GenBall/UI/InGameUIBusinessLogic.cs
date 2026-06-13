using GenBall.Event;
using Yueyn.Event;
using Yueyn.UI;

namespace GenBall.UI
{
    /// <summary>
    /// 局内常驻 UI 业务逻辑
    /// 由编排层在首次场景初始化前创建，监听全局事件管理局内 UI 开闭。
    /// </summary>
    public class InGameUIBusinessLogic : BusinessLogicBase
    {
        private bool _mainHudOpened;

        protected override void OnCreateInternal()
        {
            CEventRouter.Instance.Subscribe((int)GlobalEventId.WheelOpened, OnWheelOpened);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.InGameUIReady, OnInGameUIReady);
        }

        protected override void OnDestroyInternal()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.WheelOpened, OnWheelOpened);
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.InGameUIReady, OnInGameUIReady);
            _mainHudOpened = false;
            base.OnDestroyInternal();
        }

        private void OnWheelOpened()
        {
            AbilityWheelFormLogic.Open();
        }

        private void OnInGameUIReady()
        {
            if (!_mainHudOpened)
            {
                MainHudFormLogic.Open();
                _mainHudOpened = true;
            }
        }
    }
}
