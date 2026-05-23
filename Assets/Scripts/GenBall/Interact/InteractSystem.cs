using System.Collections.Generic;
using Yueyn.Base.Variable;

namespace GenBall.Interact
{
    public class InteractSystem : IInteractSystem
    {
        private readonly List<IInteractable> _interactables = new();
        public Variable<List<IInteractable>> Interactables { get; }
        public Variable<int> CurrentSelectionIndex { get; }

        public InteractSystem()
        {
            Interactables = Variable<List<IInteractable>>.Create();
            CurrentSelectionIndex = Variable<int>.Create();
            Interactables.SetValue(_interactables);
        }

        public void Init() { }

        public void UnInit()
        {
            _interactables.Clear();
        }

        public void NextSelection()
        {
            if(_interactables.Count<=0) return;
            var index = CurrentSelectionIndex.Value;
            index++;
            index %= _interactables.Count;
            CurrentSelectionIndex.PostValue(index);
        }

        public void LastSelection()
        {
            if(_interactables.Count<=0) return;
            var index = CurrentSelectionIndex.Value;
            index--;
            index+=_interactables.Count;
            index %= _interactables.Count;
            CurrentSelectionIndex.PostValue(index);
        }

        public void TriggerInteractable()
        {
            var index = CurrentSelectionIndex.Value;
            if (index < _interactables.Count && index >= 0)
            {
                _interactables[index].Interact();
            }
        }
        public void AddInteractable(IInteractable interactable)
        {
            if(_interactables.Contains(interactable))  return;
            _interactables.Add(interactable);
            Interactables.PostValue();
            if (_interactables.Count == 1)
            {
                CurrentSelectionIndex.PostValue(0);
            }
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if(!_interactables.Contains(interactable)) return;
            _interactables.Remove(interactable);
            Interactables.PostValue();
            if (_interactables.Count >= CurrentSelectionIndex.Value)
            {
                CurrentSelectionIndex.PostValue(0);
            }
        }
    }
    
}