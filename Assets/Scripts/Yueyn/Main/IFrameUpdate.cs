namespace Yueyn.Main
{
    public interface IFrameUpdate
    {
        void FrameUpdate(float deltaTime);
        SystemScope FrameUpdateScope { get; }
    }
}