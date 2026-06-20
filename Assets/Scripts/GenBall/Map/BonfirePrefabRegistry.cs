using System.Collections.Generic;

namespace GenBall.Map
{
    /// <summary>
    /// Maps bonfire type strings to runtime prefab paths for bonfire spawning.
    /// New bonfire types register their paths here during system initialization.
    /// </summary>
    public static class BonfirePrefabRegistry
    {
        private static readonly Dictionary<string, string> Paths = new()
        {
            ["Default"] = "Assets/AssetBundles/Common/Map/SavePoint/DefaultSavePoint.prefab"
        };

        public static void Register(string typeName, string prefabPath)
        {
            Paths[typeName] = prefabPath;
        }

        public static bool TryGetPath(string typeName, out string path)
        {
            return Paths.TryGetValue(typeName, out path);
        }

        /// <summary>
        /// Returns all registered bonfire type names. Used by editor dropdown.
        /// </summary>
        public static IEnumerable<string> RegisteredTypes => Paths.Keys;
    }
}
