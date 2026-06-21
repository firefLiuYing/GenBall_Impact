using System;
using GenBall.Interact;
using UnityEngine;

namespace GenBall.Event
{
    /// <summary>
    /// Small bridge component used by RuntimeEventTrigger in Interact mode.
    /// Implements IInteractable so the player's interaction system can activate
    /// the trigger. Delegates to a plain Action callback.
    ///
    /// Sight-based discovery is handled by InteractSystem (IFrameUpdate SphereCast).
    /// </summary>
    public class RuntimeInteractProxy : MonoBehaviour, IInteractable
    {
        /// <summary>Called when the player interacts with this object.</summary>
        public event Action OnInteract;

        public string OperationDescription => "Interact";
        public bool CanInteract => true;

        public void Interact()
        {
            OnInteract?.Invoke();
        }

        public void OnFocused()
        {
            Debug.Log($"[RuntimeInteractProxy] Focused: {gameObject.name}");
        }

        public void OnUnfocused()
        {
            Debug.Log($"[RuntimeInteractProxy] Unfocused: {gameObject.name}");
        }
    }
}
