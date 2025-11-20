namespace GenBall.BattleSystem
{
    public interface IInteractable
    {
        /// <summary>
        /// 对可交互物体发出一个交互
        /// </summary>
        /// <param name="stimulus">交互信号</param>
        /// <param name="responses">反馈信号</param>
        public void Handle(IInteractToken stimulus,out IInteractToken[] responses);
    }
}