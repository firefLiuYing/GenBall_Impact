namespace GenBall.BattleSystem.Character
{
    public interface ICharacterInitializer
    {
        public int Priority { get; }
        public void Initialize(CharacterState characterState);
    }
}