using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GenBall.Framework.Config;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure
{
    /// <summary>
    /// 存档系统，纯 C# 实现，不依赖 MonoBehaviour
    /// 配置从 IConfigProvider 读取
    /// </summary>
    public class SaveSystem : ISaveService
    {
        private int _maxSaveCount = 6;
        private bool _hasLoadCachedSaveSlot = false;
        private readonly Dictionary<int, SaveSlotData> _cachedSaveSlotMap = new();
        private readonly SaveSlotDataListJson _cachedSaveSlotDataJson = new()
        {
            slots = new(),
        };

        public int MaxSaveCount => _maxSaveCount;

        public void Init()
        {
            var rep = SystemRepository.Instance;
            if (rep.HasSystem<IConfigProvider>())
            {
                var config = rep.GetSystem<IConfigProvider>().GetConfig<AppSettingsConfig>();
                if (config != null)
                {
                    _maxSaveCount = config.maxSaveCount;
                }
            }
        }

        public void UnInit()
        {
            _cachedSaveSlotMap.Clear();
        }

        public async Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas()
        {
            try
            {
                return await InternalGetSaveSlotInfo();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public async Task<GameData> LoadGameData(int saveIndex)
        {
            try
            {
                return await InternalGetSaveFile(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public async Task<bool> SaveGameData(GameData gameData, int saveIndex)
        {
            try
            {
                return await InternalUpdateSaveFile(gameData, saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public async Task<int> CreateNewSave()
        {
            try
            {
                return await InternalCreateSaveFile();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return -1;
            }
        }

        public async Task<bool> DeleteSave(int saveIndex)
        {
            try
            {
                return await InternalDeleteSaveFile(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        private static string GetSaveSlotFilePath()
        {
            return $"{GetSaveRootPath()}//save_slot_info.json";
        }

        private static string GetSaveFilePath(int saveIndex)
        {
            return $"{GetSaveRootPath()}//savedata_{saveIndex}.json";
        }

        private static string GetSaveRootPath()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "SaveGames");
#else
            return Application.persistentDataPath;
#endif
        }

        private async Task<bool> InternalWriteSaveSlotInfo()
        {
            try
            {
                _cachedSaveSlotDataJson.slots.Clear();
                _cachedSaveSlotDataJson.slots.AddRange(_cachedSaveSlotMap.Values);
                string json = JsonUtility.ToJson(_cachedSaveSlotDataJson, true);
                if (!File.Exists(GetSaveSlotFilePath()))
                {
                    var directory = Path.GetDirectoryName(GetSaveSlotFilePath());
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    await using FileStream fs = File.Create(GetSaveSlotFilePath());
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                }
                else
                {
                    await File.WriteAllTextAsync(GetSaveSlotFilePath(), json);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入存档信息失败：{e.Message}");
                return false;
            }
        }

        private async Task<IEnumerable<SaveSlotData>> InternalGetSaveSlotInfo()
        {
            try
            {
                if (_hasLoadCachedSaveSlot)
                {
                    return _cachedSaveSlotMap.Values;
                }

                if (File.Exists(GetSaveSlotFilePath()))
                {
                    string json = await File.ReadAllTextAsync(GetSaveSlotFilePath());
                    var saveSlotInfo = JsonUtility.FromJson<SaveSlotDataListJson>(json);
                    _cachedSaveSlotMap.Clear();
                    foreach (var item in saveSlotInfo.slots)
                    {
                        _cachedSaveSlotMap.TryAdd(item.saveIndex, item);
                    }
                    _hasLoadCachedSaveSlot = true;
                    return saveSlotInfo.slots;
                }
                await InternalWriteSaveSlotInfo();
                _hasLoadCachedSaveSlot = true;
                return _cachedSaveSlotMap.Values;
            }
            catch (Exception e)
            {
                Debug.LogError($"获取存档信息失败：{e.Message}");
                return null;
            }
        }

        private void UpdateCachedSaveSlotInfo(GameData gameData, int saveIndex)
        {
            var slot = new SaveSlotData
            {
                saveIndex = saveIndex,
                isEmpty = false,
                CreateTime = gameData.CreateTime,
                LastUpdateTime = gameData.LastUpdateTime,
                TotalTime = gameData.TotalTime
            };
            _cachedSaveSlotMap[saveIndex] = slot;
        }

        private async Task<int> InternalCreateSaveFile()
        {
            try
            {
                int saveIndex = 0;
                foreach (var saveSlot in _cachedSaveSlotMap.Values)
                {
                    if (saveIndex <= saveSlot.saveIndex)
                    {
                        saveIndex = saveSlot.saveIndex + 1;
                    }
                }

                var filePath = GetSaveFilePath(saveIndex);
                GameData gameData = new GameData();
                var json = JsonUtility.ToJson(gameData, true);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData, saveIndex);
                await InternalWriteSaveSlotInfo();
                return saveIndex;
            }
            catch (Exception e)
            {
                Debug.LogError($"创建存档失败：{e.Message}");
                return -1;
            }
        }

        private async Task<bool> InternalUpdateSaveFile(GameData gameData, int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"找不到 saveIndex: {saveIndex} 对应的存档");
                }
                var filePath = GetSaveFilePath(saveIndex);
                if (!File.Exists(filePath))
                {
                    throw new Exception("要更新的存档不存在，请先创建存档");
                }
                var json = JsonUtility.ToJson(gameData, true);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData, saveIndex);
                await InternalWriteSaveSlotInfo();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"更新存档失败：{e.Message}");
                return false;
            }
        }

        private async Task<GameData> InternalGetSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"找不到 saveIndex: {saveIndex} 对应的存档");
                }
                var filePath = GetSaveFilePath(saveIndex);
                var json = await File.ReadAllTextAsync(filePath);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"读取存档文件失败：{e.Message}");
                return null;
            }
        }

        private async Task<bool> InternalDeleteSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"找不到 saveIndex: {saveIndex} 对应的存档");
                }

                var filePath = GetSaveFilePath(saveIndex);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _cachedSaveSlotMap.Remove(saveIndex);
                await InternalWriteSaveSlotInfo();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"删除存档失败 saveIndex:{saveIndex} :{e.Message}");
                return false;
            }
        }
    }
}
