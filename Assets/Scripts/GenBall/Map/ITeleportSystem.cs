using Yueyn.Main;

namespace GenBall.Map
{
    public interface ITeleportSystem : ISystem
    {
        bool IsTeleporting { get; set; }
        SavePointModel CachedSavePointModel { get; }
        bool Teleport(TeleportRequestInfo teleportRequestInfo);
    }
}
