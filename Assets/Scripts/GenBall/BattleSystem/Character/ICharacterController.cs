namespace GenBall.BattleSystem.Character
{
    public interface ICharacterController
    {
        public int Priority { get; }
        public void Initialize(CharacterState  characterState);
        public void Tick(float deltaTime);
    }
}