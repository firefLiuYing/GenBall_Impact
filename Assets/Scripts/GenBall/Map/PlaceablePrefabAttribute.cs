using System;

namespace GenBall.Map
{
    /// <summary>
    /// Maps a placeable type to its prefab path.
    /// Used by the editor "Add New" context menu to instantiate the correct prefab.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PlaceablePrefabAttribute : Attribute
    {
        public string PrefabPath { get; }

        public PlaceablePrefabAttribute(string prefabPath)
        {
            PrefabPath = prefabPath;
        }
    }
}
