using UnityEngine;
using Yueyn.Resource;
using Yueyn.UI;

namespace Yueyn.Main
{
    /// <summary>
    /// 默认框架，用于初始化一些全局的系统
    /// </summary>
    public class FrameworkDefault : FrameworkBase
    {
        protected override void DoInit()
        {
            // 注册资源系统
            #if UNITY_EDITOR
            SystemRep.RegisterSystem<IResourceSystem>(new ResourceSystemEditor());
            #else
            SystemRep.RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle());
            #endif
            // 注册UI系统
            SystemRep.RegisterSystem<IUISystem>(new UISystemDefault());

            Debug.Log("[FrameworkDefault] Systems registered successfully");
        }
    }
}