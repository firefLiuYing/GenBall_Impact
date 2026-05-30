using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Event;
using GenBall.Framework.Config;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class LaunchSystemDefault : ILaunchSystem, IFrameUpdate
    {
        private IConfigProvider _configProvider;
        private IGameManagerSystem _gameManager;
        private ISaveService _saveService;
        private SimpleFsm<ILaunchSystem> _fsm;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();

        private RunningMode _runningMode;
        private string _startSceneName;
        private bool _devMode;
        private float _sceneLoadProgress;
        private bool _isSceneLoading;

        public RunningMode Mode => _runningMode;
        public string StartSceneName => _startSceneName;
        public float SceneLoadProgress => _sceneLoadProgress;
        public bool IsSceneLoading => _isSceneLoading;

        public SystemScope FrameUpdateScope => SystemScope.Framework;

        internal void SetSceneLoading(bool isLoading, float progress = 0f)
        {
            _isSceneLoading = isLoading;
            _sceneLoadProgress = progress;
        }

        public void Init()
        {
            _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var config = _configProvider.GetConfig<AppSettingsConfig>();
            _startSceneName = config.startSceneName;
            _runningMode = config.runningMode;
            _devMode = config.devMode;

            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _saveService = SystemRepository.Instance.GetSystem<ISaveService>();

            _gameManager.Mode = _runningMode;

            _fsm = new SimpleFsm<ILaunchSystem>(this,
                new SplashState(),
                new StartFormState(),
                new LoadSceneState()
            );

            _fsm.Start<SplashState>();
        }

        public void UnInit()
        {
            _fsm?.Shutdown();
        }

        public void FrameUpdate(float deltaTime)
        {
            if (_fsm == null || !_fsm.IsRunning)
                return;

            _fsm.Update(deltaTime);

            // Splash 阶段最小展示时长，之后推进到主菜单（DevMode 下跳过等待）
            float splashMinTime = _devMode ? 0f : 1.5f;
            if (_fsm.CurrentStateType == typeof(SplashState) && _fsm.CurrentStateTime > splashMinTime)
            {
                CEventRouter.Instance.FireNow((int)GlobalEventId.SplashComplete);
                _fsm.ChangeState<StartFormState>();
            }
        }

        public void StartNewGame()
        {
            if ((Mode & RunningMode.SaveData) != 0)
            {
                InternalStartNewGame();
            }
            else
            {
                StartWithoutLoad();
            }
        }

        public void ContinueLastGame()
        {
            if ((Mode & RunningMode.LoadData) != 0)
            {
                InternalContinueLastGame();
            }
            else
            {
                StartWithoutLoad();
            }
        }

        public void LoadGame(int saveIndex)
        {
            if ((Mode & RunningMode.LoadData) != 0)
            {
                InternalStartGame(saveIndex);
            }
            else
            {
                StartWithoutLoad();
            }
        }

        public void SkipSplash()
        {
            if (_fsm != null && _fsm.CurrentStateType == typeof(SplashState))
            {
                CEventRouter.Instance.FireNow((int)GlobalEventId.SplashComplete);
                _fsm.ChangeState<StartFormState>();
            }
        }

        private void StartWithoutLoad()
        {
            _gameManager.CurSaveIndex = 0;
            SetProviderDefaults();
            var gameData = new GameData();
            FinalizeGameData(gameData);
        }

        private void FinalizeGameData(GameData gameData)
        {
            Debug.Log("开始游戏");
            _gameManager.GameData = gameData;
            _fsm.ChangeState<LoadSceneState>();
        }

        private void SetProviderDefaults()
        {
            var playerProvider = _gameManager.GetProvider("Player") as PlayerSaveDataProvider;
            if (playerProvider != null)
            {
                playerProvider.RuntimeData.lastSceneName = _startSceneName;
                playerProvider.RuntimeData.lastSavePointIndex = 0;
            }
        }

        private async void InternalStartNewGame()
        {
            try
            {
                var saveIndex = await _saveService.CreateNewSave();
                _gameManager.CurSaveIndex = saveIndex;

                // Populate providers with default initial state
                SetProviderDefaults();

                // Save the initial state to disk
                await _gameManager.SaveGame();

                FinalizeGameData(_gameManager.GameData);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private async void InternalContinueLastGame()
        {
            try
            {
                var saveSlotDatas = await _saveService.GetSaveSlotDatas();
                _cachedSaveSlotData.Clear();
                _cachedSaveSlotData.AddRange(saveSlotDatas);
                var saveIndex = _cachedSaveSlotData.OrderByDescending(slot => slot.LastUpdateTime).First().saveIndex;
                InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private async void InternalStartGame(int saveIndex)
        {
            try
            {
                var success = await _gameManager.LoadGameData(saveIndex);
                if (!success)
                {
                    Debug.LogError($"[LaunchSystem] Failed to load save slot {saveIndex}");
                    return;
                }
                FinalizeGameData(_gameManager.GameData);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
