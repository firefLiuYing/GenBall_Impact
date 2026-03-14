namespace GenBall.Interact
{
    public interface IInteractable
    {
        /// <summary>
        /// 在UI上面显示的按钮名称，例如，宝箱的就是“打开”
        /// </summary>
        public string OperationDescription { get; }
        public void Interact();
    }
}