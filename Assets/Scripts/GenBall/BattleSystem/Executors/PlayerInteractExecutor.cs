using GenBall.Interact;
using GenBall.BattleSystem.Command;

namespace GenBall.BattleSystem.Executors
{
    /// <summary>
    /// Execute layer: handles InteractCommand for player interaction.
    /// Sight detection is now handled by InteractSystem (IFrameUpdate).
    /// </summary>
    public class PlayerInteractExecutor : IInteract
    {
        private readonly IInteractSystem _interactSystem;

        public PlayerInteractExecutor(IInteractSystem interactSystem)
        {
            _interactSystem = interactSystem;
        }

        public void Interact(InteractCommand cmd)
        {
            switch (cmd.Action)
            {
                case InteractAction.Trigger:
                    _interactSystem.TriggerInteractable();
                    break;
                case InteractAction.Next:
                    _interactSystem.NextSelection();
                    break;
                case InteractAction.Previous:
                    _interactSystem.LastSelection();
                    break;
            }
        }
    }
}
