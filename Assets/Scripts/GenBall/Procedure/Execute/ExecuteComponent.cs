using System;
using System.Collections.Generic;
using System.Linq;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class ExecuteComponent : MonoBehaviour, IComponent
    {
        public int Priority => 10000;
        [SerializeField] private string startSceneName;
        [SerializeField] private RunningMode runningMode;
        public RunningMode Mode => runningMode;
        public string StartSceneName => startSceneName;

        private Fsm<ExecuteComponent> _fsm;
        private readonly List<FsmState<ExecuteComponent>> _states = new();
        private Variable<GameData> _gameData;

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
        
        private void RegisterStates()
        {
            _states.Clear();
            _states.Add(new ProcedureLoadState());
            _states.Add(new StartFormState());
            _states.Add(new LoadSceneState());
        }
        public void Init()
        {
            #if UNITY_EDITOR
            if ((Mode & RunningMode.LoadData) == 0)
            {
                runningMode = 0;
            }
            #else
            runningMode=RunningMode.SaveData|RunningMode.LoadData;
            #endif
            GameManager.Instance.Mode = Mode;
            RegisterStates();
            _fsm=GameEntry.Fsm.CreateFsm("LauncherExecute", this, _states);
            _fsm.Start<ProcedureLoadState>();
            _gameData=Variable<GameData>.Create();
            _fsm.SetData("GameData",_gameData);
        }

        private void StartWithoutLoad()
        {
            GameManager.Instance.CurSaveIndex = 0;
            var gameData=new  GameData();
            InternalStartGame(gameData);
        }
        
        private void InternalStartGame(GameData gameData)
        {
            Debug.Log("¿ªÊ¼ÓÎÏ·");
            GameManager.Instance.GameData = gameData;
            _gameData.PostValue(gameData);
        }

        private async void InternalStartNewGame()
        {
            try
            {
                var saveIndex = await GameEntry.Save.CreateNewSave();
                InternalStartGame(saveIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        
        private readonly List<SaveSlotData> _cachedSaveSlotData = new();
        private async void InternalContinueLastGame()
        {
            try
            {
                var saveSlotDatas = await GameEntry.Save.GetSaveSlotDatas();
                _cachedSaveSlotData.Clear();
                _cachedSaveSlotData.AddRange(saveSlotDatas);
                var saveIndex= _cachedSaveSlotData.OrderByDescending(slot=>slot.LastUpdateTime).First().saveIndex;
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
                var gameData = await GameEntry.Save.LoadGameData(saveIndex);
                GameManager.Instance.CurSaveIndex=saveIndex;
                InternalStartGame(gameData);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
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
    }
}