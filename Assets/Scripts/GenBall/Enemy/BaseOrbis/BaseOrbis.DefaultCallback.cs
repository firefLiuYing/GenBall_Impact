using System;
using GenBall.BattleSystem;
using UnityEngine;

namespace GenBall.Enemy.BaseOrbis
{
    public partial class BaseOrbis
    {
        [SerializeField] protected float detectionRadius;
        [SerializeField] protected LayerMask detectionLayer;
        [SerializeField] protected float speed;
        [SerializeField] protected float attackRadius;
        protected Rigidbody _rigidbody;

        protected void DetectPlayer()
        {
            var hits=Physics.SphereCastAll(transform.position, detectionRadius,Vector3.forward,detectionRadius,detectionLayer);
            foreach (var hit in hits)
            {
                if (hit.transform.TryGetComponent<Player.Player>(out var player))
                {
                    SetTarget(player);
                }
            }
        }
        #region Wander
        protected void DefaultWanderFixedUpdate(float deltaTime)
        {
            DetectPlayer();
        }

        protected void DefaultWanderUpdate(float deltaTime)
        {
            
        }

        protected void DefaultWanderHandle(IInteractToken stimulus,out IInteractToken[] responses)
        {
            responses = Array.Empty<IInteractToken>();
        }
        #endregion

        #region Chase

        protected void DefaultChaseFixedUpdate(float deltaTime)
        {
            if (Target is MonoBehaviour monoBehaviour)
            {
                var direction = monoBehaviour.transform.position - transform.position;
                var distance = direction.magnitude;
                direction.y = 0;
                direction.Normalize();
                _rigidbody.velocity=new Vector3(direction.x*speed,_rigidbody.velocity.y,direction.z*speed);

                if (distance < attackRadius)
                {
                    InAttackRadius.PostValue(true);
                }
            }
        }

        protected void DefaultChaseUpdate(float deltaTime)
        {
            
        }

        protected void DefaultChaseHandle(IInteractToken stimulus, out IInteractToken[] responses)
        {
            responses = Array.Empty<IInteractToken>();
        }

        #endregion


        #region Attack

        protected void DefaultAttackFixedUpdate(float deltaTime)
        {
            if (Target is MonoBehaviour monoBehaviour)
            {
                var direction = monoBehaviour.transform.position - transform.position;
                var distance = direction.magnitude;
                direction.y = 0;
                direction.Normalize();
                _rigidbody.velocity=Vector3.zero;

                if (distance > attackRadius)
                {
                    InAttackRadius.PostValue(false);
                }
            }
        }

        protected void DefaultAttackUpdate(float deltaTime)
        {
            
        }

        protected void DefaultAttackHandle(IInteractToken stimulus, out IInteractToken[] responses)
        {
            responses = Array.Empty<IInteractToken>();
        }

        #endregion


        #region Return

        protected void DefaultReturnFixedUpdate(float deltaTime)
        {
            
        }

        protected void DefaultReturnUpdate(float deltaTime)
        {
            
        }

        protected void DefaultReturnHandle(IInteractToken stimulus, out IInteractToken[] responses)
        {
            responses = Array.Empty<IInteractToken>();
        }

        #endregion
        
    }
}