using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GenBall.Utils.Singleton;

namespace GenBall.Map
{
    public class SceneSystem:ISingleton
    {
        public static SceneSystem Instance => SingletonManager.GetSingleton<SceneSystem>();

        private readonly Dictionary<string, SceneStateObj> _sceneStateObjs = new();
        private readonly Dictionary<string, SceneConfigDictionary> _mapConfig = new();

        private bool _mapConfigInitialized=false;
        private bool _sceneStateInitialized=false;
        public void InitializeMapConfig(MapModel mapModel)
        {
            if(_mapConfigInitialized) return;
            _mapConfig.Clear();
            foreach (var sceneModel in mapModel.scenes)
            {
                var config = new SceneConfigDictionary
                {
                    SavePointConfigs = new(),
                    EnemyUnitConfigs = new()
                };
                foreach (var saveModel in sceneModel.savePoints)
                {
                    config.SavePointConfigs.Add(saveModel.id,saveModel);
                }

                foreach (var unitModel in sceneModel.enemyUnits)
                {
                    config.EnemyUnitConfigs.Add(unitModel.id,unitModel);
                }
                
                _mapConfig.Add(sceneModel.sceneName, config);
            }
            _mapConfigInitialized = true;
        }

        public void InitializeSceneStateObjs(MapSaveData mapSaveData)
        {
            if(_sceneStateInitialized) return;
            foreach (var sceneSaveData in mapSaveData.unlockedScenes)
            {
                _sceneStateObjs.TryAdd(sceneSaveData.sceneName, new SceneStateObj()
                {
                    UnlockedSavePoints = sceneSaveData.unlockedSavePoints.ToHashSet(),
                    KilledEnemyUnits = sceneSaveData.killedEnemyUnits.ToHashSet()
                });
            }
            _sceneStateInitialized = true;
        }

        public void UnlockSavePoint(string sceneName, int savePointIndex)
        {
            if (_sceneStateObjs.TryGetValue(sceneName, out var sceneStateObj))
            {
                sceneStateObj.UnlockedSavePoints.Add(savePointIndex);
            }
            else
            {
                _sceneStateObjs.Add(sceneName,new SceneStateObj()
                {
                    UnlockedSavePoints = new HashSet<int>(){savePointIndex},
                    KilledEnemyUnits = new()
                });
            }
        }

        public void KillEnemyUnit(string sceneName, int enemyUnitIndex)
        {
            if (_sceneStateObjs.TryGetValue(sceneName, out var sceneStateObj))
            {
                sceneStateObj.KilledEnemyUnits.Add(enemyUnitIndex);
            }
            else
            {
                _sceneStateObjs.Add(sceneName, new SceneStateObj()
                {
                    UnlockedSavePoints = new HashSet<int>(),
                    KilledEnemyUnits = new HashSet<int>(){enemyUnitIndex},
                });
            }
        }

        private IEnumerable<int> GetUnlockedSavePoints(string sceneName)
        {
            return _sceneStateObjs.TryGetValue(sceneName, out var sceneStateObj) ? sceneStateObj.UnlockedSavePoints : Enumerable.Empty<int>();
        }

        public IEnumerable<SavePointModel> GetUnlockedSavePointModels(string sceneName)
        {
            var unlockedSavePointIds=GetUnlockedSavePoints(sceneName);
            return _mapConfig.TryGetValue(sceneName, out var sceneConfigDictionary) ? unlockedSavePointIds.Select(id => sceneConfigDictionary.SavePointConfigs[id]).Where(m=>m!=null) : Enumerable.Empty<SavePointModel>();
        }

        public SavePointModel GetSavePointModel(string sceneName, int unitIndex)
        {
            return _mapConfig.TryGetValue(sceneName, out var sceneConfigDictionary) ? sceneConfigDictionary.SavePointConfigs.GetValueOrDefault(unitIndex) : null;
        }

        private bool IsEnemyUnitKilled(string sceneName, int unitIndex)
        {
            return _sceneStateObjs.TryGetValue(sceneName, out var sceneStateObj) && sceneStateObj.KilledEnemyUnits.Contains(unitIndex);
        }

        public EnemyUnitModel GetEnemyModel(string sceneName, int unitIndex)
        {
            return _mapConfig.TryGetValue(sceneName, out var sceneConfigDictionary) ? sceneConfigDictionary.EnemyUnitConfigs.GetValueOrDefault(unitIndex) : null;
        }

        public IEnumerable<EnemyUnitModel> GetAllUnKilledEnemyModel(string sceneName)
        {
            return _mapConfig.TryGetValue(sceneName, out var sceneConfigDictionary) ? sceneConfigDictionary.EnemyUnitConfigs.Where(e=>!IsEnemyUnitKilled(sceneName, e.Key)).Select(e=>e.Value) : Enumerable.Empty<EnemyUnitModel>();
        }
        
        private class SceneStateObj
        {
            public HashSet<int> UnlockedSavePoints;
            public HashSet<int> KilledEnemyUnits;
        }

        private class SceneConfigDictionary
        {
            public Dictionary<int,SavePointModel> SavePointConfigs;
            public Dictionary<int, EnemyUnitModel> EnemyUnitConfigs;
        }
    }
}