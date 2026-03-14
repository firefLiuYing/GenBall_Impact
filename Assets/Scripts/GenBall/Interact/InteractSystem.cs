using System.Collections.Generic;
using GenBall.Utils.Singleton;
using Yueyn.Base.Variable;

namespace GenBall.Interact
{
    public class InteractSystem : ISingleton
    {
        public static InteractSystem Instance => SingletonManager.GetSingleton<InteractSystem>();

        private readonly List<IInteractable> _interactables = new();
        public readonly Variable<List<IInteractable>> Interactables;

        public InteractSystem()
        {
            Interactables=Variable<List<IInteractable>>.Create();
            Interactables.SetValue(_interactables);
        }
        public void AddInteractable(IInteractable interactable)
        {
            _interactables.Add(interactable);
            Interactables.PostValue();
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            _interactables.Remove(interactable);
            Interactables.PostValue();
        }
    }
    
}