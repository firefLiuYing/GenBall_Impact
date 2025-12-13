using System;
using UnityEngine;

namespace GenBall.Enemy.Detect
{
    public class NormalDetectModule : DetectModule
    {
        [Header("ÓÎµ´Ì½²é·¶Î§")][SerializeField] private float wanderDetectRange;
        [Header("³ðºÞ·¶Î§")][SerializeField] private float hateRange;
        [Header("¹¥»÷¾àÀë")] [SerializeField] private float attackRange;
        [Header("Ä¿±ê²ã¼¶")][SerializeField] private LayerMask targetLayer;
        public override void Initialize()
        {
            
        }

        public override void ModuleUpdate(float deltaTime)
        {
            
        }

        public override void ModuleFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public override void OnRecycle()
        {
            
        }

        public override void Search(Action<Player.Player> findCallback)
        {
            var colliders = Physics.OverlapSphere(transform.position, wanderDetectRange, targetLayer);
            Player.Player target = null;
            foreach (var col in colliders)
            {
                target = col.GetComponentInParent<Player.Player>();
                if(target != null) break;
            }

            if (target != null)
            {
                // Debug.Log("ÕÒµ½ÁË");
                // Debug.Log($"{Vector3.Distance(transform.position, target.transform.position)}");
                findCallback?.Invoke(target);
            }
        }

        public override bool InHateRange()
        {
            if (Owner.Target == null) return false;
            var distance = Vector3.Distance(Owner.Target.transform.position, transform.position);
            return distance <= hateRange;
        }

        public override float GetTargetDistance()
        {
            if (Owner.Target == null) return Mathf.Infinity;
            return Vector3.Distance(Owner.Target.transform.position, transform.position);
        }
    }
}