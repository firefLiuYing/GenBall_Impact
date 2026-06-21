#if UNITY_EDITOR
using GenBall.Map;
using GenBall.Map.EnemyUnitConfig;
using UnityEditor;
using UnityEngine;

namespace GenBall.Utils.Editor.Map
{
    /// <summary>
    /// Hierarchy right-click context menu entries for creating Map placeables.
    /// Mirrors the "3D Object &gt; Cube" workflow for lower learning curve.
    ///
    /// Dynamic placeables (enemies, triggers) are unpacked immediately after creation
    /// to break the prefab link. This prevents missing-dependency errors during
    /// asset bundle packaging, since placeholder prefabs live outside the AB scope.
    /// Non-dynamic placeables (save points, mechanisms) keep their prefab link.
    /// </summary>
    public static class PlaceableContextMenu
    {
        private const int MenuPriority = 10;

        // ── Enemy ──────────────────────────────────────────────

        [MenuItem("GameObject/Map/Enemy/NormalOrbis", false, MenuPriority)]
        private static void CreateNormalOrbis()
        {
            CreateDynamicPlaceable<NormalOrbisConfig>("NormalOrbis_Placeholder");
        }

        // ── SavePoint ──────────────────────────────────────────

        [MenuItem("GameObject/Map/SavePoint", false, MenuPriority + 10)]
        private static void CreateSavePoint()
        {
            CreateDynamicPlaceable<SavePointConfig>("SavePoint");
        }

        // ── SceneTrigger ───────────────────────────────────────

        [MenuItem("GameObject/Map/Trigger Volume", false, MenuPriority + 20)]
        private static void CreateTriggerVolume()
        {
            CreateDynamicPlaceable<TriggerVolume>("TriggerVolume");
        }

        // ── Helpers ────────────────────────────────────────────

        /// <summary>
        /// Create a dynamic placeable (enemies, triggers). Instantiates the prefab
        /// then immediately unpacks to break the prefab link, avoiding AB dependency.
        /// </summary>
        private static void CreateDynamicPlaceable<T>(string defaultName, string prefabPath = null)
            where T : Component
        {
            Vector3 spawnPos = GetSpawnPosition();

            // Resolve prefab path: parameter first, then [PlaceablePrefab] attribute
            if (string.IsNullOrEmpty(prefabPath))
            {
                var attr = System.Attribute.GetCustomAttribute(typeof(T), typeof(PlaceablePrefabAttribute)) as PlaceablePrefabAttribute;
                prefabPath = attr?.PrefabPath;
            }

            GameObject go;
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    go.transform.position = spawnPos;
                    // Break prefab link to avoid AB packaging dependency
                    PrefabUtility.UnpackPrefabInstance(go,
                        PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
                else
                {
                    go = new GameObject(defaultName);
                    go.transform.position = spawnPos;
                    go.AddComponent<T>();
                }
            }
            else
            {
                go = new GameObject(defaultName);
                go.transform.position = spawnPos;
                go.AddComponent<T>();
            }

            go.name = defaultName;
            Undo.RegisterCreatedObjectUndo(go, $"Create {typeof(T).Name}");
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Create a non-dynamic placeable (save points, mechanisms) that keeps its
        /// prefab link. The prefab must be within the asset bundle packaging scope.
        /// </summary>
        private static void CreateStaticPlaceable<T>(string defaultName, string prefabPath)
            where T : Component
        {
            Vector3 spawnPos = GetSpawnPosition();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject go;
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = spawnPos;
            }
            else
            {
                go = new GameObject(defaultName);
                go.transform.position = spawnPos;
                go.AddComponent<T>();
            }

            go.name = defaultName;
            Undo.RegisterCreatedObjectUndo(go, $"Create {typeof(T).Name}");
            Selection.activeGameObject = go;
        }

        private static Vector3 GetSpawnPosition()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView?.camera != null)
            {
                return sceneView.camera.transform.position +
                    sceneView.camera.transform.forward * 5f;
            }
            return Vector3.zero;
        }
    }
}
#endif
