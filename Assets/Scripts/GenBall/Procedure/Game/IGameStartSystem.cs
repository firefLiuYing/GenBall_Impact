using System.Threading.Tasks;
using GenBall.Procedure;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public enum GameStartType
    {
        NewGame,
        Continue,
        LoadGame,
    }

    public class GameStartRequest
    {
        public GameStartType Type;
        public int SaveIndex;
    }

    public class GameStartContext
    {
        public string TargetSceneName;
        public int TargetSavePointIndex;
        public GameData GameData;
        public bool IsNewGame;
    }

    public interface IGameStartSystem : ISystem
    {
        Task<GameStartContext> PrepareStartAsync(GameStartRequest request);
    }
}
