using GenBall.Map;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public interface ISceneLoadSystem : ISystem
    {
        float LoadProgress { get; }
        bool IsLoading { get; }
        string TargetSceneName { get; }
        SavePointModel TargetSavePoint { get; }
        void SetTargetSavePoint(SavePointModel savePoint);
        void AsyncLoadScene(string sceneName);
    }
}
