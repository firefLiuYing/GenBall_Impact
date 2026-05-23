using System.Collections.Generic;
using Yueyn.Base.Variable;
using Yueyn.Main;

namespace GenBall.Interact
{
    public interface IInteractSystem : ISystem
    {
        Variable<List<IInteractable>> Interactables { get; }
        Variable<int> CurrentSelectionIndex { get; }
        void NextSelection();
        void LastSelection();
        void TriggerInteractable();
        void AddInteractable(IInteractable interactable);
        void RemoveInteractable(IInteractable interactable);
    }
}
