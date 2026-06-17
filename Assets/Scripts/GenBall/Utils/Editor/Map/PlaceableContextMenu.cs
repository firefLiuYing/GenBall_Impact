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
    /// </summary>
    public static class PlaceableContextMenu
    {
        private const int MenuPriority = 10;

        // ── Enemy ──────────────────────────────────────────────

        [MenuItem("GameObject/Map/Enemy/NormalOrbis", false, MenuPriority)]
        private static void CreateNormalOrbis()
        {
            CreatePlaceable<NormalOrbisConfig>("NormalOrbis_Placeholder");
        }

        // ── SavePoint ──────────────────────────────────────────

        [MenuItem("GameObject/Map/SavePoint", false, MenuPriority + 10)]
        private static void CreateSavePoint()
        {
            CreatePlaceableFromPrefab<SavePointConfig>(
                "SavePoint",
                "Assets/AssetBundles/Common/SavePoint/SavePoint.prefab");
        }

        // ── SceneTrigger ───────────────────────────────────────

        [MenuItem("GameObject/Map/SceneTrigger", false, MenuPriority + 20)]
        private static void CreateSceneTrigger()
        {
            CreatePlaceableFromPrefab<SceneTriggerConfig>(
                "Trigger",
                "Assets/AssetBundles/Common/TriggerObject/Trigger.prefab");
        }

        // ── Helpers ────────────────────────────────────────────

        /// <summary>
        /// Create a placeable by instantiating a prefab (which should already have the component).
        /// </summary>
        private static void CreatePlaceableFromPrefab<T>(string defaultName, string prefabPath)
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

        /// <summary>
        /// Create a placeable as an empty GameObject with the config component added.
        /// Used for dynamic entities (enemies) where the prefab is a lightweight placeholder
        /// that may not exist yet.
        /// </summary>
        private static void CreatePlaceable<T>(string defaultName) where T : Component
        {
            // Try to find the placeholder prefab from [PlaceablePrefab] attribute
            var prefabAttr = System.Attribute.GetCustomAttribute(typeof(T), typeof(PlaceablePrefabAttribute)) as PlaceablePrefabAttribute;
            string prefabPath = prefabAttr?.PrefabPath;

            Vector3 spawnPos = GetSpawnPosition();
            GameObject go;

            if (!string.IsNullOrEmpty(prefabPath))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    go.transform.position = spawnPos;
                    go.name = defaultName;
                    Undo.RegisterCreatedObjectUndo(go, $"Create {typeof(T).Name}");
                    Selection.activeGameObject = go;
                    return;
                }
            }

            // Fallback: create empty GO with component
            go = new GameObject(defaultName);
            go.transform.position = spawnPos;
            go.AddComponent<T>();

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
