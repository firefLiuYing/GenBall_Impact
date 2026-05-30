using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.Enemy
{
    /// <summary>
    /// Ground detection for sphere-based enemies (Orbis).
    /// Uses SphereCollider radius for raycast distance.
    /// Place on enemy prefab root — finds SphereCollider in children.
    /// </summary>
    public class SphereGroundDetector : MonoBehaviour, ICharacterGroundDetect
    {
        public bool IsOnGround { get; private set; }

        [SerializeField] private LayerMask groundLayerMask = ~0;

        private SphereCollider _collider;
        private LayerMask _effectiveMask;

        private void Awake()
        {
            _collider = GetComponentInChildren<SphereCollider>();

            // Exclude Orbis-related layers from ground detection (same as old JumpMover)
            int exclude = (1 << LayerMask.NameToLayer("Orbis"))
                        | (1 << LayerMask.NameToLayer("OrbisAttack"))
                        | (1 << LayerMask.NameToLayer("Barrier"));
            _effectiveMask = groundLayerMask & ~exclude;
        }

        private void FixedUpdate()
        {
            if (_collider == null)
            {
                IsOnGround = false;
                return;
            }

            var origin = transform.position + _collider.center;
            IsOnGround = Physics.Raycast(origin, Vector3.down, _collider.radius + 0.05f, _effectiveMask);
        }
    }
}
