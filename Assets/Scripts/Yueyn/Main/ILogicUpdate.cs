namespace Yueyn.Main
{
    public interface ILogicUpdate
    {
        void LogicUpdate(float deltaTime);
        SystemScope LogicUpdateScope { get; }
    }
}