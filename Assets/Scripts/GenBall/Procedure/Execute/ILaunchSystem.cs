using GenBall.Procedure.Game;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public interface ILaunchSystem : ISystem
    {
        RunningMode Mode { get; }
        string StartSceneName { get; }
        void SkipStartupLoading();
        void StartGameWithContext(GameStartContext context);
    }
}
