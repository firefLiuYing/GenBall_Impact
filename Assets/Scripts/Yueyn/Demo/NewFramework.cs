using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Demo.Framework
{
    /// <summary>
    /// 主要作用是借助MonoBehaviour的生命周期来实现游戏更新，以及充当程序入口基类
    /// </summary>
    public class FrameworkBase:MonoSingleton<FrameworkBase>
    {
        protected SystemRepository SystemRep;
        // 封装一下SystemRep的方法
        // .............
        
        

        protected override void Init()
        {
            InternalInit();
        }

        private void InternalInit()
        {
            // 可能的需要固定初始化的地方
            DoInit();
            // 可能的需要固定初始化的地方
        }
        protected virtual void DoInit(){}
        private void Start()
        {
            InternalStart();
        }

        private void InternalStart()
        {
            // xxxxx
            DoStart();
            // xxxxx
        }
        protected virtual void DoStart(){}
        private void Update()
        {
            // .............
            InternalFrameUpdate();
            // .............
        }

        private void InternalFrameUpdate()
        {
            DoFrameUpdate();
            SystemUpdaterManager.Instance.FrameUpdate(Time.deltaTime);
        }
        protected virtual void DoFrameUpdate(){}
        private void FixedUpdate()
        {
            InternalLogicUpdate();
        }

        private void InternalLogicUpdate()
        {
            // ..................
            DoLogicUpdate();
            SystemUpdaterManager.Instance.LogicUpdate(Time.fixedDeltaTime);
            // ..................
        }
        protected virtual void DoLogicUpdate(){}
        private void LateUpdate()
        {
            InternalLateFrameUpdate();
        }
        private void InternalLateFrameUpdate()
        {
            // ...............
            DoLateFrameUpdate();
            SystemUpdaterManager.Instance.LateFrameUpdate(Time.fixedDeltaTime);
            // ...............
        }
        protected virtual void DoLateFrameUpdate(){}
    }

    // 只有业务逻辑实现这个接口，框架基建不实现
    public interface ISystem
    {
        public void Init();
        public void UnInit();
    }
    public class SystemRepository
    {
        private readonly Dictionary<Type, ISystem> _systems = new();
        // 注册方法等和上一版一致，会顺便注册到SystemUpdaterManager
    }

    // 可能需要换一个更贴切的名字
    public enum SystemScope
    {
        /// <summary>
        /// 用于游戏逻辑和表现的，目的是暂停后不再更新
        /// </summary>
        Game,
        /// <summary>
        /// 用于系统逻辑的，目的是不受暂停影响
        /// </summary>
        Framework
        // 暂时想到这两个，如果有其他的，再加
    }
    public interface ILogicUpdate
    {
        public void LogicUpdate(float deltaTime);
        public SystemScope LogicUpdateScope { get; }
    }
    // FrameUpdate和LateFrameUpdate就不写了
    public class SystemUpdaterManager : Singleton<SystemUpdaterManager>
    {
        protected override void Init()
        {
            // .........
        }
        // 注册相关方法，注意会根据SystemScope来注册到不同的SystemUpdater中
        // ...........
        private SystemUpdater _gameSystemUpdater;
        private SystemUpdater _frameworkSystemUpdater;
        // 三个更新方法，内部需要根据暂停来决定是否更新，更新时直接调用SystemUpdater的Update方法
        public void LogicUpdate(float deltaTime)
        {
            
        }
        public void FrameUpdate(float deltaTime){}
        public void LateFrameUpdate(float deltaTime){}
    }

    public class SystemUpdater
    {
        // 持有三种IUpdate的容器
        // 提供三种更新方法
        // 注意foreach时的迭代器问题，如果有余裕可以封装一层专门解决了迭代器迭代过程不能插入删除元素的问题的容器类，这个情况应该会遇到不少
    }
}