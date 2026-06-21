using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GenBall.Event
{
    /// <summary>
    /// ScriptableObject holding designer-defined placed events from CSV import.
    /// Placed event IDs start at 6000 to avoid conflicts with the GlobalEventId enum.
    /// Asset path: Assets/Resources/Configs/PlacedEventTable.asset
    /// </summary>
    public class PlacedEventTable : ScriptableObject
    {
        public List<PlacedEventEntry> entries = new();

        /// <summary>
        /// Validate that no placed event IDs conflict with GlobalEventId enum values
        /// or duplicate each other. Returns null if valid, error message otherwise.
        /// </summary>
        public string ValidateNoConflicts()
        {
            var enumValues = new HashSet<int>(
                System.Enum.GetValues(typeof(GlobalEventId)).Cast<int>());

            var seenIds = new HashSet<int>();
            foreach (var e in entries)
            {
                if (enumValues.Contains(e.id))
                    return $"PlacedEvent [{e.id}] {e.name} conflicts with GlobalEventId enum value.";
                if (!seenIds.Add(e.id))
                    return $"Duplicate PlacedEvent id: {e.id}";
            }
            return null;
        }
    }

    [System.Serializable]
    public class PlacedEventEntry
    {
        public int id;
        public string name;
        public string displayName;
        /// <summary>
        /// Assembly-qualified name of the default parameter type, or empty.
        /// Example: "GenBall.Event.Params.SpawnEnemyParams, Assembly-CSharp"
        /// </summary>
        public string defaultParamType;
    }
}
