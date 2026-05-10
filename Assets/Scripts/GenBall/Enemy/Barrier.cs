using GenBall.BattleSystem.Character;
using UnityEngine;

namespace GenBall.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class Barrier : CharacterControllerBase
    {
        private Collider _collider;
        public void SetColliderEnable(bool enable)=>_collider.enabled=enable;

        public override void Initialize(CharacterState characterState)
        {
            _collider=GetComponent<Collider>();
        }

        public override void Tick(float deltaTime)
        {
            
        }
    }
}