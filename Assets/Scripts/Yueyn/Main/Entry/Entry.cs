using System;
using System.Collections.Generic;

namespace Yueyn.Main.Entry
{
    public static class Entry
    {
        private static readonly Dictionary<Type,IComponent> _components=new();

        public static void Update(float elapsedSeconds, float realElapseSeconds)
        {
            foreach (var component in _components.Values)
            {
                component.Update(elapsedSeconds, realElapseSeconds);
            }
        }

        public static void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var component in _components.Values)
            {
                component.FixedUpdate(fixedDeltaTime);
            }
        }
        public static T GetComponent<T>() where T : IComponent
        {
            _components.TryGetValue(typeof(T), out IComponent component);
            return (T)component;
        }

        public static void Register(IComponent component)
        {
            component.OnRegister();
            _components.Add(component.GetType(), component);
        }

        public static void Unregister(IComponent component)
        {
            component.OnUnregister();
            _components.Remove(component.GetType());
        }
    }
}