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
