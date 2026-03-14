using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    public abstract class CharacterControllerBase : MonoBehaviour, ICharacterController
    {
        [SerializeField] private int priority;
        public int Priority=>priority;
        public abstract void Initialize(CharacterState characterState);
        public abstract void Tick(float deltaTime);
    }
}