using System;
using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    /// <summary>
    /// Data source: detects ground contact using capsule collider physics.
    /// Not part of the three decision/dispatch/execute layers — provides IsOnGround data.
    /// </summary>
    public class GroundDetector : MonoBehaviour, ICharacterGroundDetect
    {
        public bool IsOnGround { get; private set; }
        [SerializeField] LayerMask groundDetectLayerMask;
        private CapsuleCollider _collider;

        private void Awake()
        {
            _collider = GetComponentInChildren<CapsuleCollider>();
        }

        private void FixedUpdate()
        {
            GroundDetection();
        }

        private void GroundDetection()
        {
            var origin = transform.position + _collider.center;
            var sphereCenter = origin + transform.up * (-_collider.height * 0.5f + _collider.radius);
            var sphereHit = Physics.OverlapSphere(sphereCenter, _collider.radius + 0.01f, groundDetectLayerMask);
            var hit = Physics.Raycast(origin, -transform.up, _collider.height / 2 + 0.414f * _collider.radius, groundDetectLayerMask);
            IsOnGround = hit && sphereHit.Length > 0;
        }
    }
}
