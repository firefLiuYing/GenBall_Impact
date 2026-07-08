namespace Yueyn.UI
{
    /// <summary>
    /// Cell 生命周期契约。任何想要被 CellViewList 管理的 MonoBehaviour 需实现此接口。
    /// </summary>
    public interface ICellView
    {
        /// <summary>Cell 实例化后调用一次（在 OnRefresh 之前）</summary>
        void OnCreate();

        /// <summary>绑定数据时调用。index 从 0 开始</summary>
        void OnRefresh(int index, object data);

        /// <summary>Cell 被移除前调用。清理资源，不应销毁 GameObject（由 CellViewList 负责）</summary>
        void OnRemove();
    }
}
