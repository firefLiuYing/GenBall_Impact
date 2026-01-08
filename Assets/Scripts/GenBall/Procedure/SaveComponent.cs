using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Utils.Singleton;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Utils;

namespace GenBall.Procedure
{
    public class SaveComponent : MonoBehaviour,IComponent
    {
        
        [SerializeField] private int maxSaveCount = 6;

        public int MaxSaveCount
        {
            get => maxSaveCount;
            set => maxSaveCount = value;
        }
        
        private bool _hasLoadCachedSaveSlot = false;

        private readonly Dictionary<int, SaveSlotData> _cachedSaveSlotMap = new();
        private readonly SaveSlotDataListJson _cachedSaveSlotDataJson = new()
        {
            slots = new(),
        };
        /// <summary>
        /// 获取存档位情况（有哪几个存档位有存档记录，每个存档的基本信息）
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取指定存档位的存档信息
        /// </summary>
        /// <param name="saveIndex"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 在指定存档位保存存档数据
        /// </summary>
        /// <param name="gameData"></param>
        /// <param name="saveIndex"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 在指定存档位创建新存档
        /// </summary>
        /// <returns></returns>
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
            return Path.Combine( Application.dataPath, "SaveGames");
            #else
            return UnityEngine.Application.persistentDataPath;
            #endif
        }

        
        private async Task<bool> InternalWriteSaveSlotInfo()
        {
            try
            {
                _cachedSaveSlotDataJson.slots.Clear();
                _cachedSaveSlotDataJson.slots.AddRange(_cachedSaveSlotMap.Values);
                string json = JsonUtility.ToJson(_cachedSaveSlotDataJson,true);
                Debug.Log($"gzp 当前存档有：{json}");
                if (!File.Exists(GetSaveSlotFilePath()))
                {
                    Debug.Log("gzp 检测到文件不存在，正在创建新的存档信息文件");
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
    
                Debug.Log($"gzp 要写入的存档基本信息为：{json}");
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
                    string json =await File.ReadAllTextAsync(GetSaveSlotFilePath());
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
                Debug.LogError($"读取存档信息失败：{e.Message}");
                return null;
            }
        }

        private void UpdateCachedSaveSlotInfo([NotNull] GameData gameData,int saveIndex)
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
                int saveIndex=0;
                foreach (var saveSlot in _cachedSaveSlotMap.Values)
                {
                    if (saveIndex <= saveSlot.saveIndex)
                    {
                        saveIndex = saveSlot.saveIndex+1;
                    }
                }
                Debug.Log($"gzp 创建新存档的id为：{saveIndex}");
                var filePath = GetSaveFilePath(saveIndex);
                GameData gameData = new GameData
                {
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    TotalTime = new DateTime(0)
                };
                var json= JsonUtility.ToJson(gameData,true);
                Debug.Log($"gzp 创建新的存档信息：{json}");
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                await InternalWriteSaveSlotInfo();
                return saveIndex;
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 创建存档失败：{e.Message}");
                return -1;
            }
        }

        private async Task<bool> InternalUpdateSaveFile([NotNull] GameData gameData, int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp 找不到saveIndex: {saveIndex} 对应的存档");
                }
                var filePath = GetSaveFilePath(saveIndex);
                if (!File.Exists(filePath))
                {
                    throw new Exception("gzp 要更新的存档不存在，请先创建存档");
                }
                var json = JsonUtility.ToJson(gameData,true);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                await InternalWriteSaveSlotInfo();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 更新存档失败：{e.Message}");
                return false;
            }
        }

        private async Task<GameData> InternalGetSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp 找不到saveIndex: {saveIndex} 对应的存档");
                }
                var filePath = GetSaveFilePath(saveIndex);
                var json = await File.ReadAllTextAsync(filePath);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 获取存档文件失败：{e.Message}");
                return null;
            }
        }

        private async Task<bool> InternalDeleteSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp 找不到saveIndex: {saveIndex} 对应的存档");
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
                Debug.LogError($"gzp 删除存档失败，saveIndex:{saveIndex} :{e.Message}");
                return false;
            }
        }
        #region 生命周期

        public void OnRegister()
        {
            
        }
        public void OnUnregister()
        {
            
        }

        public void ComponentUpdate(float elapsedSeconds, float realElapseSeconds)
        {
            
        }
        public void ComponentFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void Shutdown()
        {
            
        }
        #endregion
    }

    [Serializable]
    public class SaveSlotDataListJson
    {
        public List<SaveSlotData> slots;
    }
    
    [Serializable]
    public struct SaveSlotData
    {
        public int saveIndex;
        public bool isEmpty;
        [SerializeField] private long lastUpdateTime;
        [SerializeField] private long totalTime;
        [SerializeField] private long createTime;

        public DateTime LastUpdateTime
        {
            get => new DateTime(lastUpdateTime);
            set => lastUpdateTime = value.Ticks;
        }

        public DateTime CreateTime
        {
            get => new DateTime(createTime);
            set => createTime = value.Ticks;
        }

        public DateTime TotalTime
        {
            get => new DateTime(totalTime);
            set => totalTime = value.Ticks;
        }
    }
}