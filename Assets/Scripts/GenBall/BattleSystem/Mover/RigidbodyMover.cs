using GenBall.Event;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.BattleSystem.Mover
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private bool _isPhysicsPaused;

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
