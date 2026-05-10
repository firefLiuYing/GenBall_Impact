using System;
using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.UI
{
    /// <summary>
    /// UI Logic全局管理器（单例）
    /// 负责创建、管理所有Logic实例的生命周期
    /// </summary>
    public class UILogicManager : Singleton<UILogicManager>
    {
        /// <summary>
        /// 所有Logic实例（按LogicId索引）
        /// </summary>
        private readonly Dictionary<int, UILogicBase> _allLogics = new Dictionary<int, UILogicBase>();

        /// <summary>
        /// 自增Logic ID生成器
        /// </summary>
        private int _nextLogicId = 1;

        protected override void Init()
        {
            _allLogics.Clear();
            _nextLogicId = 1;
            Debug.Log("[UILogicManager] Initialized");
        }

        /// <summary>
        /// 创建Logic实例（外部调用入口）
        /// </summary>
        /// <typeparam name="T">Logic类型</typeparam>
        /// <returns>Logic实例</returns>
        public T CreateLogic<T>() where T : UILogicBase, new()
        {
            var logic = new T();
            int logicId = _nextLogicId++;
            logic.SetLogicId(logicId);

            _allLogics[logicId] = logic;

            Debug.Log($"[UILogicManager] Created Logic: {typeof(T).Name} (ID: {logicId})");

            return logic;
        }

        /// <summary>
        /// 获取Logic实例
        /// </summary>
        public UILogicBase GetLogic(int logicId)
        {
            _allLogics.TryGetValue(logicId, out var logic);
            return logic;
        }

        /// <summary>
        /// 获取指定类型的Logic实例（返回第一个匹配的）
        /// </summary>
        public T GetLogic<T>() where T : UILogicBase
        {
            foreach (var logic in _allLogics.Values)
            {
                if (logic is T typedLogic)
                {
                    return typedLogic;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定类型的所有Logic实例
        /// </summary>
        public List<T> GetAllLogics<T>() where T : UILogicBase
        {
            var result = new List<T>();
            foreach (var logic in _allLogics.Values)
            {
                if (logic is T typedLogic)
                {
                    result.Add(typedLogic);
                }
            }
            return result;
        }

        /// <summary>
        /// 销毁Logic实例（在Form关闭时调用）
        /// </summary>
        public void DestroyLogic(int logicId)
        {
            if (_allLogics.TryGetValue(logicId, out var logic))
            {
                _allLogics.Remove(logicId);
                Debug.Log($"[UILogicManager] Destroyed Logic: {logic.GetType().Name} (ID: {logicId})");
            }
        }

        /// <summary>
        /// 检查Logic是否存在
        /// </summary>
        public bool HasLogic(int logicId)
        {
            return _allLogics.ContainsKey(logicId);
        }

        /// <summary>
        /// 清理所有Logic
        /// </summary>
        public void ClearAllLogics()
        {
            _allLogics.Clear();
            _nextLogicId = 1;
            Debug.Log("[UILogicManager] Cleared all logics");
        }

        /// <summary>
        /// 获取当前Logic数量
        /// </summary>
        public int GetLogicCount()
        {
            return _allLogics.Count;
        }
    }
}
