namespace Yueyn.UI
{
    /// <summary>
    /// 子页面业务逻辑基类
    /// 不绑定页面，由 BusinessFormLogic 或其他 BusinessPartLogic 管理
    /// </summary>
    public abstract class BusinessPartLogic : BusinessPartLogicContainer
    {
        // 子页面逻辑不绑定 Form，只负责业务逻辑
    }
}
