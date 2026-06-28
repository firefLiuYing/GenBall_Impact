using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Procedure;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class GameManager : IGameManagerSystem
    {
        private int _curSaveIndex;
        private readonly Dictionary<string, ISaveDataProvider> _providers = new();

        public GameData GameData { get; set; }

        public RunningMode Mode { get; set; }

        public int CurSaveIndex
        {
            get => _curSaveIndex;
            set => _curSaveIndex = value;
        }

        public void Init() { }
        public void UnInit() { }

        public void RegisterSaveDataProvider(ISaveDataProvider provider)
        {
            _providers[provider.DataKey] = provider;
        }

        public void UnregisterSaveDataProvider(ISaveDataProvider provider)
        {
            _providers.Remove(provider.DataKey);
        }

        public ISaveDataProvider GetProvider(string key)
        {
            _providers.TryGetValue(key, out var provider);
            return provider;
        }

        public async Task<bool> SaveGame()
        {
            try
            {
                if ((Mode & RunningMode.SaveData) == 0) return true;
                if (CurSaveIndex < 0) return false;

                // Assemble GameData from all registered providers
                GameData = new GameData();
                foreach (var kvp in _providers)
                {
                    GameData.SetData(kvp.Key, kvp.Value.CollectSaveData());
                }
                GameData.LastUpdateTime = DateTime.Now;

                Debug.Log($"Save game data: {GameData}");
                var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
                return await saveService.SaveGameData(GameData, CurSaveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateSaveFields(string providerKey, Dictionary<string, string> fields)
        {
            if (fields == null || fields.Count == 0) return true;

            if (CurSaveIndex < 0)
            {
                Debug.LogWarning($"[GameManager] UpdateSaveFields: No active save slot.");
                return false;
            }

            if (!_providers.TryGetValue(providerKey, out var provider))
            {
                Debug.LogWarning($"[GameManager] UpdateSaveFields: Provider '{providerKey}' is not registered. Cross-system update ignored.");
                return false;
            }

            try
            {
                var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
                var gameData = await saveService.LoadGameData(CurSaveIndex);
                if (gameData == null)
                {
                    Debug.LogError($"[GameManager] UpdateSaveFields: Failed to load save slot {CurSaveIndex}.");
                    return false;
                }

                // Hydrate provider from disk state, merge fields, re-collect
                var currentJson = gameData.GetData(providerKey);
                if (!string.IsNullOrEmpty(currentJson))
                {
                    provider.ApplySaveData(currentJson);
                }
                provider.MergeSaveFields(fields);
                gameData.SetData(providerKey, provider.CollectSaveData());
                gameData.LastUpdateTime = DateTime.Now;

                var result = await saveService.SaveGameData(gameData, CurSaveIndex);
                if (result)
                {
                    GameData = gameData;
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] UpdateSaveFields failed: {e.Message}");
                return false;
            }
        }

        public async Task<bool> LoadGameData(int saveIndex)
        {
            try
            {
                if ((Mode & RunningMode.LoadData) == 0) return false;

                var saveService = SystemRepository.Instance.GetSystem<ISaveService>();
                GameData = await saveService.LoadGameData(saveIndex);
                if (GameData == null) return false;

                CurSaveIndex = saveIndex;

                // Distribute to registered providers
                foreach (var block in GameData.DataBlocks)
                {
                    if (_providers.TryGetValue(block.Key, out var provider))
                    {
                        provider.ApplySaveData(block.Value);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
    }

    [Flags]
    public enum RunningMode
    {
        SaveData = 1 << 0,
        LoadData = 1 << 1,
    }
}
