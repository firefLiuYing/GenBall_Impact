using System;

namespace GenBall.Event
{
    /// <summary>
    /// Marks an EventParameterBase subclass as a suggested parameter type for
    /// specific event IDs. A soft hint — the editor defaults to these but
    /// allows selecting any parameter type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class EventParamHintAttribute : Attribute
    {
        public int EventId { get; }

        public EventParamHintAttribute(int eventId)
        {
            EventId = eventId;
        }
    }
}
