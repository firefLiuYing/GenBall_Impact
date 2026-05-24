using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace Yueyn.UI
{
    // ── PartLogic 类型注册表 ──

    /// <summary>
    /// PartView 类型 → PartLogic 类型的映射注册表
    /// 通过反射扫描程序集中所有 BusinessPartLogic&lt;TView&gt; 子类自动构建
    /// </summary>
    public static class PartLogicTypeRegistry
    {
        private static Dictionary<Type, Type> _cache;
        private static bool _initialized;

        /// <summary>
        /// 根据 PartView 类型查找对应的 PartLogic 类型
        /// </summary>
        public static Type Resolve(Type partViewType)
        {
            if (!_initialized) BuildCache();
            _cache.TryGetValue(partViewType, out var result);
            return result;
        }

        private static void BuildCache()
        {
            _cache = new Dictionary<Type, Type>();
            _initialized = true;

            var openGeneric = typeof(BusinessPartLogic<>);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;
                    var baseType = type.BaseType;
                    while (baseType != null)
                    {
                        if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == openGeneric)
                        {
                            var viewType = baseType.GetGenericArguments()[0];
                            if (!_cache.ContainsKey(viewType))
                                _cache[viewType] = type;
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                }
            }
        }
    }

    // ── BusinessPartLogic 基类 ──

    /// <summary>
    /// Part 业务逻辑基类（非泛型，框架内部使用）
    ///
    /// 与 FormLogic 对称：OnCreate 时绑定 PartView（存在则直接绑定，不存在则加载预制体）。
    /// Part 必须依附于 Form（或父 Part）存在。
    /// </summary>
    public abstract class BusinessPartLogic : BusinessPartLogicContainer
    {
        /// <summary>
        /// 预制体路径（子类必须实现，用于动态加载 Part）
        /// </summary>
        public abstract string PrefabPath { get; }

        /// <summary>
        /// 初始化参数（可选）
        /// </summary>
        public virtual object InitParam => null;

        /// <summary>
        /// 父节点 Transform（在 OnCreate 之前设置）
        /// - 静态 Part：指向 PartView 所在 GameObject 的 transform
        /// - 动态 Part：指向实例化目标的 transform
        /// </summary>
        public Transform ParentTransform { get; set; }

        /// <summary>
        /// 绑定的 PartView
        /// </summary>
        public PartViewBase BoundView { get; private set; }

        /// <summary>
        /// 实例化的 Part GameObject（仅动态 Part，静态 Part 为 null）
        /// </summary>
        public GameObject PartGameObject { get; private set; }

        // ===== 框架生命周期 =====

        protected override void OnCreateInternal()
        {
            base.OnCreateInternal();
            LoadAndBindPart();
            OnPartCreated();
            // 自动发现并初始化子 Part
            var root = BoundView != null ? BoundView.transform :
                       (PartGameObject != null ? PartGameObject.transform : null);
            if (root != null)
                DiscoverChildPartLogics(root);
        }

        protected override void OnDestroyInternal()
        {
            OnPartDestroying();

            if (BoundView != null)
                UnbindView();

            if (PartGameObject != null)
            {
                Object.Destroy(PartGameObject);
                PartGameObject = null;
            }

            base.OnDestroyInternal();
        }

        // ── Part 加载（按需） ──

        private void LoadAndBindPart()
        {
            if (ParentTransform == null)
            {
                Debug.LogWarning($"[BusinessPartLogic] ParentTransform is null for {GetType().Name}. "
                    + "Use CreateLogic<T>(p => p.ParentTransform = ...) to set it.");
                return;
            }

            // 静态 Part：PartView 已存在于父节点的 GameObject 上
            BoundView = ParentTransform.GetComponent<PartViewBase>();
            if (BoundView != null)
            {
                BindView(BoundView);
                return;
            }

            // 动态 Part：加载预制体并实例化
            if (string.IsNullOrEmpty(PrefabPath))
            {
                Debug.LogWarning($"[BusinessPartLogic] No PartView found at parent and PrefabPath is empty: {GetType().Name}");
                return;
            }

            var prefab = CResourceManager.Instance.LoadSync<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[BusinessPartLogic] Failed to load prefab: {PrefabPath}");
                return;
            }

            PartGameObject = Object.Instantiate(prefab, ParentTransform);
            BoundView = PartGameObject.GetComponent<PartViewBase>();

            if (BoundView != null)
                BindView(BoundView);
            else
                Debug.LogWarning($"[BusinessPartLogic] Prefab '{PrefabPath}' has no PartViewBase component.");
        }

        // ===== View 绑定 =====

        private void BindView(PartViewBase view)
        {
            OnViewBound(view);
        }

        private void UnbindView()
        {
            OnViewUnbound(BoundView);
            BoundView = null;
        }

        // ===== Part 生命周期回调（供子类重写） =====

        protected virtual void OnPartCreated() { }
        protected virtual void OnPartDestroying() { }
        protected virtual void OnViewBound(PartViewBase view) { }
        protected virtual void OnViewUnbound(PartViewBase view) { }
    }

    // ── BusinessPartLogic<TView> 泛型基类 ──

    /// <summary>
    /// Part 业务逻辑泛型基类
    /// 声明此 PartLogic 绑定的 PartView 类型，用于容器自动发现。
    ///
    /// 使用方式：
    ///   public class HpBarPartLogic : BusinessPartLogic&lt;HpBarPartView&gt; { ... }
    /// </summary>
    public abstract class BusinessPartLogic<TView> : BusinessPartLogic where TView : PartViewBase
    {
        /// <summary>
        /// 绑定的 PartView（强类型）
        /// </summary>
        public new TView BoundView => base.BoundView as TView;
    }
}
