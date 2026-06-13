using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.Framework.Config;
using GenBall.Player;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class GameStartSystemDefault : IGameStartSystem
    {
        private IConfigProvider _configProvider;
        private IGameManagerSystem _gameManager;
        private ISaveService _saveService;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();

        public void Init()
        {
            _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _saveService = SystemRepository.Instance.GetSystem<ISaveService>();
        }

        public void UnInit()
        {
            _cachedSaveSlotData.Clear();
        }

        public async Task<GameStartContext> PrepareStartAsync(GameStartRequest request)
        {
            switch (request.Type)
            {
                case GameStartType.NewGame:
                    return PrepareNewGame();
                case GameStartType.Continue:
                    return await PrepareContinueGame();
                case GameStartType.LoadGame:
                    return await PrepareLoadGame(request.SaveIndex);
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Type), $"Unknown GameStartType: {request.Type}");
            }
        }

        private GameStartContext PrepareNewGame()
        {
            // Set default provider state (in memory only, no disk write)
            SetProviderDefaults();

            // New games don't have a save index until first save
            _gameManager.CurSaveIndex = -1;

            var gameData = new GameData();
            _gameManager.GameData = gameData;

            Debug.Log("[GameStartSystem] New game prepared (not yet saved to disk)");

            return new GameStartContext
            {
                TargetSceneName = GetStartSceneName(),
                TargetSavePointIndex = 0,
                GameData = gameData,
                IsNewGame = true,
            };
        }

        private async Task<GameStartContext> PrepareContinueGame()
        {
            if ((_gameManager.Mode & RunningMode.LoadData) == 0)
            {
                Debug.Log("[GameStartSystem] LoadData mode disabled, falling back to new game.");
                return PrepareNewGame();
            }

            try
            {
                var saveSlotDatas = await _saveService.GetSaveSlotDatas();
                if (saveSlotDatas == null || !saveSlotDatas.Any())
                {
                    Debug.Log("[GameStartSystem] No save slots found, falling back to new game.");
                    return PrepareNewGame();
                }

                var latestSlot = saveSlotDatas.OrderByDescending(s => s.LastUpdateTime).First();
                return await LoadSaveAndBuildContext(latestSlot.saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameStartSystem] Continue failed: {e.Message}");
                return PrepareNewGame();
            }
        }

        private async Task<GameStartContext> PrepareLoadGame(int saveIndex)
        {
            if ((_gameManager.Mode & RunningMode.LoadData) == 0)
            {
                Debug.Log("[GameStartSystem] LoadData mode disabled, falling back to new game.");
                return PrepareNewGame();
            }

            try
            {
                return await LoadSaveAndBuildContext(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameStartSystem] Load game failed for slot {saveIndex}: {e.Message}");
                return PrepareNewGame();
            }
        }

        private async Task<GameStartContext> LoadSaveAndBuildContext(int saveIndex)
        {
            var success = await _gameManager.LoadGameData(saveIndex);
            if (!success)
            {
                throw new Exception($"Failed to load save slot {saveIndex}");
            }

            var playerProvider = _gameManager.GetProvider("Player") as PlayerSaveDataProvider;
            var targetSceneName = playerProvider?.RuntimeData.lastSceneName;
            var targetSavePointIndex = playerProvider?.RuntimeData.lastSavePointIndex ?? 0;

            if (string.IsNullOrEmpty(targetSceneName))
            {
                targetSceneName = GetStartSceneName();
            }

            Debug.Log($"[GameStartSystem] Game loaded from slot {saveIndex}, scene={targetSceneName}, savePoint={targetSavePointIndex}");

            return new GameStartContext
            {
                TargetSceneName = targetSceneName,
                TargetSavePointIndex = targetSavePointIndex,
                GameData = _gameManager.GameData,
                IsNewGame = false,
            };
        }

        private void SetProviderDefaults()
        {
            var playerProvider = _gameManager.GetProvider("Player") as PlayerSaveDataProvider;
            if (playerProvider != null)
            {
                playerProvider.RuntimeData.lastSceneName = GetStartSceneName();
                playerProvider.RuntimeData.lastSavePointIndex = 0;
            }
        }

        private string GetStartSceneName()
        {
            var config = _configProvider.GetConfig<AppSettingsConfig>();
            return config?.startSceneName ?? "Prologue";
        }
    }
}
