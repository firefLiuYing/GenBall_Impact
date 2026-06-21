using System.Collections.Generic;
using UnityEngine;
using Yueyn.Event;

namespace GenBall.Event
{
    /// <summary>
    /// A single event entry within an EventAdapter.
    /// </summary>
    [System.Serializable]
    public class EventEntry
    {
        public int eventId;

        [SerializeReference]
        public EventParameterBase parameters;
    }

    /// <summary>
    /// Serializable event container. Internally holds a list of EventEntry,
    /// similar to UnityEvent's persistent calls. Inspector shows a compact
    /// inline list with per-entry Event dropdown and parameter configuration.
    ///
    /// Can be embedded in any MonoBehaviour or data structure.
    /// At runtime, Fire() iterates all entries and dispatches each.
    /// </summary>
    [System.Serializable]
    public class EventAdapter
    {
        // Legacy fields — kept for backward compatibility with previously serialized data.
        // Moved into _entries on first access via EnsureMigrated().
        [SerializeField, HideInInspector]
        private int eventId;

        [SerializeReference, HideInInspector]
        private EventParameterBase parameters;

        [SerializeField]
        private List<EventEntry> _entries = new();

        private bool _migrated;

        public IReadOnlyList<EventEntry> Entries
        {
            get
            {
                EnsureMigrated();
                return _entries;
            }
        }

        /// <summary>Accessor for the first entry's eventId (backward compat).</summary>
        public int EventId
        {
            get
            {
                EnsureMigrated();
                return _entries.Count > 0 ? _entries[0].eventId : 0;
            }
            set
            {
                EnsureMigrated();
                if (_entries.Count == 0)
                    _entries.Add(new EventEntry());
                _entries[0].eventId = value;
            }
        }

        /// <summary>Accessor for the first entry's parameters (backward compat).</summary>
        public EventParameterBase Parameters
        {
            get
            {
                EnsureMigrated();
                return _entries.Count > 0 ? _entries[0].parameters : null;
            }
            set
            {
                EnsureMigrated();
                if (_entries.Count == 0)
                    _entries.Add(new EventEntry());
                _entries[0].parameters = value;
            }
        }

        public bool HasParameters
        {
            get
            {
                EnsureMigrated();
                return _entries.Count > 0 && _entries[0].parameters != null;
            }
        }

        private void EnsureMigrated()
        {
            if (_migrated) return;
            _migrated = true;

            if (_entries.Count > 0) return;
            if (eventId == 0) return;

            _entries.Add(new EventEntry
            {
                eventId = eventId,
                parameters = parameters,
            });
        }

        /// <summary>
        /// Fire all event entries via CEventRouter.
        /// </summary>
        public void Fire()
        {
            EnsureMigrated();
            foreach (var entry in _entries)
            {
                if (entry.parameters != null)
                    entry.parameters.Dispatch(entry.eventId);
                else
                    CEventRouter.Instance.FireNow(entry.eventId);
            }
        }

        /// <summary>
        /// Create a copy with cloned entries (parameters are shallow-copied).
        /// </summary>
        public EventAdapter Clone()
        {
            var clone = new EventAdapter();
            EnsureMigrated();
            foreach (var entry in _entries)
            {
                clone._entries.Add(new EventEntry
                {
                    eventId = entry.eventId,
                    parameters = entry.parameters,
                });
            }
            clone._migrated = true;
            return clone;
        }
    }
}
