namespace GenBall.BattleSystem.Command
{
    public interface IWheel
    {
        void Execute(WheelCommand cmd);
        bool IsWheeling { get; }
    }
}
