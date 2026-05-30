using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem
{
    /// <summary>
    /// Event data for HealthChanged entity event.
    /// </summary>
    public struct HealthChangedEventData
    {
        public float OldHealth;
        public float NewHealth;
        public float MaxHealth;
        public GameObject DamageSource;
    }

    /// <summary>
    /// Per-entity death behavior. Implementations handle entity-specific death logic
    /// (animations, respawn, loot, despawn, etc.).
    /// </summary>
    public interface IDeathHandler
    {
        void OnDeath(DeathInfo deathInfo);
    }
}

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Monitors the entity's CurrentHealth stat via HealthChanged events.
    /// When CurrentHealth drops to zero or below, triggers the global IDeathSystem
    /// pipeline and then delegates to an entity-specific IDeathHandler.
    /// </summary>
    public class DeathComponent
    {
        private readonly BattleEntity _entity;
        private readonly IDeathHandler _handler;
        private bool _dead;

        public DeathComponent(BattleEntity entity, IDeathHandler handler)
        {
            _entity = entity;
            _handler = handler;

            var eventDispatcher = _entity.Get<EventDispatcherComponent>();
            eventDispatcher?.Subscribe<HealthChangedEventData>(
                (int)EntityEventId.HealthChanged, OnHealthChanged);
        }

        private void OnHealthChanged(HealthChangedEventData data)
        {
            if (_dead) return;
            if (data.NewHealth > 0f) return;

            _dead = true;

            var deathInfo = DeathInfo.Create(
                _entity.gameObject,
                new List<string> { DeathTag.HealthEmpty },
                data.DamageSource);

            // Entity-specific death behavior (before death system releases deathInfo)
            _handler.OnDeath(deathInfo);

            // Run global death pipeline (buff events etc.) — releases deathInfo
            var deathSystem = SystemRepository.Instance.GetSystem<IDeathSystem>();
            deathSystem?.ApplyDeath(deathInfo);
        }
    }
}
