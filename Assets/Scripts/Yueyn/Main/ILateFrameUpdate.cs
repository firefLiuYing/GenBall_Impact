namespace Yueyn.Main
{
    public interface ILateFrameUpdate
    {
        void LateFrameUpdate(float deltaTime);
        SystemScope LateFrameUpdateScope { get; }
    }
}
