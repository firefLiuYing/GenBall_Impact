using GenBall.BattleSystem.Character;
using GenBall.Player.Input;

namespace GenBall.Player.Controller
{
    public class InteractController : CharacterControllerBase
    {
        private CharacterState _player;
        private InputHandler _input;
        public override void Initialize(CharacterState characterState)
        {
            _player = characterState;
            _input = _player.GetComponentInChildren<InputHandler>();
        }

        public override void Tick(float deltaTime)
        {
            
        }
    }
}