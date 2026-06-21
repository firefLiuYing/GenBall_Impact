using System;
using GenBall.Interact;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Event
{
    /// <summary>
    /// Small bridge component used by RuntimeEventTrigger in Interact mode.
    /// Implements IInteractable so the player's interaction system can activate
    /// the trigger. Delegates to a plain Action callback.
    /// </summary>
    public class RuntimeInteractProxy : MonoBehaviour, IInteractable
    {
        /// <summary>Called when the player interacts with this object.</summary>
        public event Action OnInteract;

        public string OperationDescription => "Interact";

        public void Register()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>()?.AddInteractable(this);
        }

        public void Unregister()
        {
            SystemRepository.Instance.GetSystem<IInteractSystem>()?.RemoveInteractable(this);
        }

        public void Interact()
        {
            OnInteract?.Invoke();
        }
    }
}
