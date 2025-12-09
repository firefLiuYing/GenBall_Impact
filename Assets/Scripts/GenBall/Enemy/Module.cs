using UnityEngine;

namespace GenBall.Enemy
{
    public abstract class Module : MonoBehaviour
    {
        public void SetOwner(EnemyEntity owner)=>Owner = owner;
        public abstract void Initialize();
        protected EnemyEntity Owner;
    }
}