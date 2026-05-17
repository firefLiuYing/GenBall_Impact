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
            Debug.Log("[FrameworkDefault] Systems registered successfully");
        }
        
        protected override void DoFrameUpdate()
        {
            
        }
    }
}