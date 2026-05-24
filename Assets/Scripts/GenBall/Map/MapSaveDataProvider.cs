using GenBall.Procedure;
using UnityEngine;

namespace GenBall.Map
{
    public class MapSaveDataProvider : ISaveDataProvider
    {
        public string DataKey => "Map";

        private MapSaveData _runtimeData = new()
        {
            unlockedScenes = new(),
        };

        public MapSaveData RuntimeData => _runtimeData;

        public string CollectSaveData()
        {
            return JsonUtility.ToJson(_runtimeData);
        }

        public void ApplySaveData(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                _runtimeData = JsonUtility.FromJson<MapSaveData>(json) ?? new MapSaveData();
            }
        }
    }
}
