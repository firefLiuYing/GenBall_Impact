using System;
using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    public class CapsuleColliderGroundDetect : MonoBehaviour, ICharacterGroundDetect
    {
        public bool IsOnGround { get; private set;}
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
            var hit=Physics.Raycast(origin,Vector3.down,_collider.height/2+0.01f,groundDetectLayerMask);
            IsOnGround = hit;
        }
    }
}