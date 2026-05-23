using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Character;
using GenBall.Interact;
using GenBall.Player.Input;
using Yueyn.Main;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace GenBall.Player.Controller
{
    public class InteractController : CharacterControllerBase
    {
        private CharacterState _player;
        private InputHandler _input;
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _input = _player.GetComponentInChildren<InputHandler>();
            _input.OnInteract += Interact;
            _input.OnScrollChange+=ChangeSelection;
        }

        private void Interact()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>().TriggerInteractable();
        }

        private void ChangeSelection(float delta)
        {
            if(delta<0) SystemRepository.Instance.GetSystem<IInteractSystem>().NextSelection();
            else SystemRepository.Instance.GetSystem<IInteractSystem>().LastSelection();
        }

        public override void Tick(float deltaTime)
        {
            DetectSightInteract();
        }

        [SerializeField] private float sightDetectRadius;
        [SerializeField] private float sightDetectDistance;
        [SerializeField] private LayerMask interactableLayer;

        private readonly HashSet<IInteractable> _lastFrameInteractables = new();
        private readonly HashSet<IInteractable> _thisFrameInteractables = new();
        private void DetectSightInteract()
        {
            _lastFrameInteractables.AddRange(_thisFrameInteractables);
            _thisFrameInteractables.Clear();
            var hits = Physics.SphereCastAll(Camera.main.transform.position,sightDetectRadius,Camera.main.transform.forward,sightDetectDistance,interactableLayer);
            foreach (var hit in hits)
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>() ?? hit.collider.GetComponentInChildren<IInteractable>();
                if (interactable != null)
                {
                    _thisFrameInteractables.Add(interactable);
                }
            }

            foreach (var lastFrameInteractable in _lastFrameInteractables)
            {
                if (!_thisFrameInteractables.Contains(lastFrameInteractable))
                {
                    SystemRepository.Instance.GetSystem<IInteractSystem>().RemoveInteractable(lastFrameInteractable);
                }
            }

            foreach (var thisFrameInteractable in _thisFrameInteractables)
            {
                if (!_lastFrameInteractables.Contains(thisFrameInteractable))
                {
                    SystemRepository.Instance.GetSystem<IInteractSystem>().AddInteractable(thisFrameInteractable);
                }
            }
            _lastFrameInteractables.Clear();
        }

        private void OnDrawGizmos()
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * sightDetectDistance, Color.green);
        }
    }
}