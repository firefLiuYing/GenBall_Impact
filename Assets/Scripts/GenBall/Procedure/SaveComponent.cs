using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure
{
    public class SaveComponent : MonoBehaviour,IComponent
    {
        public int Priority => 500;
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
        /// ��ȡ�浵λ��������ļ����浵λ�д浵��¼��ÿ���浵�Ļ�����Ϣ��
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
        /// ��ȡָ���浵λ�Ĵ浵��Ϣ
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
        /// ��ָ���浵λ����浵����
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
        /// ��ָ���浵λ�����´浵
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
                Debug.Log($"gzp ��ǰ�浵�У�{json}");
                if (!File.Exists(GetSaveSlotFilePath()))
                {
                    Debug.Log("gzp ��⵽�ļ������ڣ����ڴ����µĴ浵��Ϣ�ļ�");
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
    
                Debug.Log($"gzp Ҫд��Ĵ浵������ϢΪ��{json}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"д��浵��Ϣʧ�ܣ�{e.Message}");
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
                Debug.LogError($"��ȡ�浵��Ϣʧ�ܣ�{e.Message}");
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
                Debug.Log($"gzp �����´浵��idΪ��{saveIndex}");
                var filePath = GetSaveFilePath(saveIndex);
                GameData gameData = new GameData();
                var json= JsonUtility.ToJson(gameData,true);
                Debug.Log($"gzp �����µĴ浵��Ϣ��{json}");
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                await InternalWriteSaveSlotInfo();
                return saveIndex;
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp �����浵ʧ�ܣ�{e.Message}");
                return -1;
            }
        }

        private async Task<bool> InternalUpdateSaveFile([NotNull] GameData gameData, int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp �Ҳ���saveIndex: {saveIndex} ��Ӧ�Ĵ浵");
                }
                var filePath = GetSaveFilePath(saveIndex);
                if (!File.Exists(filePath))
                {
                    throw new Exception("gzp Ҫ���µĴ浵�����ڣ����ȴ����浵");
                }
                var json = JsonUtility.ToJson(gameData,true);
                await File.WriteAllTextAsync(filePath, json);
                UpdateCachedSaveSlotInfo(gameData,saveIndex);
                await InternalWriteSaveSlotInfo();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp ���´浵ʧ�ܣ�{e.Message}");
                return false;
            }
        }

        private async Task<GameData> InternalGetSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp �Ҳ���saveIndex: {saveIndex} ��Ӧ�Ĵ浵");
                }
                var filePath = GetSaveFilePath(saveIndex);
                var json = await File.ReadAllTextAsync(filePath);
                return JsonUtility.FromJson<GameData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"gzp ��ȡ�浵�ļ�ʧ�ܣ�{e.Message}");
                return null;
            }
        }

        private async Task<bool> InternalDeleteSaveFile(int saveIndex)
        {
            try
            {
                if (!_cachedSaveSlotMap.ContainsKey(saveIndex))
                {
                    throw new Exception($"gzp �Ҳ���saveIndex: {saveIndex} ��Ӧ�Ĵ浵");
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
                Debug.LogError($"gzp ɾ���浵ʧ�ܣ�saveIndex:{saveIndex} :{e.Message}");
                return false;
            }
        }
        #region ��������

        public void Init()
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

}