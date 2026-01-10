using GenBall.Event.Generated;
using UnityEngine;
using Yueyn.Base.Variable;

namespace GenBall.Player
{
    public partial class Player
    {
        
        private Rigidbody _rigidbody;
        private CapsuleCollider _collider;
        private Variable<bool> _onGround;
        private void InitPhysics()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _collider = GetComponentInChildren<CapsuleCollider>();
        }
        private void PhysicsUpdate()
        {
            GroundDetection();
            
            GameEntry.Event.FirePlayerPosition(transform);
        }

        [SerializeField]private LayerMask groundDetectLayerMask; 
        private void GroundDetection()
        {
            var origin = transform.position + _collider.center;
            var hit=Physics.Raycast(origin,Vector3.down,_collider.height/2+0.01f,groundDetectLayerMask);
            // Debug.Log($"µÿ√ÊºÏ≤‚£∫{hit}");
            if(hit!=_onGround.Value) _onGround.PostValue(hit);
            // _onGround.PostValue(hit);
        }
    }
}