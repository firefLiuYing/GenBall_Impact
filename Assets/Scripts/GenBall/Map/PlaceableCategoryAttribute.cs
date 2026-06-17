using System;

namespace GenBall.Map
{
    /// <summary>
    /// Marks a MonoBehaviour as a scene-placeable type.
    /// The editor auto-discovers all types with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PlaceableCategoryAttribute : Attribute
    {
        /// <summary>Unique category key for grouping in the editor tree.</summary>
        public string Category { get; }

        /// <summary>Human-readable display name in the editor.</summary>
        public string DisplayName { get; }

        /// <summary>Sort order in the editor category tree (lower = first).</summary>
        public int Order { get; set; }

        public PlaceableCategoryAttribute(string category, string displayName, int order = 0)
        {
            Category = category;
            DisplayName = displayName;
            Order = order;
        }
    }
}
