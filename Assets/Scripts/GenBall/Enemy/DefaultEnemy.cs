using GenBall.BattleSystem;
using UnityEngine;

namespace GenBall.Enemy
{
    public class DefaultEnemy : MonoBehaviour, IEnemy
    {
        public void Initialize()
        {
            gameObject.SetActive(true);
        }
        // public void OnAttacked(AttackInfo attackInfo)
        // {
        //     Debug.Log($"我被打了，是{attackInfo.Attacker}干的");
        // }

        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

        public void Handle(IInteractToken stimulus, out IInteractToken response)
        {
            // todo gzp 补充完整
            response = null;
        }
    }
}