using System;
using JetBrains.Annotations;

namespace GenBall.Utils.Attributes.InspectorButton
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InspectorButtonAttribute : Attribute
    {
        public string Name;
        public InspectorButtonAttribute([NotNull] string name)
        {
            Name = name;
        }
    }
}