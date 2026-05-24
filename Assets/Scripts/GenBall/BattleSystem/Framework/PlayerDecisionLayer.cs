using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Character;
using GenBall.Framework.Entity;
using UnityEngine;

namespace GenBall.BattleSystem.Framework
{
    public interface IPlayerInputProvider
    {
        Vector3 MoveDirection { get; }
        Vector2 ViewDelta { get; }
        bool JumpPressed { get; }
        bool DashPressed { get; }
        bool FirePressed { get; }
    }

    /// <summary>
    /// Player decision layer driven by IPlayerInputProvider.
    /// Issues Move, Rotate, Jump, Dash, and Attack commands to the
    /// CommandDispatcherComponent each frame.
    ///
    /// Implements IEntityLogicUpdate so it receives frame updates from
    /// EntityUpdateSystem when registered with a BattleEntity.
    /// </summary>
    public class PlayerDecisionLayer : IDecisionLayer, IEntityLogicUpdate
    {
        private readonly BattleEntity _entity;
        private readonly IPlayerInputProvider _input;
        private CommandDispatcherComponent _dispatcher;

        private const float DashCooldown = 0.5f;
        private const float DashSpeed = 10f;
        private const float JumpSpeed = 8f;
        private float _dashCooldownTimer;

        public CommandDispatcherComponent Dispatcher { get; set; }

        public PlayerDecisionLayer(BattleEntity entity, IPlayerInputProvider inputProvider)
        {
            _entity = entity;
            _input = inputProvider;
        }

        public void MakeDecision(float deltaTime)
        {
            // Lazy-resolve dispatcher (allows tests to set it directly)
            if (_dispatcher == null)
                _dispatcher = Dispatcher ?? _entity.Get<CommandDispatcherComponent>();

            if (_dispatcher == null)
                return;

            // Continuous commands every frame
            _dispatcher.Issue(new MoveCommand(_input.MoveDirection));
            _dispatcher.Issue(new RotateCommand(_input.ViewDelta.x, _input.ViewDelta.y));

            // Jump
            if (_input.JumpPressed && IsOnGround())
            {
                _dispatcher.Issue(new JumpCommand(Vector3.up * JumpSpeed));
            }

            // Dash (with cooldown)
            if (_input.DashPressed && _dashCooldownTimer <= 0f)
            {
                _dispatcher.Issue(new DashCommand(GetDashDirection(), DashSpeed));
                _dashCooldownTimer = DashCooldown;
            }

            // Attack
            if (_input.FirePressed)
            {
                _dispatcher.Issue(new AttackCommand(0));
            }

            // Tick cooldown
            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= deltaTime;
        }

        public void LogicUpdate(float deltaTime)
        {
            MakeDecision(deltaTime);
        }

        private bool IsOnGround()
        {
            var groundDetect = _entity.Get<ICharacterGroundDetect>();
            if (groundDetect != null) return groundDetect.IsOnGround;

            return _entity.GetComponentInChildren<ICharacterGroundDetect>()?.IsOnGround ?? true;
        }

        private Vector3 GetDashDirection()
        {
            var dir = _input.MoveDirection.normalized;
            return dir != Vector3.zero ? dir : Vector3.forward;
        }
    }
}
