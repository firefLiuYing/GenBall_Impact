using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Listens for HealthChanged events on the entity. When CurrentHealth
    /// decreases (actual damage, not shield-only), issues a StunCommand
    /// through the CommandDispatcher to interrupt the current action.
    ///
    /// Also implements IStun + IEntityLogicUpdate to serve as its own
    /// stun executor with a duration timer.
    /// </summary>
    public class HitReactionComponent : IStun, IEntityLogicUpdate
    {
        private readonly CommandDispatcherComponent _dispatcher;
        private readonly float _stunDuration;
        private float _stunTimer;

        public bool IsStunned => _stunTimer > 0f;

        /// <param name="dispatcher">The entity's CommandDispatcherComponent</param>
        /// <param name="eventDispatcher">The entity's EventDispatcherComponent</param>
        /// <param name="stunDuration">Duration of stun in seconds (default 0.3)</param>
        public HitReactionComponent(
            CommandDispatcherComponent dispatcher,
            EventDispatcherComponent eventDispatcher,
            float stunDuration = 0.3f)
        {
            _dispatcher = dispatcher;
            _stunDuration = stunDuration;

            // Register self as the executor for StunCommand
            _dispatcher.RegisterExecutor<StunCommand>(this);

            // Subscribe to HealthChanged events (same pattern as DeathComponent)
            eventDispatcher.Subscribe<HealthChangedEventData>(
                (int)EntityEventId.HealthChanged, OnHealthChanged);
        }

        private void OnHealthChanged(HealthChangedEventData data)
        {
            // Only trigger stun on actual health loss (not shield-only damage)
            if (data.NewHealth >= data.OldHealth)
                return;

            if (_dispatcher == null)
                return;

            // Clear buffered commands and issue stun
            _dispatcher.ClearBuffer();
            _dispatcher.Issue(new StunCommand(_stunDuration));
        }

        public void Stun(StunCommand command)
        {
            _stunTimer = command.Duration;
        }

        public void LogicUpdate(float deltaTime)
        {
            if (_stunTimer > 0f)
            {
                _stunTimer -= deltaTime;
            }
        }
    }
}
