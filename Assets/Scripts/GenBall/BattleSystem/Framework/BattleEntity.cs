using System;
using System.Collections.Generic;
using GenBall.Framework.Entity;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework
{
    public class BattleEntity : MonoBehaviour
    {
        private readonly Dictionary<Type, object> _components = new();

        /// <summary>
        /// Register a component by its concrete type. Auto-registers IEntityFrameUpdate/IEntityLogicUpdate.
        /// </summary>
        public void RegisterComponent<T>(T component) where T : class
        {
            RegisterByType(typeof(T), component);
        }

        /// <summary>
        /// Register the same component instance under an additional type key (interface or base class).
        /// Use when a component needs to be retrieved by an interface via Get/Iinterface/ or TryGet.
        /// Does not double-register IEntityFrameUpdate/IEntityLogicUpdate for the same instance.
        /// </summary>
        public void RegisterComponentAs<TInterface>(object component) where TInterface : class
        {
            RegisterByType(typeof(TInterface), component);
        }

        private readonly HashSet<object> _registeredUpdaters = new();
        private void RegisterByType(Type type, object component)
        {
            _components[type] = component;

            // Only register update interfaces once per component instance
            if (_registeredUpdaters.Add(component))
            {
                var updateSystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
                if (updateSystem != null)
                {
                    if (component is IEntityFrameUpdate frameUpd)
                        updateSystem.AddFrameUpdate(frameUpd);
                    if (component is IEntityLogicUpdate logicUpd)
                        updateSystem.AddLogicUpdate(logicUpd);
                }
            }
        }

        /// <summary>Get a registered component by type. Returns null if not found.</summary>
        public T Get<T>() where T : class
        {
            _components.TryGetValue(typeof(T), out var component);
            return component as T;
        }

        /// <summary>Try to get a registered component by type.</summary>
        public bool TryGet<T>(out T component) where T : class
        {
            if (_components.TryGetValue(typeof(T), out var obj))
            {
                component = obj as T;
                return true;
            }
            component = null;
            return false;
        }

        /// <summary>Check if a component type is registered.</summary>
        public bool Has<T>() where T : class
        {
            return _components.ContainsKey(typeof(T));
        }

        private void OnDestroy()
        {
            // Clean up any decision layer FSM before removing update registrations
            if (_components.TryGetValue(typeof(EnemyDecisionLayer), out var layer) && layer is EnemyDecisionLayer decisionLayer)
                decisionLayer.Cleanup();

            var updateSystem = SystemRepository.Instance.GetSystem<IEntityUpdateSystem>();
            if (updateSystem != null)
            {
                foreach (var kvp in _components)
                {
                    if (kvp.Value is IEntityFrameUpdate frameUpd)
                        updateSystem.RemoveFrameUpdate(frameUpd);
                    if (kvp.Value is IEntityLogicUpdate logicUpd)
                        updateSystem.RemoveLogicUpdate(logicUpd);
                }
            }
            _components.Clear();
        }
    }
}
