using System;
using GenBall.BattleSystem;
using UnityEngine;

namespace GenBall.Enemy
{
    public class DefaultEnemy : MonoBehaviour, IEnemy
    {
        private int _health;
        public void Initialize()
        {
            gameObject.SetActive(true);
            _health = 100;
        }

        public void OnAttacked(AttackInfo attackInfo)
        {
            _health -= attackInfo.Damage;
            Debug.Log($"我被打了，是{attackInfo.Attacker}干的，扣了{attackInfo.Damage}血,还剩{_health}血");
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
    }
}