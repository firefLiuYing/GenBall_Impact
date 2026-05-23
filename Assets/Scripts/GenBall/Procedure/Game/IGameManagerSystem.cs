using System.Threading.Tasks;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public interface IGameManagerSystem : ISystem
    {
        GameData GameData { get; set; }
        RunningMode Mode { get; set; }
        int CurSaveIndex { get; set; }
        Task<bool> SaveGame();
    }
}
