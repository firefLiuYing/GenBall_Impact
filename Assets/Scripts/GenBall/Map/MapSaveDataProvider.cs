using System.Collections.Generic;
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

        public void MergeSaveFields(Dictionary<string, string> fields)
        {
            if (fields.TryGetValue(SaveFieldKeys.Map.UnlockedScenes, out var scenesJson))
            {
                var wrapper = JsonUtility.FromJson<SceneSaveDataListWrapper>(scenesJson);
                if (wrapper != null && wrapper.scenes != null)
                {
                    _runtimeData.unlockedScenes = wrapper.scenes;
                }
            }
        }
    }

    [System.Serializable]
    internal class SceneSaveDataListWrapper
    {
        public List<SceneSaveData> scenes;
    }
}
