using System;

namespace GenBall.BattleSystem.Character
{
    [Obsolete]
    public interface ICharacterInitializer
    {
        public int Priority { get; }
        public void Initialize(CharacterState characterState);
    }
}