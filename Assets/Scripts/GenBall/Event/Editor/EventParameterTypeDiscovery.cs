#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GenBall.Event.Editor
{
    /// <summary>
    /// Reflection-based discovery of all EventParameterBase subclasses.
    /// Provides type suggestions keyed by event ID via [EventParamHint] attributes.
    /// </summary>
    public static class EventParameterTypeDiscovery
    {
        private static List<Type> _cached;

        /// <summary>
        /// Returns all non-abstract types deriving from EventParameterBase.
        /// Results are cached after first call.
        /// </summary>
        public static List<Type> DiscoverAll()
        {
            if (_cached != null) return _cached;

            _cached = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException) { return Array.Empty<Type>(); }
                })
                .Where(t => !t.IsAbstract
                    && typeof(EventParameterBase).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

            return _cached;
        }

        /// <summary>
        /// Returns parameter types that have an [EventParamHint] matching the given eventId.
        /// If none match, returns all types (the hint is a soft constraint).
        /// </summary>
        public static List<Type> GetSuggestedTypes(int eventId)
        {
            var all = DiscoverAll();
            var hinted = all.Where(t => t.GetCustomAttributes<EventParamHintAttribute>()
                .Any(a => a.EventId == eventId)).ToList();
            return hinted.Count > 0 ? hinted : all;
        }

        /// <summary>
        /// Clear the cache (call after domain reload or assembly changes).
        /// </summary>
        public static void ClearCache() => _cached = null;
    }
}
#endif
