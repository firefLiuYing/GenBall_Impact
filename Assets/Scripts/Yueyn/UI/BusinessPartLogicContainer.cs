using System;
using Yueyn.Utils;
using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// BusinessPartLogic 容器
    /// 管理子 PartLogic 的生命周期，支持自动发现子 PartView 并创建对应 PartLogic。
    /// </summary>
    public abstract class BusinessPartLogicContainer : BusinessLogicBase
    {
        private readonly SafeIterableList<BusinessPartLogic> _partLogics = new SafeIterableList<BusinessPartLogic>();

        /// <summary>
        /// 添加子 PartLogic
        /// </summary>
        protected void AddPartLogic(BusinessPartLogic partLogic)
        {
            if (_partLogics.Contains(partLogic))
                return;

            _partLogics.Add(partLogic);
        }

        /// <summary>
        /// 移除子 PartLogic
        /// </summary>
        protected void RemovePartLogic(BusinessPartLogic partLogic)
        {
            if (!_partLogics.Contains(partLogic))
                return;

            partLogic.OnDestroy();
            _partLogics.Remove(partLogic);
        }

        /// <summary>
        /// 清空所有子 PartLogic
        /// </summary>
        protected void ClearPartLogics()
        {
            var snapshot = _partLogics.GetIterableSnapshot();
            foreach (var partLogic in snapshot)
                partLogic.OnDestroy();
            _partLogics.Clear();
        }

        // ===== 子 Part 自动发现 =====

        /// <summary>
        /// 递归扫描 root 下的所有 PartViewBase，自动创建对应的 PartLogic 并绑定。
        /// 跳过已经有父 PartLogic 管理的 PartView（避免重复创建）。
        /// </summary>
        protected void DiscoverChildPartLogics(Transform root)
        {
            if (root == null) return;

            var childViews = root.GetComponentsInChildren<PartViewBase>(includeInactive: true);
            foreach (var view in childViews)
            {
                if (view.transform == root) continue;
                if (IsOwnedByOtherPart(view, root)) continue;

                var partLogicType = PartLogicTypeRegistry.Resolve(view.GetType());
                if (partLogicType == null) continue;

                try
                {
                    var partLogic = (BusinessPartLogic)Activator.CreateInstance(partLogicType);
                    partLogic.ParentTransform = view.transform;
                    partLogic.OnCreate();
                    AddPartLogic(partLogic);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PartLogicContainer] Failed to create PartLogic for {view.GetType().Name}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 检查 PartView 是否属于某个已在 root 层级中发现的父 PartView
        /// </summary>
        private static bool IsOwnedByOtherPart(PartViewBase view, Transform root)
        {
            var parent = view.transform.parent;
            while (parent != null && parent != root)
            {
                if (parent.GetComponent<PartViewBase>() != null)
                    return true;
                parent = parent.parent;
            }
            return false;
        }

        // ===== 生命周期 =====

        protected override void OnDestroyInternal()
        {
            ClearPartLogics();
            base.OnDestroyInternal();
        }
    }
}
