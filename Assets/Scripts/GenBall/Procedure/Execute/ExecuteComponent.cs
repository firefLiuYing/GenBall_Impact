using System;
using System.Collections.Generic;
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
        [SerializeField,Tooltip("PlayMode的区别：\nDebug:启动到StartSceneName所指的场景\nPlay:按照正常游戏流程启动到存档中上次游玩的场景或者游戏开始场景")] 
        private PlayMode playMode;
        public PlayMode Mode => playMode;
        public string StartSceneName => startSceneName;

        private Fsm<ExecuteComponent> _fsm;
        private readonly List<FsmState<ExecuteComponent>> _states = new();
        private Variable<GameData> _gameData;
        public void StartGame(GameData gameData)
        {
            Debug.Log("开始游戏");
            _gameData.PostValue(gameData);
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
            #else
            // 编辑器以外的环境强制是游玩模式
            playMode = PlayMode.Play;
            #endif
            GameManager.Instance.Mode = Mode;
            RegisterStates();
            _fsm=GameEntry.Fsm.CreateFsm("LauncherExecute", this, _states);
            _fsm.Start<ProcedureLoadState>();
            _gameData=Variable<GameData>.Create();
            _fsm.SetData("GameData",_gameData);
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
        public enum PlayMode
        {
            Debug,
            Play,
        }
    }
}