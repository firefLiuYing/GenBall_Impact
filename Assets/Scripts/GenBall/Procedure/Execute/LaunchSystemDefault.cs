using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Framework.Config;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class LaunchSystemDefault : ILaunchSystem
    {
        private IConfigProvider _configProvider;
        private IGameManagerSystem _gameManager;
        private ISaveService _saveService;
        private Fsm<ILaunchSystem> _fsm;
        private List<FsmState<ILaunchSystem>> _states;
        private Variable<GameData> _gameData;
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();

        private RunningMode _runningMode;
        private string _startSceneName;

        public RunningMode Mode => _runningMode;
        public string StartSceneName => _startSceneName;

        public void Init()
        {
            _configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            var config = _configProvider.GetConfig<AppSettingsConfig>();
            _startSceneName = config.startSceneName;
            _runningMode = config.runningMode;

            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _saveService = SystemRepository.Instance.GetSystem<ISaveService>();

            _gameManager.Mode = _runningMode;

            _states = new List<FsmState<ILaunchSystem>>
            {
                new ProcedureLoadState(),
                new StartFormState(),
                new LoadSceneState()
            };

            _fsm = Fsm<ILaunchSystem>.Create("LauncherExecute", this, _states);
            _fsm.Start<ProcedureLoadState>();

            _gameData = Variable<GameData>.Create();
            _fsm.SetData("GameData", _gameData);
        }

        public void UnInit()
        {
            _fsm?.Shutdown();
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

        private void StartWithoutLoad()
        {
            _gameManager.CurSaveIndex = 0;
            var gameData = new GameData();
            InternalStartGame(gameData);
        }

        private void InternalStartGame(GameData gameData)
        {
            Debug.Log("开始游戏");
            _gameManager.GameData = gameData;
            _gameData.PostValue(gameData);
        }

        private async void InternalStartNewGame()
        {
            try
            {
                var saveIndex = await _saveService.CreateNewSave();
                InternalStartGame(saveIndex);
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
                var gameData = await _saveService.LoadGameData(saveIndex);
                _gameManager.CurSaveIndex = saveIndex;
                InternalStartGame(gameData);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}
