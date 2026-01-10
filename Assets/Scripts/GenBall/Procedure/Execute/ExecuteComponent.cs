using System.Collections.Generic;
using UnityEngine;
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
        public void StartGame(GameData gameData)
        {
            Debug.Log("开始游戏");
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void RegisterStates()
        {
            _states.Clear();
            _states.Add(new ProcedureLoadState());
        }
        public void Init()
        {
            #if UNITY_EDITOR
            #else
            // 编辑器以外的环境强制是游玩模式
            playMode = PlayMode.Play;
            #endif
            _fsm=GameEntry.Fsm.CreateFsm("LauncherExecute", this, _states);
            RegisterStates();
            _fsm.Start<ProcedureLoadState>();
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