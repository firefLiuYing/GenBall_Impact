namespace Yueyn.UI
{
    /// <summary>
    /// PartLogic 可选实现此接口以接收 Cell 数据。
    /// PartLogicCellAdapter.OnRefresh 检测此接口并转发。
    /// </summary>
    public interface ICellViewDataReceiver
    {
        void ReceiveData(int index, object data);
    }
}
