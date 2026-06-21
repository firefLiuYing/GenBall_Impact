using UnityEngine;
using Yueyn.Event;

namespace GenBall.Event
{
    /// <summary>
    /// Serializable event payload. Stores an event ID and a polymorphic parameter
    /// object. Can be embedded in any MonoBehaviour or data structure.
    ///
    /// At runtime, Fire() calls the correct generic CEventRouter.FireNow&lt;T&gt;
    /// overload via the parameter's Dispatch() — zero reflection.
    /// </summary>
    [System.Serializable]
    public class EventAdapter
    {
        [SerializeField]
        private int eventId;

        [SerializeReference]
        private EventParameterBase parameters;

        public int EventId
        {
            get => eventId;
            set => eventId = value;
        }

        public EventParameterBase Parameters
        {
            get => parameters;
            set => parameters = value;
        }

        public bool HasParameters => parameters != null;

        /// <summary>
        /// Fire the event via CEventRouter. If parameters exists, delegates to
        /// its Dispatch() which calls the correct generic FireNow&lt;T&gt; overload.
        /// Otherwise calls the parameterless FireNow().
        /// </summary>
        public void Fire()
        {
            if (parameters != null)
            {
                parameters.Dispatch(eventId);
            }
            else
            {
                CEventRouter.Instance.FireNow(eventId);
            }
        }

        /// <summary>
        /// Create a shallow copy for baking into config data.
        /// Parameter objects should be immutable-ish data bags, so shallow copy is sufficient.
        /// </summary>
        public EventAdapter Clone()
        {
            return new EventAdapter
            {
                eventId = this.eventId,
                parameters = this.parameters,
            };
        }
    }
}
