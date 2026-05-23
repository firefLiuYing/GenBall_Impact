using System.Collections.Generic;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework
{
    public class DamageReceiverComponent : IHealth, IDamageable
    {
        private readonly BattleEntity _entity;
        private int _health;

        public int Health => _health;
        public int MaxHealth => (int)(_entity.Get<StatComponent>()?.GetValue("MaxHealth") ?? 0);
        public bool IsDead { get; private set; }

        public DamageReceiverComponent(BattleEntity entity)
        {
            _entity = entity;
            _health = MaxHealth;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            int damage = damageInfo.Damage.GetValue();
            _health -= damage;

            if (_health <= 0)
            {
                _health = 0;
                var deathSystem = SystemRepository.Instance.GetSystem<IDeathSystem>();
                deathSystem?.ApplyDeath(DeathInfo.Create(
                    _entity.gameObject,
                    new List<string> { DeathTag.HealthEmpty },
                    damageInfo.Attacker));
            }
        }

        public void Die(DeathInfo deathInfo)
        {
            IsDead = true;
        }
    }
}
