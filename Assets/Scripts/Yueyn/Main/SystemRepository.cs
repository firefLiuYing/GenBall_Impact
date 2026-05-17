using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Main
{
    /// <summary>
    /// 系统仓库，用于管理系统的注册和获取
    /// </summary>
    public class SystemRepository : Singleton<SystemRepository>
    {
        private readonly Dictionary<Type, ISystem> _systems = new();
        
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
            
            // 注册到 SystemUpdaterManager
            SystemUpdaterManager.Instance.RegisterSystem(system);
        }
        
        /// <summary>
        /// 注销系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterSystem<T>() where T : ISystem
        {
            if (!_systems.TryGetValue(typeof(T), out var system))
            {
                Debug.LogWarning($"System {typeof(T)} is not registered");
                return;
            }
            
            // 先从更新器移除
            SystemUpdaterManager.Instance.UnregisterSystem(system);
            
            // 再从字典移除
            _systems.Remove(typeof(T));
            
            // 最后注销
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
    }
}