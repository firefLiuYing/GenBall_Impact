using UnityEngine;
using GenBall.Procedure.Execute;
using Yueyn.Main;

namespace GenBall.Map
{
    public class TeleportSystem : ITeleportSystem
    {
        public void Init() { }
        public void UnInit() { }

        public bool IsTeleporting { get; set; } = false;
        public SavePointModel CachedSavePointModel { get;private set; } = null;
        public bool Teleport(TeleportRequestInfo teleportRequestInfo)
        {
            if(IsTeleporting) return false;
            if(string.IsNullOrEmpty(teleportRequestInfo.SceneName)) return false;
            var savePointModel=SystemRepository.Instance.GetSystem<ISceneStateSystem>().GetSavePointModel(teleportRequestInfo.SceneName, teleportRequestInfo.SavePointIndex);

            if(savePointModel==null) return false;
            IsTeleporting=true;
            CachedSavePointModel = savePointModel;
            var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
            loadSystem.SetTargetSavePoint(CachedSavePointModel);
            loadSystem.AsyncLoadScene(teleportRequestInfo.SceneName);
            return true;
        }
    }

    public struct TeleportRequestInfo
    {
        public string SceneName;
        public int SavePointIndex;
    }
}