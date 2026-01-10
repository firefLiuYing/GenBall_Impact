using GenBall.Procedure.Execute;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Game
{
    public class ExecuteComponent : MonoBehaviour, IComponent
    {
        public int Priority => 10000;
        [SerializeField] private string startSceneName;
        [SerializeField,Tooltip("PlayMode的区别：\nDebug:启动到StartSceneName所指的场景\nPlay:按照正常游戏流程启动到存档中上次游玩的场景或者游戏开始场景")] 
        private PlayMode playMode;
        public PlayMode Mode => playMode;
        public string StartSceneName => startSceneName;
        private readonly ExecuteProcedure  _executeProcedure=new();

        public void StartGame(GameData gameData)
        {
            
            Debug.Log($"读取到存档信息：{gameData}");
            Debug.Log("开始游戏");
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        public void Init()
        {
            #if UNITY_EDITOR
            #else
            // 编辑器以外的环境强制是游玩模式
            playMode = PlayMode.Play;
            #endif
            _executeProcedure.Init();
            _executeProcedure.Start();
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