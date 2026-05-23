using System.Collections.Generic;
using Yueyn.Main;

namespace GenBall.Map
{
    public interface ISceneStateSystem : ISystem
    {
        void InitializeMapConfig(MapModel mapModel);
        void InitializeSceneStateObjs(MapSaveData mapSaveData);
        void UnlockSavePoint(string sceneName, int savePointIndex);
        void KillEnemyUnit(string sceneName, int enemyUnitIndex);
        IEnumerable<SavePointModel> GetUnlockedSavePointModels(string sceneName);
        SavePointModel GetSavePointModel(string sceneName, int unitIndex);
        EnemyUnitModel GetEnemyModel(string sceneName, int unitIndex);
        IEnumerable<EnemyUnitModel> GetAllUnKilledEnemyModel(string sceneName);
    }
}
