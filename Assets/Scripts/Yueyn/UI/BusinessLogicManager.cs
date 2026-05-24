using UnityEngine;
using Yueyn.Utils;

namespace Yueyn.UI
{
    /// <summary>
    /// 业务逻辑管理器（第二层次）
    /// 职责：管理所有 BusinessLogic 的生命周期（创建、销毁、查询）
    ///
    /// 注意：管理器不感知 Form 概念。BusinessFormLogic 在自身 OnCreate/OnDestroy
    /// 中处理 Form 的打开/关闭，高层 Logic（管理多个 Form 的体系）同样只需继承
    /// BusinessLogicBase 即可被本管理器管理。
    /// </summary>
    public class BusinessLogicManager : Singleton<BusinessLogicManager>
    {
        // ===== Logic 管理 =====

        private readonly SafeIterableDict<int, BusinessLogicBase> _allLogics = new SafeIterableDict<int, BusinessLogicBase>();
        private int _nextLogicId = 1;

        protected override void Init()
        {
            Debug.Log("[BusinessLogicManager] Initialized");
        }

        // ===== 创建 Logic =====

        /// <summary>
        /// 创建任意 BusinessLogicBase 子类并管理其生命周期
        /// </summary>
        /// <param name="configure">可选配置回调，在 OnCreate 之前调用（如设置 PartLogic 的 ParentTransform）</param>
        public T CreateLogic<T>(System.Action<T> configure = null) where T : BusinessLogicBase, new()
        {
            var logic = new T();
            var logicId = _nextLogicId++;
            logic.SetLogicId(logicId);

            _allLogics.Set(logicId, logic);
            configure?.Invoke(logic);
            logic.OnCreate();

            Debug.Log($"[BusinessLogicManager] Created Logic: {typeof(T).Name} (ID: {logicId})");
            return logic;
        }

        // ===== 销毁 Logic =====

        /// <summary>
        /// 销毁指定 ID 的 Logic
        /// </summary>
        public void DestroyLogic(int logicId)
        {
            if (!_allLogics.TryGetValue(logicId, out var logic))
                return;

            logic.OnDestroy();
            _allLogics.Remove(logicId);

            Debug.Log($"[BusinessLogicManager] Destroyed Logic: {logic.GetType().Name} (ID: {logicId})");
        }

        // ===== 查询 Logic =====

        /// <summary>
        /// 获取指定 ID 的 Logic
        /// </summary>
        public T GetLogic<T>(int logicId) where T : BusinessLogicBase
        {
            if (_allLogics.TryGetValue(logicId, out var logic))
                return logic as T;
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
