using System;

namespace GenBall.BattleSystem.Character
{
    [Obsolete]
    public interface ICharacterController
    {
        public int Priority { get; }
        public void Initialize(CharacterState  characterState);
        public void Tick(float deltaTime);
    }
}