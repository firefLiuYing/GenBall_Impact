using System.Collections.Generic;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using GenBall.Interact;
using UnityEngine;
using Unity.VisualScripting;
using Yueyn.Main;

namespace GenBall.BattleSystem.Executors
{
    /// <summary>
    /// Execute layer: handles InteractCommand for player interaction.
    /// Replaces the old InteractController (CharacterControllerBase).
    /// </summary>
    public class PlayerInteractExecutor : IInteract, IEntityLogicUpdate
    {
        private readonly IInteractSystem _interactSystem;

        // Sight detection
        private readonly Camera _camera;
        private readonly float _sightDetectRadius;
        private readonly float _sightDetectDistance;
        private readonly LayerMask _interactableLayer;

        private readonly HashSet<IInteractable> _lastFrameInteractables = new();
        private readonly HashSet<IInteractable> _thisFrameInteractables = new();

        public PlayerInteractExecutor(IInteractSystem interactSystem, Camera camera,
            float sightDetectRadius, float sightDetectDistance, LayerMask interactableLayer)
        {
            _interactSystem = interactSystem;
            _camera = camera;
            _sightDetectRadius = sightDetectRadius;
            _sightDetectDistance = sightDetectDistance;
            _interactableLayer = interactableLayer;
        }

        public void Interact(InteractCommand cmd)
        {
            switch (cmd.Action)
            {
                case InteractAction.Trigger:
                    _interactSystem.TriggerInteractable();
                    break;
                case InteractAction.Next:
                    _interactSystem.NextSelection();
                    break;
                case InteractAction.Previous:
                    _interactSystem.LastSelection();
                    break;
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            DetectSightInteract();
        }

        private void DetectSightInteract()
        {
            _lastFrameInteractables.AddRange(_thisFrameInteractables);
            _thisFrameInteractables.Clear();

            if (_camera == null) return;

            var hits = Physics.SphereCastAll(_camera.transform.position, _sightDetectRadius,
                _camera.transform.forward, _sightDetectDistance, _interactableLayer);

            foreach (var hit in hits)
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>()
                    ?? hit.collider.GetComponentInChildren<IInteractable>();
                if (interactable != null)
                {
                    _thisFrameInteractables.Add(interactable);
                }
            }

            foreach (var lastFrameInteractable in _lastFrameInteractables)
            {
                if (!_thisFrameInteractables.Contains(lastFrameInteractable))
                {
                    _interactSystem.RemoveInteractable(lastFrameInteractable);
                }
            }

            foreach (var thisFrameInteractable in _thisFrameInteractables)
            {
                if (!_lastFrameInteractables.Contains(thisFrameInteractable))
                {
                    _interactSystem.AddInteractable(thisFrameInteractable);
                }
            }

            _lastFrameInteractables.Clear();
        }
    }
}
