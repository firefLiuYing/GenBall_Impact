using System.Collections.Generic;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Flexible stat storage for BattleEntity. Stats are keyed by string name,
    /// allowing any entity to have any set of stats without code changes.
    /// </summary>
    public class StatComponent
    {
        private readonly Dictionary<string, Stat> _stats = new();

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
            GetOrCreate(name).SetBaseValue(baseValue);
        }

        /// <summary>Get the final (calculated) value of a named stat. Returns 0 if not found.</summary>
        public float GetValue(string name)
        {
            return TryGet(name, out var stat) ? stat.FinalValue : 0f;
        }

        /// <summary>Add a modifier to a named stat. Creates it if it doesn't exist.</summary>
        public void AddModifier(string name, StatModifier modifier)
        {
            GetOrCreate(name).AddModifier(modifier);
        }

        /// <summary>Remove a modifier from a named stat. No-op if stat doesn't exist.</summary>
        public void RemoveModifier(string name, StatModifier modifier)
        {
            if (_stats.TryGetValue(name, out var stat))
            {
                stat.RemoveModifier(modifier);
            }
        }

        /// <summary>Check if a named stat exists.</summary>
        public bool HasStat(string name)
        {
            return _stats.ContainsKey(name);
        }
    }
}
