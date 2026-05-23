using System.Collections.Generic;
using System.Threading.Tasks;
using Yueyn.Main;

namespace GenBall.Procedure
{
    /// <summary>
    /// 存档服务接口
    /// </summary>
    public interface ISaveService : ISystem
    {
        int MaxSaveCount { get; }
        Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas();
        Task<GameData> LoadGameData(int saveIndex);
        Task<bool> SaveGameData(GameData gameData, int saveIndex);
        Task<int> CreateNewSave();
        Task<bool> DeleteSave(int saveIndex);
    }
}
