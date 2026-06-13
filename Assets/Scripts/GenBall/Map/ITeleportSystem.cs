using GenBall.Map;
using Yueyn.Main;

namespace GenBall.Map
{
    public interface ITeleportSystem : ISystem
    {
        bool IsTeleporting { get; }
        bool Teleport(TeleportRequestInfo request);
    }

    public struct TeleportRequestInfo
    {
        public string SceneName;
        public int SavePointIndex;
    }
}
