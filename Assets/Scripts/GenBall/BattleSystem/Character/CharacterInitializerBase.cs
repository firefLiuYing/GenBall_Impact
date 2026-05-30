using System;
using UnityEngine;

namespace GenBall.BattleSystem.Character
{
    [Obsolete]
    public abstract class CharacterInitializerBase : MonoBehaviour, ICharacterInitializer
    {
        [SerializeField] private int priority;
        public int Priority => priority;
        public abstract void Initialize(CharacterState characterState);
    }
}