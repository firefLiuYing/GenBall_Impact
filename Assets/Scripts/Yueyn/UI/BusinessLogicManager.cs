using System.Collections.Generic;
using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.UI
{
    /// <summary>
    /// 业务逻辑管理器（第二层次）
    /// 职责：
    /// 1. 管理所有 BusinessLogic 的生命周期
    /// 2. 提供创建、销毁、查询接口
    /// 3. 监听 UIManager 事件，自动关联 Logic 和 Form
    /// </summary>
    public class BusinessLogicManager : Singleton<BusinessLogicManager>
    {
        // ===== Logic 管理 =====

        private readonly SafeIterableDict<int, BusinessLogicBase> _allLogics = new SafeIterableDict<int, BusinessLogicBase>();
        private readonly Dictionary<string, int> _pathToLogicId = new Dictionary<string, int>();
        private int _nextLogicId = 1;

        protected override void Init()
        {
            Debug.Log("[BusinessLogicManager] Initialized");
        }

        // ===== 创建 Logic =====

        /// <summary>
        /// 创建 BusinessFormLogic 并打开对应页面
        /// </summary>
        public T CreateFormLogic<T>() where T : BusinessFormLogic, new()
        {
            var logic = new T();
            var logicId = _nextLogicId++;
            logic.SetLogicId(logicId);

            // 注册
            _allLogics.Set(logicId, logic);
            _pathToLogicId[logic.PrefabPath] = logicId;

            // 初始化
            logic.OnCreate();

            // 打开页面
            var formId = UIManager.Instance.OpenForm(logic.PrefabPath, logic.FormType, logic.InitParam);
            if (formId != -1)
            {
                var form = UIManager.Instance.GetForm(formId);
                logic.BindForm(form);
            }

            Debug.Log($"[BusinessLogicManager] Created FormLogic: {typeof(T).Name} (ID: {logicId})");
            return logic;
        }

        /// <summary>
        /// 异步创建 BusinessFormLogic 并打开对应页面
        /// </summary>
        public void CreateFormLogicAsync<T>(System.Action<T> onComplete = null) where T : BusinessFormLogic, new()
        {
            var logic = new T();
            var logicId = _nextLogicId++;
            logic.SetLogicId(logicId);

            // 注册
            _allLogics.Set(logicId, logic);
            _pathToLogicId[logic.PrefabPath] = logicId;

            // 初始化
            logic.OnCreate();

            // 异步打开页面
            UIManager.Instance.OpenFormAsync(
                prefabPath: logic.PrefabPath,
                formType: logic.FormType,
                param: logic.InitParam,
                onComplete: (formId, form) =>
                {
                    if (formId != -1)
                    {
                        logic.BindForm(form);
                    }
                    onComplete?.Invoke(logic);
                }
            );

            Debug.Log($"[BusinessLogicManager] Created FormLogic async: {typeof(T).Name} (ID: {logicId})");
        }

        // ===== 销毁 Logic =====

        /// <summary>
        /// 销毁 BusinessLogic
        /// </summary>
        public void DestroyLogic(int logicId)
        {
            if (!_allLogics.TryGetValue(logicId, out var logic))
                return;

            // 如果是 FormLogic，关闭对应页面
            if (logic is BusinessFormLogic formLogic && formLogic.BoundForm != null)
            {
                UIManager.Instance.CloseForm(formLogic.BoundForm.FormId);
            }

            // 销毁
            logic.OnDestroy();

            // 注销
            _allLogics.Remove(logicId);
            if (logic is BusinessFormLogic fl)
            {
                _pathToLogicId.Remove(fl.PrefabPath);
            }

            Debug.Log($"[BusinessLogicManager] Destroyed Logic: {logic.GetType().Name} (ID: {logicId})");
        }

        // ===== 查询 Logic =====

        /// <summary>
        /// 获取指定 ID 的 Logic
        /// </summary>
        public T GetLogic<T>(int logicId) where T : BusinessLogicBase
        {
            if (_allLogics.TryGetValue(logicId, out var logic))
            {
                return logic as T;
            }
            return null;
        }

        /// <summary>
        /// 获取指定路径的 FormLogic
        /// </summary>
        public T GetFormLogicByPath<T>(string prefabPath) where T : BusinessFormLogic
        {
            if (_pathToLogicId.TryGetValue(prefabPath, out var logicId))
            {
                return GetLogic<T>(logicId);
            }
            return null;
        }

        /// <summary>
        /// 检查是否存在指定 ID 的 Logic
        /// </summary>
        public bool HasLogic(int logicId)
        {
            return _allLogics.ContainsKey(logicId);
        }
    }
}
