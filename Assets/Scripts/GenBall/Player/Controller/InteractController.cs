using GenBall.BattleSystem.Character;
using GenBall.Interact;
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
            _input.OnInteract += Interact;
            _input.OnScrollChange+=ChangeSelection;
        }

        private void Interact()
        {
            InteractSystem.Instance.TriggerInteractable();
        }

        private void ChangeSelection(float delta)
        {
            if(delta<0) InteractSystem.Instance.NextSelection();
            else InteractSystem.Instance.LastSelection();
        }

        public override void Tick(float deltaTime)
        {
            
        }
    }
}