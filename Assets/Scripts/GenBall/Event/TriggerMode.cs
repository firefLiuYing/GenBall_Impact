namespace GenBall.Event
{
    /// <summary>
    /// How an EventTrigger activates at runtime.
    /// </summary>
    public enum TriggerMode
    {
        /// <summary>Physics overlap trigger — fires on enter/exit.</summary>
        Collision = 0,

        /// <summary>Player-prompted interaction via IInteractSystem.</summary>
        Interact = 1,

        /// <summary>Subscribes to another event and fires when that event fires.</summary>
        EventListener = 2,
    }
}
