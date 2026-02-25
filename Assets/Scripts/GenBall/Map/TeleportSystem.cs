using GenBall.Utils.Singleton;
using UnityEngine;

namespace GenBall.Map
{
    public class TeleportSystem : ISingleton
    {
        public static TeleportSystem Instance=>SingletonManager.GetSingleton<TeleportSystem>();

        public bool IsTeleporting { get; set; } = false;
        public SavePointModel CachedSavePointModel { get;private set; } = null;
        public bool Teleport(TeleportRequestInfo teleportRequestInfo)
        {
            if(IsTeleporting) return false;
            if(string.IsNullOrEmpty(teleportRequestInfo.SceneName)) return false;
            var savePointModel=SceneSystem.Instance.GetSavePointModel(teleportRequestInfo.SceneName, teleportRequestInfo.SavePointIndex);
            
            if(savePointModel==null) return false;
            IsTeleporting=true;
            CachedSavePointModel = savePointModel;
            GameEntry.Scene.LoadScene(teleportRequestInfo.SceneName);
            return true;
        }
    }

    public struct TeleportRequestInfo
    {
        public string SceneName;
        public int SavePointIndex;
    }
}