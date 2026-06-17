using System.Collections.Generic;

namespace GenBall.Map
{
    /// <summary>
    /// Maps enemy type strings to prefab paths for runtime spawning.
    /// Replaces the hardcoded dictionary in SceneExecutorSystemDefault.
    /// New enemy types register their paths here during system initialization.
    /// </summary>
    public static class EnemyPrefabRegistry
    {
        private static readonly Dictionary<string, string> Paths = new()
        {
            { "NormalOrbis", "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/NormalOrbis.prefab" },
        };

        /// <summary>
        /// Register a new enemy type-to-prefab mapping.
        /// </summary>
        public static void Register(string typeName, string prefabPath)
        {
            Paths[typeName] = prefabPath;
        }

        /// <summary>
        /// Try to get the prefab path for an enemy type.
        /// </summary>
        public static bool TryGetPath(string typeName, out string path)
        {
            return Paths.TryGetValue(typeName, out path);
        }
    }
}
