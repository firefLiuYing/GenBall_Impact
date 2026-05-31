using GenBall.Event;
using UnityEngine;
using Yueyn.Event;
using Yueyn.UI;

namespace GenBall.UI
{
    /// <summary>
    /// 局内常驻 UI 业务逻辑
    /// 监听全局输入/游戏事件，管理局内 UI 表单的开闭
    /// </summary>
    public class InGameUIBusinessLogic : BusinessLogicBase
    {
        // private AbilityWheelFormLogic _wheelFormLogic;

        protected override void OnCreateInternal()
        {
            CEventRouter.Instance.Subscribe((int)GlobalEventId.WheelOpened, OnWheelOpened);
        }

        protected override void OnDestroyInternal()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.WheelOpened, OnWheelOpened);
            base.OnDestroyInternal();
        }

        private void OnWheelOpened()
        {
            // Debug.Log("OnWheelOpened");
            AbilityWheelFormLogic.Open();
        }
    }
}
