using System;
using GenBall.BattleSystem;
using UnityEngine;

namespace GenBall.Enemy
{
    public class DefaultEnemy : MonoBehaviour, IEnemy
    {
        private IEnemy _enemyImplementation;

        public void Initialize()
        {
            gameObject.SetActive(true);
        }
        public void OnAttacked(AttackInfo attackInfo)
        {
            Debug.Log($"我被打了，是{attackInfo.Attacker}干的");
        }

        public void EntityUpdate(float deltaTime)
        {
            
        }

        public void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public void OnRecycle()
        {
            
        }

        // public void Handle(IInteractToken stimulus, out IInteractToken[] responses)
        // {
        //     // todo gzp 补充完整
        //     responses = Array.Empty<IInteractToken>();
        // }
    }
}