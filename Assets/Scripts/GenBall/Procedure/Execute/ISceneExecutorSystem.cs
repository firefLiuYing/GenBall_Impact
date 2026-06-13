using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Execute
{
    public class SceneInitContext
    {
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
        // TODO: 敌人出生列表 — 待场景配置系统设计
        // TODO: 场景特定初始化数据
    }

    public interface ISceneExecutorSystem : ISystem
    {
        void ExecuteSceneSetup(SceneInitContext context);
    }
}
