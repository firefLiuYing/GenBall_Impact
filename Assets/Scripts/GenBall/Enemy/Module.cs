using UnityEngine;

namespace GenBall.Enemy
{
    public abstract class Module : MonoBehaviour
    {
        public void SetOwner(EnemyBase owner)=>Owner = owner;
        public abstract void Initialize();
        public abstract void OnRecycle();
        protected EnemyBase Owner;
    }
}