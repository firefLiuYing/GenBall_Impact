using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets
{
    public class BulletSystem:MonoBehaviour,IComponent
    {
        public int Priority => 1000;
        // 需要干的事情有，生成子弹，销毁子弹，管理子弹生命周期
        public void Init()
        {
            
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

    [StructLayout(LayoutKind.Auto)]
    public struct BulletLaunchInfo
    {
        
    }
}