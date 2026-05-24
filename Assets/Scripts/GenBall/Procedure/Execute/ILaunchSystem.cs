using GenBall.Procedure.Game;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public interface ILaunchSystem : ISystem
    {
        RunningMode Mode { get; }
        string StartSceneName { get; }
        float SceneLoadProgress { get; }
        bool IsSceneLoading { get; }
        void StartNewGame();
        void ContinueLastGame();
        void LoadGame(int saveIndex);
        void SkipSplash();
    }
}
