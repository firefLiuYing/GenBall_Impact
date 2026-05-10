using System;
using GenBall.BattleSystem.Character;
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

        public void Init(GameObject owner)
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
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
            var target = other.GetComponentInParent<CharacterState>();
            if (target == null || target.gameObject == _owner) return;
            _triggered = true;
            OnHit?.Invoke(target.gameObject);
        }
    }
}
