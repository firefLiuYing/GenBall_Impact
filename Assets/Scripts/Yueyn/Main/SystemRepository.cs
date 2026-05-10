using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Main
{
    /// <summary>
    /// 系统仓库，用于管理系统
    /// </summary>
    public class SystemRepository:Singleton<SystemRepository>
    {
        private readonly Dictionary<Type,ISystem> _systems = new();
        private readonly List<IRenderUpdate> _renderUpdates = new();
        private readonly List<ILogicUpdate> _logicUpdates = new();
        protected override void Init()
        {
            _systems.Clear();
        }
        /// <summary>
        /// 注册系统，如果不以接口形式注册，会输出一个警告，但是不会影响正常运行
        /// </summary>
        /// <param name="system"></param>
        /// <typeparam name="T">为支持热替换，请自行定义并注册ISomeSystem接口，而非直接注册SomeSystem</typeparam>
        public void RegisterSystem<T>(T system) where T : ISystem
        {
            // 如果已经注册过，则抛出异常
            if (_systems.ContainsKey(typeof(T)))
            {
                throw new Exception($"System {typeof(T)} is already registered");
            }
            // 判断是否是接口
            if (!typeof(T).IsInterface)
            {
                Debug.LogWarning($"System {typeof(T)} is a class, but not an interface. Please register an interface instead.");
            }
            // 先初始化再注册
            system.Init();
            _systems.Add(typeof(T), system);
            if (system is IRenderUpdate renderUpdate)
            {
                _renderUpdates.Add(renderUpdate);
            }
            if (system is ILogicUpdate logicUpdate)
            {
                _logicUpdates.Add(logicUpdate);
            }
        }
        /// <summary>
        /// 注销系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnRegisterSystem<T>() where T : ISystem
        {
            // 先移除再注销
            var system = _systems[typeof(T)];
            _systems.Remove(typeof(T));
            if (system is IRenderUpdate renderUpdate)
            {
                _renderUpdates.Remove(renderUpdate);
            }
            if (system is ILogicUpdate logicUpdate)
            {
                _logicUpdates.Remove(logicUpdate);
            }
            system.UnInit();
        }
        
        /// <summary>
        /// 获取系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : ISystem
        {
            if (_systems.TryGetValue(typeof(T), out var system))
            {
                return (T)system;
            }
            Debug.LogError($"System {typeof(T)} is not registered");
            return default(T);
        }

        private readonly List<ILogicUpdate> _cachedLogicUpdates = new();
        private readonly List<IRenderUpdate> _cachedRenderUpdates = new();
        public void RenderUpdate(float deltaTime)
        {
            _cachedRenderUpdates.Clear();
            _cachedRenderUpdates.AddRange(_renderUpdates);
            foreach (var renderUpdate in _cachedRenderUpdates)
            {
                renderUpdate.RenderUpdate(deltaTime);
            }
        }
        public void LogicUpdate(float deltaTime)
        {
            _cachedLogicUpdates.Clear();
            _cachedLogicUpdates.AddRange(_logicUpdates);
            foreach (var logicUpdate in _cachedLogicUpdates)
            {
                logicUpdate.LogicUpdate(deltaTime);
            }
        }
    }
}