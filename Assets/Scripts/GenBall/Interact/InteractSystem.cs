using System.Collections.Generic;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Main;

namespace GenBall.Interact
{
    public class InteractSystem : IInteractSystem, IFrameUpdate
    {
        private readonly List<IInteractable> _interactables = new();
        private readonly List<IInteractable> _candidatesThisFrame = new();

        // Cone detection config
        private float _coneHalfAngle = 30f;
        private float _maxDistance = 3f;
        private LayerMask _interactableLayer;

        // Sticky selection
        private IInteractable _currentSelection;

        public Variable<List<IInteractable>> Interactables { get; }
        public Variable<int> CurrentSelectionIndex { get; }
        public SystemScope FrameUpdateScope => SystemScope.Game;

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
            _candidatesThisFrame.Clear();
            _currentSelection = null;
        }

        public void Configure(float coneHalfAngle, float maxDistance, LayerMask interactableLayer)
        {
            _coneHalfAngle = coneHalfAngle;
            _maxDistance = maxDistance;
            _interactableLayer = interactableLayer;
        }

        public void FrameUpdate(float deltaTime)
        {
            var camera = SystemRepository.Instance.GetSystem<GameCamera.ICameraSystem>()?.MainCamera;
            if (camera == null) return;

            var cameraTransform = camera.transform;
            var camPos = cameraTransform.position;
            var camForward = cameraTransform.forward;

            var layerMask = _interactableLayer.value != 0
                ? _interactableLayer
                : (LayerMask)Physics.DefaultRaycastLayers;

            // 1. OverlapSphere for initial broad detection
            var hits = Physics.OverlapSphere(camPos, _maxDistance, layerMask);

            // 2. Collect + cone filter + CanInteract filter
            _candidatesThisFrame.Clear();
            var cosHalfAngle = Mathf.Cos(_coneHalfAngle * Mathf.Deg2Rad);

            foreach (var col in hits)
            {
                var toTarget = col.transform.position - camPos;
                var dist = toTarget.magnitude;
                if (dist > _maxDistance) continue;

                // Cone check: angle between forward and target <= half angle
                var toTargetNorm = toTarget / dist;
                if (Vector3.Dot(camForward, toTargetNorm) < cosHalfAngle) continue;

                var interactable = col.GetComponentInParent<IInteractable>()
                    ?? col.GetComponentInChildren<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    if (!_candidatesThisFrame.Contains(interactable))
                        _candidatesThisFrame.Add(interactable);
                }
            }

            // 3. Sort by distance (closest first)
            _candidatesThisFrame.Sort((a, b) =>
            {
                var mbA = a as MonoBehaviour;
                var mbB = b as MonoBehaviour;
                float distA = mbA != null ? (mbA.transform.position - camPos).sqrMagnitude : float.MaxValue;
                float distB = mbB != null ? (mbB.transform.position - camPos).sqrMagnitude : float.MaxValue;
                return distA.CompareTo(distB);
            });

            // 4. Detect list change
            bool listChanged = false;
            if (_interactables.Count != _candidatesThisFrame.Count)
                listChanged = true;
            else
                for (int i = 0; i < _interactables.Count; i++)
                    if (_interactables[i] != _candidatesThisFrame[i])
                    { listChanged = true; break; }

            _interactables.Clear();
            _interactables.AddRange(_candidatesThisFrame);

            // 5. Sticky selection
            int newIndex = 0;
            if (_currentSelection != null)
            {
                int stickyIndex = _interactables.IndexOf(_currentSelection);
                if (stickyIndex >= 0)
                    newIndex = stickyIndex;
            }

            // 6. OnFocused/OnUnfocused
            var newSelection = (_interactables.Count > 0) ? _interactables[newIndex] : null;
            if (_currentSelection != newSelection)
            {
                _currentSelection?.OnUnfocused();
                newSelection?.OnFocused();
                _currentSelection = newSelection;
            }

            // 7. Post changes
            if (listChanged)
                Interactables.PostValue();
            if (CurrentSelectionIndex.Value != newIndex)
                CurrentSelectionIndex.PostValue(newIndex);

            _candidatesThisFrame.Clear();
        }

        public void NextSelection()
        {
            if (_interactables.Count <= 0) return;
            var index = CurrentSelectionIndex.Value;
            index++;
            index %= _interactables.Count;
            CurrentSelectionIndex.PostValue(index);
            SwitchFocus(index);
        }

        public void LastSelection()
        {
            if (_interactables.Count <= 0) return;
            var index = CurrentSelectionIndex.Value;
            index--;
            index += _interactables.Count;
            index %= _interactables.Count;
            CurrentSelectionIndex.PostValue(index);
            SwitchFocus(index);
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
            if (_interactables.Contains(interactable)) return;
            _interactables.Add(interactable);
            Interactables.PostValue();
            if (_interactables.Count == 1)
            {
                CurrentSelectionIndex.PostValue(0);
                SwitchFocus(0);
            }
        }

        public void RemoveInteractable(IInteractable interactable)
        {
            if (!_interactables.Contains(interactable)) return;
            _interactables.Remove(interactable);
            Interactables.PostValue();
            if (CurrentSelectionIndex.Value >= _interactables.Count)
            {
                int newIndex = _interactables.Count > 0 ? 0 : -1;
                CurrentSelectionIndex.PostValue(newIndex);
                if (newIndex >= 0)
                    SwitchFocus(newIndex);
            }
        }

        private void SwitchFocus(int newIndex)
        {
            if (newIndex < 0 || newIndex >= _interactables.Count) return;
            var newSelection = _interactables[newIndex];
            if (_currentSelection != newSelection)
            {
                _currentSelection?.OnUnfocused();
                newSelection?.OnFocused();
                _currentSelection = newSelection;
            }
        }
    }
}
