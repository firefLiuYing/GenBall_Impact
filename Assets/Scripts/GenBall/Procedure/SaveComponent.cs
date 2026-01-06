using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GenBall.Utils.Singleton;
using UnityEditor.VersionControl;
using UnityEngine;
using Yueyn.Main;
using Yueyn.Utils;

namespace GenBall.Procedure
{
    public class SaveComponent : MonoBehaviour,IComponent
    {
        /// <summary>
        /// todo gzp 存档组件思路整理，事后记得删除
        /// 需要提供的接口有：
        /// 查看当前存档位情况：当前有哪几个存档位有存入数据，以及他们的基本信息（需要在读取界面显示的信息，例如最近一次游玩日期）
        /// 创建一个指定id的存档，不能超过最大存档数量
        /// 读取一个指定id的存档，
        /// 更新一个指定id的存档，
        /// 为了实现上述功能，需要引入一个单独的文件用来存放存档的信息
        /// 这个东西会在创建存档，更新存档时更新，会在游戏打开时就进行一次读取，其他时候都是写入，如果是第一次打开，那还要进行一次创建操作
        /// </summary>
        
        [SerializeField] private int maxSaveCount = 6;

        public int MaxSaveCount
        {
            get => maxSaveCount;
            set => maxSaveCount = value;
        }
        
        private bool _hasLoadCachedSaveSlot = false;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();

        /// <summary>
        /// 获取存档位情况（有哪几个存档位有存档记录，每个存档的基本信息）
        /// </summary>
        /// <returns></returns>
        public async Task<SaveSlotInfo> GetSaveSlotInfo()
        {
            try
            {
                SaveSlotInfo saveSlotInfo;
                if (_hasLoadCachedSaveSlot)
                {
                    saveSlotInfo = new();
                    saveSlotInfo.slots.AddRange(_cachedSaveSlotData);
                }
                else
                {
                    saveSlotInfo = await InternalGetSaveSlotInfo();
                }
                return saveSlotInfo;
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
        /// <param name="saveIndex"></param>
        /// <returns></returns>
        public async Task<bool> CreateNewSave(int saveIndex)
        {
            try
            {
                return await InternalCreateSaveFile(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
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
                SaveSlotInfo saveSlotInfo = new ();
                saveSlotInfo.slots.AddRange(_cachedSaveSlotData);
                string json = JsonUtility.ToJson(saveSlotInfo,true);
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

        private async Task<SaveSlotInfo> InternalGetSaveSlotInfo()
        {
            try
            {
                SaveSlotInfo saveSlotInfo;
                if (!File.Exists(GetSaveSlotFilePath()))
                {
                    saveSlotInfo = new SaveSlotInfo
                    {
                        slots = new List<SaveSlotData>()
                    };
                    for (int i = 0; i < MaxSaveCount; i++)
                    {
                        saveSlotInfo.slots.Add(new SaveSlotData()
                        {
                            saveIndex = i,
                            isEmpty = true,
                        });
                    }
                }
                else
                {
                    string json =await File.ReadAllTextAsync(GetSaveSlotFilePath());
                    saveSlotInfo = JsonUtility.FromJson<SaveSlotInfo>(json);
                    for (int i = saveSlotInfo.slots.Count; i < MaxSaveCount; i++)
                    {
                        saveSlotInfo.slots.Add(new SaveSlotData()
                        {
                            saveIndex = i,
                            isEmpty = true,
                        });
                    }
                }
                _cachedSaveSlotData.Clear();
                _cachedSaveSlotData.AddRange(saveSlotInfo.slots);
                _hasLoadCachedSaveSlot = true;
                return saveSlotInfo;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取存档信息失败：{e.Message}");
                return null;
            }
        }

        private void UpdateCachedSaveSlotInfo(GameData gameData,int saveIndex)
        {
            if (saveIndex >= MaxSaveCount || saveIndex < 0)
            {
                throw new Exception($"gzp saveIndex不合法：saveIndex: {saveIndex}");
            }

            if (gameData == null)
            {
                _cachedSaveSlotData[saveIndex].isEmpty=true;
                return;
            }
            _cachedSaveSlotData[saveIndex].isEmpty=false;
            _cachedSaveSlotData[saveIndex].CreateTime = gameData.CreateTime;
            _cachedSaveSlotData[saveIndex].LastUpdateTime = gameData.LastUpdateTime;
            _cachedSaveSlotData[saveIndex].TotalTime = gameData.TotalTime;
        }

        private async Task<bool> InternalCreateSaveFile(int saveIndex)
        {
            try
            {
                if (saveIndex >= MaxSaveCount || saveIndex < 0)
                {
                    throw new Exception($"gzp saveIndex不合法：{saveIndex}");
                }

                var filePath = GetSaveFilePath(saveIndex);
                GameData gameData = new GameData
                {
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    TotalTime = new DateTime(0)
                };
                var json= JsonUtility.ToJson(gameData,true);
                Debug.Log(json);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                return await InternalWriteSaveSlotInfo();
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 创建存档失败：{e.Message}");
                return false;
            }
        }

        private async Task<bool> InternalUpdateSaveFile(GameData gameData, int saveIndex)
        {
            try
            {
                if (saveIndex >= MaxSaveCount || saveIndex < 0)
                {
                    throw new Exception($"gzp saveIndex:{saveIndex} 不合法");
                }
                var filePath = GetSaveFilePath(saveIndex);
                if (!File.Exists(filePath))
                {
                    throw new Exception("gzp 要更新的存档不存在，请先创建存档");
                }
                var json = JsonUtility.ToJson(gameData,true);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                return await InternalWriteSaveSlotInfo();
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
                if (saveIndex >= MaxSaveCount || saveIndex < 0)
                {
                    throw new Exception($"gzp saveIndex:{saveIndex} 不合法");
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
                if (saveIndex >= MaxSaveCount || saveIndex < 0)
                {
                    throw new Exception($"gzp saveIndex:{saveIndex} 不合法");
                }

                if (_cachedSaveSlotData[saveIndex].isEmpty)
                {
                    throw new Exception($"gzp 指定存档位无存档 saveIndex:{saveIndex}");
                }
                var filePath = GetSaveFilePath(saveIndex);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _cachedSaveSlotData[saveIndex].isEmpty = false;
                return await InternalWriteSaveSlotInfo();
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp 删除存档失败，saveIndex:{saveIndex} :{e.Message}");
                return false;
            }
        }
        #region 生命周期

        public async void OnRegister()
        {
            try
            {
                await InternalGetSaveSlotInfo();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
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
    public class SaveSlotInfo
    {
        public List<SaveSlotData> slots=new();
    }
    
    [Serializable]
    public class SaveSlotData
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