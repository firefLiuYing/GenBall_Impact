using System;
using GenBall.Event.Generated;
using GenBall.Procedure.Game;
using UnityEngine;

namespace GenBall.BattleSystem.Mover
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : MonoBehaviour
    {
        private static bool IsPaused => (PauseManager.Instance.State & PauseState.PhysicsPaused) == PauseState.PhysicsPaused;
        private Rigidbody _rigidbody;
        private bool _lastCommandIsPause = false;
        public RigidbodyConstraints Constraints{get=>_rigidbody.constraints; set=>_rigidbody.constraints = value;}
        public bool UseGravity{get=>_rigidbody.useGravity; set=>_rigidbody.useGravity = value;}
        public void SetVelocity(Vector3 velocity)
        {
            if(IsPaused) return;
            _rigidbody.velocity = velocity;
        }

        public void SetPosition(Vector3 position)
        {
            if(IsPaused) return;
            _rigidbody.MovePosition(position);
        }

        public void SetRotation(Quaternion rotation)
        {
            if(IsPaused) return;
            _rigidbody.MoveRotation(rotation);
        }

        public void AddForce(Vector3 force, ForceMode forceMode=ForceMode.Force)
        {
            if(IsPaused) return;
            _rigidbody.AddForce(force, forceMode);
        }

        private Vector3 _storedVelocity = Vector3.zero;
        private Vector3 _storedAngularVelocity = Vector3.zero;
        private bool _storedIsKinematic = false;
        private void OnPause()
        {
            if(_lastCommandIsPause)  return;
            _storedVelocity=_rigidbody.velocity;
            _storedAngularVelocity=_rigidbody.angularVelocity;
            _storedIsKinematic=_rigidbody.isKinematic;
            
            _rigidbody.velocity=Vector3.zero;
            _rigidbody.angularVelocity=Vector3.zero;
            _rigidbody.isKinematic=true;
            
            _lastCommandIsPause=true;
        }

        private void OnResume()
        {
            if(!_lastCommandIsPause) return;
            
            _rigidbody.isKinematic=_storedIsKinematic;
            _rigidbody.velocity=_storedVelocity;
            _rigidbody.angularVelocity=_storedAngularVelocity;
            
            _lastCommandIsPause=false;
        }
        private void OnPauseStateChanged(PauseState state)
        {
            if ((state & PauseState.PhysicsPaused) == PauseState.PhysicsPaused)
            {
                OnPause();
            }
            else
            {
                OnResume();
            }
        }
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            GameEntry.Event.SubscribeSystemPause(OnPauseStateChanged);
        }

        private void OnDisable()
        {
            GameEntry.Event.UnsubscribeSystemPause(OnPauseStateChanged);
        }
    }
}