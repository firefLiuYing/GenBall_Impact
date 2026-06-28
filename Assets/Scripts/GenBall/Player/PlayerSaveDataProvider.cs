using System.Collections.Generic;
using GenBall.Procedure;
using UnityEngine;

namespace GenBall.Player
{
    public class PlayerSaveDataProvider : ISaveDataProvider
    {
        public string DataKey => "Player";

        private PlayerSaveData _runtimeData = new()
        {
            lastSavePointIndex = 0,
            lastSceneName = "",
        };

        public PlayerSaveData RuntimeData => _runtimeData;

        public string CollectSaveData()
        {
            return JsonUtility.ToJson(_runtimeData);
        }

        public void ApplySaveData(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                _runtimeData = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
            }
        }

        public void MergeSaveFields(Dictionary<string, string> fields)
        {
            if (fields.TryGetValue(SaveFieldKeys.Player.LastSavePointIndex, out var idxStr)
                && int.TryParse(idxStr, out var idx))
            {
                _runtimeData.lastSavePointIndex = idx;
            }
            if (fields.TryGetValue(SaveFieldKeys.Player.LastSceneName, out var scene))
            {
                _runtimeData.lastSceneName = scene;
            }
        }
    }
}
