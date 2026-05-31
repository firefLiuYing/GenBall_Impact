using System;
using GenBall.BattleSystem.Framework;
using GenBall.Event;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Main;

namespace GenBall.CombatState
{
    public class CombatStateSystem : ICombatStateSystem, ILogicUpdate
    {
        private float _lastCombatTime;
        private bool _wasInCombat;
        private bool _unsubscribed;
        private BattleEntity _playerEntity;
        private EventDispatcherComponent _playerEventDispatcher;

        private Action _onPlayerHealthChanged;
        private Action _onEnemyDeath;

        public bool IsInCombat => Time.time - _lastCombatTime < CombatTimeoutSeconds;
        public float CombatTimeoutSeconds { get; set; } = 5f;
        public SystemScope LogicUpdateScope => SystemScope.Game;

        public void Init()
        {
        }

        public void UnInit()
        {
            if (_unsubscribed) return;
            _unsubscribed = true;

            if (_playerEventDispatcher != null && _onPlayerHealthChanged != null)
            {
                _playerEventDispatcher.Unsubscribe((int)EntityEventId.HealthChanged, _onPlayerHealthChanged);
            }

            if (_onEnemyDeath != null)
            {
                CEventRouter.Instance.Unsubscribe((int)GlobalEventId.EnemyDeath, _onEnemyDeath);
            }
        }

        public void BindPlayer(BattleEntity player)
        {
            _playerEntity = player;
            _playerEventDispatcher = player?.Get<EventDispatcherComponent>();

            _onPlayerHealthChanged = () =>
            {
                _lastCombatTime = Time.time;
            };

            _onEnemyDeath = () =>
            {
                _lastCombatTime = Time.time;
            };

            _playerEventDispatcher?.Subscribe((int)EntityEventId.HealthChanged, _onPlayerHealthChanged);
            CEventRouter.Instance.Subscribe((int)GlobalEventId.EnemyDeath, _onEnemyDeath);

            _lastCombatTime = Time.time;
            _wasInCombat = true;
        }

        public void LogicUpdate(float deltaTime)
        {
            var isInCombatNow = IsInCombat;
            if (_wasInCombat != isInCombatNow)
            {
                _wasInCombat = isInCombatNow;
                if (!isInCombatNow)
                {
                    CEventRouter.Instance.FireNow((int)GlobalEventId.CombatStateChanged);
                }
            }
        }
    }
}
