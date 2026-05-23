using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public interface IPauseSystem : ISystem
    {
        bool IsPaused { get; }
        void SetPause(bool paused);
    }
}
