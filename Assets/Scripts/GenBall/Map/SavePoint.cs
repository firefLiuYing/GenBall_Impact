using System.Collections.Generic;
using GenBall.CombatState;
using GenBall.Event;
using GenBall.Interact;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Map
{
    public class SavePoint : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private List<EventAdapter> _interactEvents = new();

        private ICombatStateSystem _combatStateSystem;

        [SerializeField]
        private string _displayName;

        /// <summary>
        /// Injected by SpawnBonfires during scene initialization.
        /// </summary>
        public void SetConfig(string displayName)
        {
            _displayName = displayName;
        }

        private void Awake()
        {
            _combatStateSystem = SystemRepository.Instance.GetSystem<ICombatStateSystem>();
        }

        public string OperationDescription => _displayName;
        public bool CanInteract => _combatStateSystem != null && !_combatStateSystem.IsInCombat;

        public void Interact()
        {
            foreach (var evt in _interactEvents)
            {
                evt?.Fire();
            }
            Debug.Log($"[SavePoint] Interacted: {_displayName}");
        }

        public void OnFocused()
        {
            Debug.Log($"[SavePoint] Focused: {_displayName}");
        }

        public void OnUnfocused()
        {
            Debug.Log($"[SavePoint] Unfocused: {_displayName}");
        }
    }
}
