using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Procedure;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public interface IGameManagerSystem : ISystem
    {
        GameData GameData { get; set; }
        RunningMode Mode { get; set; }
        int CurSaveIndex { get; set; }
        Task<bool> SaveGame();

        void RegisterSaveDataProvider(ISaveDataProvider provider);
        void UnregisterSaveDataProvider(ISaveDataProvider provider);
        ISaveDataProvider GetProvider(string key);
        Task<bool> LoadGameData(int saveIndex);
        Task<bool> UpdateSaveFields(string providerKey, Dictionary<string, string> fields);
    }
}
