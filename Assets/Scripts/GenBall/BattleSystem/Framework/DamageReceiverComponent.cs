using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework
{
    public class DamageReceiverComponent : IHealth, IDamageable, IHealable
    {
        private readonly BattleEntity _entity;

        public int Health => (int)(_entity.Get<StatComponent>()?.GetValue("CurrentHealth") ?? 0);
        public int MaxHealth => (int)(_entity.Get<StatComponent>()?.GetValue("MaxHealth") ?? 0);
        public bool IsDead { get; private set; }
        public bool IsInvincible { get; set; }

        public DamageReceiverComponent(BattleEntity entity)
        {
            _entity = entity;
            // Always reset CurrentHealth to MaxHealth on construction
            var stats = _entity.Get<StatComponent>();
            if (stats != null)
            {
                stats.SetBase("CurrentHealth", MaxHealth);
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            // Debug.Log("gzp 1");
            if (IsDead || IsInvincible) return;
            // Debug.Log("gzp 2");
            var stats = _entity.Get<StatComponent>();
            if (stats == null) return;
            // Debug.Log("gzp 3");
            int damage = damageInfo.Damage.GetValue();
            var oldHealth = stats.GetValue("CurrentHealth");

            // Shield absorbs damage first
            if (stats.HasStat("Shield"))
            {
                var shield = stats.GetValue("Shield");
                if (shield > 0f)
                {
                    if (shield >= damage)
                    {
                        stats.SetBase("Shield", shield - damage);
                        // Health unchanged, but still fire HealthChanged (shield absorbed it)
                        var newHealth = stats.GetValue("CurrentHealth");
                        FireHealthChanged(oldHealth, newHealth, stats.GetValue("MaxHealth"), damageInfo.Attacker);
                        return;
                    }
                    else
                    {
                        damage -= (int)shield;
                        stats.SetBase("Shield", 0f);
                    }
                }
            }

            // Deduct remaining damage from CurrentHealth
            var newCurrentHealth = oldHealth - damage;
            if (newCurrentHealth < 0f) newCurrentHealth = 0f;
            stats.SetBase("CurrentHealth", newCurrentHealth);

            FireHealthChanged(oldHealth, stats.GetValue("CurrentHealth"),
                stats.GetValue("MaxHealth"), damageInfo.Attacker);
            // Debug.Log($"gzp 4 {stats.GetValue("CurrentHealth")}");
        }

        public void Heal(int healAmount)
        {
            if (IsDead) return;

            var stats = _entity.Get<StatComponent>();
            if (stats == null) return;

            var oldHealth = stats.GetValue("CurrentHealth");
            var maxHealth = stats.GetValue("MaxHealth");
            var newHealth = Mathf.Min(oldHealth + healAmount, maxHealth);

            stats.SetBase("CurrentHealth", newHealth);

            FireHealthChanged(oldHealth, newHealth, maxHealth, null);
        }

        public void Die(DeathInfo deathInfo)
        {
            IsDead = true;
        }

        private void FireHealthChanged(float oldHealth, float newHealth, float maxHealth, GameObject damageSource)
        {
            var ed = _entity.Get<EventDispatcherComponent>();
            if (ed == null) return;

            ed.FireNow((int)EntityEventId.HealthChanged,
                new HealthChangedEventData
                {
                    OldHealth = oldHealth,
                    NewHealth = newHealth,
                    MaxHealth = maxHealth,
                    DamageSource = damageSource,
                });
        }
    }
}
