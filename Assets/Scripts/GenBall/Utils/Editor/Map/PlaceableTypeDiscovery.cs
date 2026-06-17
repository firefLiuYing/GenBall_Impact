#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GenBall.Map;
using UnityEngine;

namespace GenBall.Utils.Editor.Map
{
    /// <summary>
    /// Reflection-based discovery of all IScenePlaceable implementation types.
    /// </summary>
    public static class PlaceableTypeDiscovery
    {
        /// <summary>
        /// Info about a discovered placeable type.
        /// </summary>
        public class PlaceableTypeInfo
        {
            public Type Type;
            public PlaceableCategoryAttribute CategoryAttribute;
            public PlaceablePrefabAttribute PrefabAttribute;
        }

        private static List<PlaceableTypeInfo> _cached;

        /// <summary>
        /// Returns all MonoBehaviour types that implement IScenePlaceable and have [PlaceableCategory].
        /// Results are cached after first call.
        /// </summary>
        public static List<PlaceableTypeInfo> DiscoverAll()
        {
            if (_cached != null) return _cached;

            _cached = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException) { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract
                    && typeof(MonoBehaviour).IsAssignableFrom(t)
                    && typeof(IScenePlaceable).IsAssignableFrom(t))
                .Select(t => new PlaceableTypeInfo
                {
                    Type = t,
                    CategoryAttribute = t.GetCustomAttribute<PlaceableCategoryAttribute>(),
                    PrefabAttribute = t.GetCustomAttribute<PlaceablePrefabAttribute>(),
                })
                .Where(info => info.CategoryAttribute != null)
                .OrderBy(info => info.CategoryAttribute.Order)
                .ThenBy(info => info.CategoryAttribute.DisplayName)
                .ToList();

            return _cached;
        }

        /// <summary>
        /// Clear the cache (call after domain reload or assembly changes).
        /// </summary>
        public static void ClearCache() => _cached = null;
    }
}
#endif
