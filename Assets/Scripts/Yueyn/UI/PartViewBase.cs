namespace Yueyn.UI
{
    /// <summary>
    /// Part View 基类（非泛型，继承 UIComponent）
    /// 对应 BusinessPartLogic，用于 Form 内的子组件视图
    /// </summary>
    public abstract class PartViewBase : UIComponent
    {
    }

    /// <summary>
    /// Part View 基类（泛型，数据驱动）
    /// 对应 BusinessPartLogic，通过 ViewData 驱动刷新
    /// </summary>
    /// <typeparam name="TViewData">View 数据类型</typeparam>
    public abstract class PartViewBase<TViewData> : PartViewBase where TViewData : new()
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

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            ViewData = new TViewData();
        }
    }
}
