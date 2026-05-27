using System.Collections.Generic;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Event data for StatChanged entity event.
    /// </summary>
    public struct StatChangedEventData
    {
        public string StatName;
        public float OldValue;
        public float NewValue;
    }

    /// <summary>
    /// Flexible stat storage for BattleEntity. Stats are keyed by string name,
    /// allowing any entity to have any set of stats without code changes.
    ///
    /// Fires StatChanged events through EventDispatcherComponent when stats change.
    /// </summary>
    public class StatComponent
    {
        private readonly BattleEntity _entity;
        private readonly Dictionary<string, Stat> _stats = new();

        public StatComponent(BattleEntity entity = null)
        {
            _entity = entity;
        }

        /// <summary>
        /// Get an existing stat or create a new one with the given base value.
        /// </summary>
        public Stat GetOrCreate(string name, float baseValue = 0f)
        {
            if (!_stats.TryGetValue(name, out var stat))
            {
                stat = new Stat(baseValue);
                _stats[name] = stat;
            }
            return stat;
        }

        /// <summary>Try to get a stat by name. Returns false if not found.</summary>
        public bool TryGet(string name, out Stat stat)
        {
            return _stats.TryGetValue(name, out stat);
        }

        /// <summary>Set the base value of a named stat. Creates it if it doesn't exist.</summary>
        public void SetBase(string name, float baseValue)
        {
            var stat = GetOrCreate(name);
            var oldValue = stat.SetBaseValue(baseValue);
            FireStatChanged(name, oldValue, stat.FinalValue);
        }

        /// <summary>Get the final (calculated) value of a named stat. Returns 0 if not found.</summary>
        public float GetValue(string name)
        {
            return TryGet(name, out var stat) ? stat.FinalValue : 0f;
        }

        /// <summary>Add a modifier to a named stat. Creates it if it doesn't exist.</summary>
        public void AddModifier(string name, StatModifier modifier)
        {
            var stat = GetOrCreate(name);
            var oldValue = stat.AddModifier(modifier);
            FireStatChanged(name, oldValue, stat.FinalValue);
        }

        /// <summary>Remove a modifier from a named stat. No-op if stat doesn't exist.</summary>
        public void RemoveModifier(string name, StatModifier modifier)
        {
            if (_stats.TryGetValue(name, out var stat))
            {
                var oldValue = stat.RemoveModifier(modifier);
                FireStatChanged(name, oldValue, stat.FinalValue);
            }
        }

        /// <summary>Check if a named stat exists.</summary>
        public bool HasStat(string name)
        {
            return _stats.ContainsKey(name);
        }

        private void FireStatChanged(string name, float oldValue, float newValue)
        {
            var ed = _entity?.Get<EventDispatcherComponent>();
            if (ed == null) return;

            ed.FireNow((int)EntityEventId.StatChanged,
                new StatChangedEventData
                {
                    StatName = name,
                    OldValue = oldValue,
                    NewValue = newValue,
                });
        }
    }
}
