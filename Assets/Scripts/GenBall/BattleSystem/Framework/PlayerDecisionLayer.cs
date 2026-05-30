using System;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using GenBall.Player;
using UnityEngine;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Player decision layer driven by IPlayerInputEvents.
    /// Subscribes to discrete input events (Jump, Dash, Fire, etc.) and issues
    /// the corresponding commands to the CommandDispatcherComponent.
    /// Continuous inputs (Move, Rotate) are polled each frame via LogicUpdate.
    ///
    /// Implements IEntityLogicUpdate for frame-based Move/Rotate polling.
    /// </summary>
    public class PlayerDecisionLayer : IDecisionLayer, IEntityLogicUpdate
    {
        private readonly BattleEntity _entity;
        private readonly IPlayerInputEvents _input;
        private CommandDispatcherComponent _dispatcher;

        private const float DashCooldown = 0.5f;
        private const float DashSpeed = 10f;
        private const float JumpSpeed = 8f;
        private float _dashCooldownTimer;

        public CommandDispatcherComponent Dispatcher { get; set; }

        public PlayerDecisionLayer(BattleEntity entity, IPlayerInputEvents inputEvents)
        {
            _entity = entity;
            _input = inputEvents;

            // Subscribe to discrete input events
            if (_input != null)
            {
                _input.OnJump += HandleJump;
                _input.OnDash += HandleDash;
                _input.OnFire += HandleFire;
                _input.OnReload += HandleReload;
                _input.OnSwitchWeapon += HandleSwitchWeapon;
                _input.OnInteract += HandleInteract;
                _input.OnScroll += HandleScroll;
            }
        }

        // ================================================================
        // Continuous inputs (polled each frame)
        // ================================================================

        public void LogicUpdate(float deltaTime)
        {
            ResolveDispatcher();
            if (_dispatcher == null) return;

            // Continuous commands every frame
            if (_input != null)
            {
                _dispatcher.Issue(new MoveCommand(_input.MoveDirection));
                _dispatcher.Issue(new RotateCommand(_input.ViewDelta.x, _input.ViewDelta.y));
            }

            // Tick cooldown
            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= deltaTime;
        }

        public void MakeDecision(float deltaTime)
        {
            // Kept for compatibility with tests and non-update-driven callers.
            // In production, LogicUpdate drives the per-frame work.
            LogicUpdate(deltaTime);
        }

        // ================================================================
        // Event handlers — discrete inputs
        // ================================================================

        private void HandleJump(ButtonState state)
        {
            ResolveDispatcher();
            if (_dispatcher == null) return;

            switch (state)
            {
                case ButtonState.Down:
                    if (IsOnGround())
                    {
                        _dispatcher.Issue(new JumpCommand(
                            Vector3.up * JumpSpeed, JumpPhase.Start));
                    }
                    break;

                case ButtonState.Up:
                    // Issue cancel to cut jump short (variable height)
                    _dispatcher.Issue(new JumpCommand(default, JumpPhase.Cancel));
                    break;
            }
        }

        private void HandleDash(ButtonState state)
        {
            ResolveDispatcher();
            if (_dispatcher == null || state != ButtonState.Down) return;
            if (_dashCooldownTimer > 0f) return;

            var dir = _input.MoveDirection.normalized;
            if (dir == Vector3.zero) dir = Vector3.forward;

            _dispatcher.Issue(new DashCommand(dir, DashSpeed));
            _dashCooldownTimer = DashCooldown;
        }

        private void HandleFire(ButtonState state)
        {
            ResolveDispatcher();
            if (_dispatcher == null) return;
            _dispatcher.Issue(new AttackCommand(0, state));
        }

        private void HandleReload(ButtonState state)
        {
            ResolveDispatcher();
            if (_dispatcher == null || state != ButtonState.Down) return;
            _dispatcher.Issue(new ReloadCommand());
        }

        private void HandleSwitchWeapon(ButtonState state)
        {
            ResolveDispatcher();
            if (_dispatcher == null || state != ButtonState.Down) return;
            _dispatcher.Issue(new SwitchWeaponCommand());
        }

        private void HandleInteract()
        {
            ResolveDispatcher();
            if (_dispatcher == null) return;
            _dispatcher.Issue(new InteractCommand(InteractAction.Trigger));
        }

        private void HandleScroll(float delta)
        {
            ResolveDispatcher();
            if (_dispatcher == null || delta == 0) return;
            _dispatcher.Issue(new InteractCommand(
                delta > 0 ? InteractAction.Next : InteractAction.Previous));
        }

        // ================================================================
        // Helpers
        // ================================================================

        private void ResolveDispatcher()
        {
            if (_dispatcher == null)
                _dispatcher = Dispatcher ?? _entity?.Get<CommandDispatcherComponent>();
        }

        private bool IsOnGround()
        {
            var groundDetect = _entity?.Get<ICharacterGroundDetect>();
            if (groundDetect != null) return groundDetect.IsOnGround;

            return _entity?.GetComponentInChildren<ICharacterGroundDetect>()?.IsOnGround ?? true;
        }
    }
}
