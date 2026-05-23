using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.Main
{
    /// <summary>
    /// ϵͳ�ֿ⣬���ڹ���ϵͳ��ע��ͻ�ȡ
    /// </summary>
    public class SystemRepository : Singleton<SystemRepository>
    {
        private readonly Dictionary<Type, ISystem> _systems = new();
        
        protected override void Init()
        {
            _systems.Clear();
        }
        
        /// <summary>
        /// ע��ϵͳ��������Խӿ���ʽע�ᣬ�����һ�����棬���ǲ���Ӱ����������
        /// </summary>
        /// <param name="system"></param>
        /// <typeparam name="T">Ϊ֧�����滻�������ж��岢ע��ISomeSystem�ӿڣ�����ֱ��ע��SomeSystem</typeparam>
        public void RegisterSystem<T>(T system) where T : ISystem
        {
            // ����Ѿ�ע��������׳��쳣
            if (_systems.ContainsKey(typeof(T)))
            {
                throw new Exception($"System {typeof(T)} is already registered");
            }
            
            // �ж��Ƿ��ǽӿ�
            if (!typeof(T).IsInterface)
            {
                Debug.LogWarning($"System {typeof(T)} is a class, but not an interface. Please register an interface instead.");
            }
            
            // �ȳ�ʼ����ע��
            system.Init();
            _systems.Add(typeof(T), system);
            
            // ע�ᵽ SystemUpdaterManager
            SystemUpdaterManager.Instance.RegisterSystem(system);
        }
        
        /// <summary>
        /// ע��ϵͳ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnregisterSystem<T>() where T : ISystem
        {
            if (!_systems.TryGetValue(typeof(T), out var system))
            {
                Debug.LogWarning($"System {typeof(T)} is not registered");
                return;
            }
            
            // �ȴӸ������Ƴ�
            SystemUpdaterManager.Instance.UnregisterSystem(system);
            
            // �ٴ��ֵ��Ƴ�
            _systems.Remove(typeof(T));
            
            // ���ע��
            system.UnInit();
        }
        
        /// <summary>
        /// ��ȡϵͳ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <summary>
        /// 检查系统是否已注册
        /// </summary>
        public bool HasSystem<T>() where T : ISystem
        {
            return _systems.ContainsKey(typeof(T));
        }

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