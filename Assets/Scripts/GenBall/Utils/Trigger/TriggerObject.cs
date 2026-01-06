using UnityEngine;
using UnityEngine.Events;

namespace GenBall.Utils.Trigger
{
    [RequireComponent(typeof(Collider))]
    public class TriggerObject : MonoBehaviour
    {
        public UnityEvent  onTriggerEnter;
        public UnityEvent onTriggerStay;
        public UnityEvent onTriggerExit;
        public LayerMask targetLayerMask;

        private void OnTriggerEnter(Collider other)
        {
            if (targetLayerMask.Contain(other.gameObject.layer))
            {
                onTriggerEnter?.Invoke();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (targetLayerMask.Contain(other.gameObject.layer))
            {
                onTriggerStay?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (targetLayerMask.Contain(other.gameObject.layer))
            {
                onTriggerExit?.Invoke();
            }
        }
        
    }

    public static class LayerMaskExtensions
    {
        public static bool Contain(this LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;
    }
}