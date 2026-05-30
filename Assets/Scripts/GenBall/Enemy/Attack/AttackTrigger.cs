using System;
using UnityEngine;

namespace GenBall.Enemy.Attack
{
    [RequireComponent(typeof(Collider))]
    public class AttackTrigger : MonoBehaviour
    {
        private Collider _collider;
        private GameObject _owner;
        private bool _triggered;
        public event Action<GameObject> OnHit;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        public void Init(GameObject owner)
        {
            _owner = owner;
        }

        public void StartDetect()
        {
            _collider.enabled = true;
            _triggered = false;
        }

        public void StopDetect()
        {
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (other.gameObject == _owner) return;
            if (!other.CompareTag("Player")) return;
            _triggered = true;
            Debug.Log($"[AttackTrigger] HIT! target={other.gameObject.name}");
            OnHit?.Invoke(other.gameObject);
        }
    }
}
