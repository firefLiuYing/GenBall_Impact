using UnityEngine;

namespace GenBall.Enemy.Move
{
    public class JumpMoveModule : MoveModule
    {
        private Rigidbody _rigidbody;
        private Vector3 _target;
        public override void Initialize()
        {
            _rigidbody=GetComponent<Rigidbody>();
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

        public override void MoveTo(Vector3 target)
        {
            Debug.Log($"œÎ»•{target}");
            _target = target;
            // todo gzp ≤‚ ‘¥˙¬Î
            _rigidbody.AddForce(_target*100);
        }
    }
}