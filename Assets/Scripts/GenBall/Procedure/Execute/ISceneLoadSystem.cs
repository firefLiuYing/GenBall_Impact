using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public enum SceneLoadMode
    {
        Single,
        Additive,
    }

    public interface ISceneLoadSystem : ISystem
    {
        float LoadProgress { get; }
        bool IsLoading { get; }
        string TargetSceneName { get; }
        void LoadScene(string sceneName, SceneLoadMode mode = SceneLoadMode.Single);
    }
}
