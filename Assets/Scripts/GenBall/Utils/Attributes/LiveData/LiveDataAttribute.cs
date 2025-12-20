using System;

namespace GenBall.Utils.Attributes.LiveData
{
    [AttributeUsage(AttributeTargets.Field,Inherited = false, AllowMultiple = false)]
    public sealed class LiveDataAttribute : Attribute
    {
        public string EventName { get; private set; }
        public bool GeneratePublicProperties { get; set; } = true;
        public string PropertyName { get; set; }
        public bool SkipIfEqual { get; set; } = false;
        
        public LiveDataAttribute(string eventName)
        {
            EventName = eventName??throw new ArgumentNullException(nameof(eventName));
        }
    }
}