namespace Yueyn.UI
{
    /// <summary>
    /// UI 业务页面基类（非泛型版本）
    /// 继承自 UIComponent，可以放在 UIFormScript 上
    /// </summary>
    public abstract class UIBusinessFormBase : UIComponent
    {
        // 基础 View 层，不包含数据绑定
    }

    /// <summary>
    /// UI 业务页面基类（泛型版本，支持数据驱动）
    /// </summary>
    /// <typeparam name="TViewData">View 数据类型</typeparam>
    public abstract class UIBusinessFormBase<TViewData> : UIBusinessFormBase where TViewData : new()
    {
        /// <summary>
        /// View 数据
        /// </summary>
        public TViewData ViewData { get; private set; }

        /// <summary>
        /// 设置 View 数据并刷新
        /// </summary>
        public void SetViewData(TViewData data)
        {
            ViewData = data;
            RefreshView();
        }

        /// <summary>
        /// 刷新 View（供子类重写）
        /// </summary>
        protected virtual void RefreshView() { }

        /// <summary>
        /// 初始化时创建默认数据
        /// </summary>
        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            ViewData = new TViewData();
        }
    }
}
