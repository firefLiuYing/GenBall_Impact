using GenBall.Event;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem.Mover
{
    /// <summary>
    /// Single source of truth for Rigidbody velocity.
    /// All movement executors read Velocity and write via SetVelocity().
    /// SetVelocity immediately updates both the cached Velocity and Rigidbody.velocity,
    /// so reads within the same frame are always consistent.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private bool _isPhysicsPaused;

        /// <summary>
        /// Current velocity. Always up-to-date — reads the value set by the last
        /// SetVelocity call (or the Rigidbody initial velocity before any calls).
        /// </summary>
        public Vector3 Velocity { get; private set; }

        private Rigidbody Rigidbody
        {
            get
            {
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();
                return _rigidbody;
            }
        }

        public RigidbodyConstraints Constraints { get => Rigidbody.constraints; set => Rigidbody.constraints = value; }
        public bool UseGravity { get => Rigidbody.useGravity; set => Rigidbody.useGravity = value; }

        public void SetVelocity(Vector3 velocity)
        {
            if (_isPhysicsPaused) return;
            Velocity = velocity;
            Rigidbody.velocity = velocity;
        }

        public void SetPosition(Vector3 position)
        {
            if (_isPhysicsPaused) return;
            Rigidbody.MovePosition(position);
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_isPhysicsPaused) return;
            Rigidbody.MoveRotation(rotation);
        }

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            if (_isPhysicsPaused) return;
            Rigidbody.AddForce(force, forceMode);
        }

        private Vector3 _storedVelocity = Vector3.zero;
        private Vector3 _storedAngularVelocity = Vector3.zero;
        private bool _storedIsKinematic = false;

        private void OnPauseChanged()
        {
            var ps = SystemRepository.Instance.GetSystem<IPauseSystem>();
            bool shouldPause = ps != null && ps.IsPhysicsPaused;

            if (shouldPause && !_isPhysicsPaused)
            {
                _storedVelocity = Rigidbody.velocity;
                _storedAngularVelocity = Rigidbody.angularVelocity;
                _storedIsKinematic = Rigidbody.isKinematic;

                Rigidbody.velocity = Vector3.zero;
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.isKinematic = true;
            }
            else if (!shouldPause && _isPhysicsPaused)
            {
                Rigidbody.isKinematic = _storedIsKinematic;
                Rigidbody.velocity = _storedVelocity;
                Rigidbody.angularVelocity = _storedAngularVelocity;
            }

            _isPhysicsPaused = shouldPause;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            Velocity = _rigidbody.velocity;
        }

        private void OnEnable()
        {
            CEventRouter.Instance.Subscribe((int)GlobalEventId.PauseChanged, OnPauseChanged);
        }

        private void OnDisable()
        {
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.PauseChanged, OnPauseChanged);
        }
    }
}
