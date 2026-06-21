namespace GenBall.Interact
{
    public interface IInteractable
    {
        /// <summary>
        /// UI 交互按钮显示的名称，比如，开门的就是"打开"
        /// </summary>
        string OperationDescription { get; }

        /// <summary>
        /// 是否可以交互。返回 false 时不会出现在候选列表中。
        /// 由各实现类自行检查条件（如战斗状态等）。
        /// </summary>
        bool CanInteract { get; }

        void Interact();

        /// <summary>
        /// 被选中时调用（预留高亮接口，当前仅输出日志）。
        /// </summary>
        void OnFocused();

        /// <summary>
        /// 取消选中时调用。
        /// </summary>
        void OnUnfocused();
    }
}
