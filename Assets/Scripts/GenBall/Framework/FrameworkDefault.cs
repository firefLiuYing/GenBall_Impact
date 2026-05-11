using UnityEngine;
using Yueyn.Main;
using Yueyn.Resource;
using Yueyn.UI;
using Yueyn.Event;
using Yueyn.Pool;

namespace GenBall.Framework
{
    /// <summary>
    /// 默认框架，用于初始化一些全局的系统
    /// </summary>
    public class FrameworkDefault : FrameworkBase
    {
        protected override void DoInit()
        {
            // 注册事件系统
            SystemRep.RegisterSystem<IEventSystem>(new CEventSystem());
            // 注册资源系统
            #if UNITY_EDITOR
            SystemRep.RegisterSystem<IResourceSystem>(new ResourceSystemEditor());
            #else
            SystemRep.RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle());
            #endif
            // 注册UI系统
            SystemRep.RegisterSystem<IUISystem>(new UISystemDefault());
            // 注册对象池系统
            SystemRep.RegisterSystem<IPoolSystem>(new PoolSystemDefault());

            Debug.Log("[FrameworkDefault] Systems registered successfully");
        }
    }
}