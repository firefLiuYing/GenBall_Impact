using System;
using GenBall.BattleSystem.Framework;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player.Executor
{
    /// <summary>
    /// Adapts InputHandler (MonoBehaviour) to IPlayerInputEvents (pure interface)
    /// for consumption by PlayerDecisionLayer.
    /// Forwards discrete input events directly and exposes continuous
    /// MoveDirection / ViewDelta as pollable properties.
    /// </summary>
    public class PlayerInputAdapter : IPlayerInputEvents
    {
        private readonly InputHandler _inputHandler;

        public event Action<ButtonState> OnJump;
        public event Action<ButtonState> OnDash;
        public event Action<ButtonState> OnFire;
        public event Action<ButtonState> OnReload;
        public event Action<ButtonState> OnSwitchWeapon;
        public event Action OnInteract;
        public event Action<float> OnScroll;

        public Vector3 MoveDirection => _inputHandler.MoveDirection;
        public Vector2 ViewDelta => _inputHandler.ViewDelta;

        public PlayerInputAdapter(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;

            // Forward discrete input events from handler to adapter events
            _inputHandler.OnJump += state => OnJump?.Invoke(state);
            _inputHandler.OnDash += state => OnDash?.Invoke(state);
            _inputHandler.OnFire += state => OnFire?.Invoke(state);
            _inputHandler.OnReload += state => OnReload?.Invoke(state);
            _inputHandler.OnSwitchWeapon += state => OnSwitchWeapon?.Invoke(state);
            _inputHandler.OnInteract += () => OnInteract?.Invoke();
            _inputHandler.OnScrollChange += delta => OnScroll?.Invoke(delta);
        }
    }
}
