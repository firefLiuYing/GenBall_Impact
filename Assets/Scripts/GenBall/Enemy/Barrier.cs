using UnityEngine;

namespace GenBall.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class Barrier : MonoBehaviour
    {
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void SetColliderEnable(bool enable) => _collider.enabled = enable;
    }
}