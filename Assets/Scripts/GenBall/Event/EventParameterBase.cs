namespace GenBall.Event
{
    /// <summary>
    /// Implemented by all event parameter classes. Each concrete class
    /// calls the correct generic CEventRouter.FireNow&lt;T&gt; overload
    /// for its own type — zero reflection at dispatch time.
    /// </summary>
    public interface IEventParameter
    {
        void Dispatch(int eventId);
    }

    /// <summary>
    /// Base class for all event parameter types. Derive from this,
    /// mark the derived class [System.Serializable], and use
    /// [SerializeReference] to store instances polymorphically.
    /// </summary>
    [System.Serializable]
    public abstract class EventParameterBase : IEventParameter
    {
        public abstract void Dispatch(int eventId);
    }
}
