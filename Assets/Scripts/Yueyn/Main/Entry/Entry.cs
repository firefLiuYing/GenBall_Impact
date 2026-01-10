using System;
using System.Collections.Generic;
using System.Linq;

namespace Yueyn.Main.Entry
{
    public class Entry
    {
        private readonly Dictionary<Type,IComponent> _components=new();

        public void Update(float elapsedSeconds, float realElapseSeconds)
        {
            foreach (var component in _components.Values)
            {
                component.ComponentUpdate(elapsedSeconds, realElapseSeconds);
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var component in _components.Values)
            {
                component.ComponentFixedUpdate(fixedDeltaTime);
            }
        }
        public T GetComponent<T>() where T : IComponent
        {
            _components.TryGetValue(typeof(T), out IComponent component);
            return (T)component;
        }

        public void Register(IComponent component)
        {
            // component.OnRegister();
            _components.Add(component.GetType(), component);
            
        }

        private readonly List<IComponent> _cachedComponents = new List<IComponent>();
        public void Initialize()
        {
            _cachedComponents.Clear();
            _cachedComponents.AddRange(_components.Values.OrderBy(c=>c.Priority));
            foreach (var component in _cachedComponents)
            {
                component.Init();
            }
        }
        public void Unregister(IComponent component)
        {
            component.OnUnregister();
            _components.Remove(component.GetType());
        }
    }
}