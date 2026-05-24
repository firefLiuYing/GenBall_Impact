using GenBall.BattleSystem.Framework;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Adapts InputHandler (MonoBehaviour) to IPlayerInputProvider (pure interface)
    /// for consumption by PlayerDecisionLayer.
    /// </summary>
    public class PlayerInputAdapter : IPlayerInputProvider
    {
        private readonly InputHandler _inputHandler;

        public Vector3 MoveDirection => _inputHandler.MoveDirection;

        public Vector2 ViewDelta => _inputHandler.ViewDelta;

        /// <summary>
        /// Uses ConsumeBufferedJump() to avoid re-triggering JumpCommand each frame
        /// while the jump button is held. The buffered window allows a brief grace period
        /// for the player to press jump slightly before landing.
        /// </summary>
        public bool JumpPressed => _inputHandler.ConsumeBufferedJump();

        /// <summary>
        /// IsDashPressed is true only on the frame the dash key was pressed
        /// (InputActionPhase.Started). This prevents re-triggering during hold.
        /// </summary>
        public bool DashPressed => _inputHandler.IsDashPressed;

        public bool FirePressed => _inputHandler.IsFirePressed;

        public PlayerInputAdapter(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }
    }
}
