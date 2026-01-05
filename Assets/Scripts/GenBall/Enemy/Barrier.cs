using UnityEngine;

namespace GenBall.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class Barrier : Module
    {
        private Collider _collider;

        public override void Initialize()
        {
            _collider=GetComponent<Collider>();
        }
        public void SetColliderEnable(bool enable)=>_collider.enabled=enable;

        public override void OnRecycle()
        {
            
        }
    }
}