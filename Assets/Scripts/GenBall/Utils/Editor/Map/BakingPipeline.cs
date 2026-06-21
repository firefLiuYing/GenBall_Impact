#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using GenBall.Event;
using GenBall.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GenBall.Utils.Editor.Map
{
    /// <summary>
    /// Centralized baking pipeline that collects IScenePlaceable components from the active scene
    /// and writes them into the global SceneConfigCollection asset.
    /// </summary>
    public static class BakingPipeline
    {
        private const string ConfigAssetPath = "Assets/Resources/Configs/SceneConfigCollection.asset";
        private const string ConfigDirectory = "Assets/Resources/Configs";

        [MenuItem("Tools/Bake Current Scene")]
        public static void BakeCurrentSceneStandalone()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                Debug.LogWarning("[BakingPipeline] No active scene to bake.");
                return;
            }

            var placeables = new System.Collections.Generic.List<IScenePlaceable>();
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var components = root.GetComponentsInChildren<MonoBehaviour>(true)
                    .OfType<IScenePlaceable>();
                placeables.AddRange(components);
            }

            BakeCurrentScene(placeables);
        }

        /// <summary>
        /// Run validation on all IScenePlaceable objects in the active scene.
        /// Returns true if all pass, false with collected error messages.
        /// </summary>
        public static bool RunValidation(
            List<IScenePlaceable> placeables, out List<string> errors)
        {
            errors = new List<string>();
            foreach (var p in placeables)
            {
                var error = p.Validate();
                if (error != null)
                {
                    errors.Add($"[{p.Category}] {p.DisplayLabel}: {error}");
                }
            }
            return errors.Count == 0;
        }

        /// <summary>
        /// Bake all IScenePlaceable objects from the active scene into the SceneConfigCollection.
        /// </summary>
        public static void BakeCurrentScene(List<IScenePlaceable> placeables)
        {
            var scene = SceneManager.GetActiveScene();
            var sceneName = scene.name;

            // 1. Validate
            if (!RunValidation(placeables, out var errors))
            {
                // Also check PlacedEventTable for ID conflicts
                var placedTable = AssetDatabase.LoadAssetAtPath<PlacedEventTable>(
                    "Assets/Resources/Configs/PlacedEventTable.asset");
                if (placedTable != null)
                {
                    var tableError = placedTable.ValidateNoConflicts();
                    if (tableError != null)
                    {
                        errors.Add($"[PlacedEventTable] {tableError}");
                    }
                }
            }

            if (errors.Count > 0)
            {
                var errorMsg = string.Join("\n", errors);
                EditorUtility.DisplayDialog("Validation Failed",
                    $"Found {errors.Count} error(s):\n\n{errorMsg}", "OK");
                Debug.LogError($"[BakingPipeline] Validation failed:\n{errorMsg}");
                return;
            }

            // 2. Load or create SceneConfigCollection
            var config = GetOrCreateConfig();

            // 3. Find or create the entry for this scene
            var entry = config.scenes.FirstOrDefault(s => s.sceneName == sceneName);
            if (entry == null)
            {
                entry = new SceneConfigEntry { sceneName = sceneName };
                config.scenes.Add(entry);
            }

            // 4. Get scene display name and default save point
            var sceneConfig = Object.FindObjectOfType<SceneConfig>();
            var displayNameKey = $"MapEditor_{sceneName}_DisplayName";
            var prefsDisplayName = EditorPrefs.GetString(displayNameKey, "");
            entry.displayName = !string.IsNullOrEmpty(prefsDisplayName) ? prefsDisplayName
                : (sceneConfig != null ? sceneConfig.DisplayName : sceneName);
            var savePointKey = $"MapEditor_{sceneName}_DefaultSavePointId";
            entry.defaultSavePointId = EditorPrefs.GetInt(savePointKey, 0);

            // 5. Clear existing lists and repopulate
            entry.savePoints.Clear();
            entry.enemySpawns.Clear();
            entry.triggers.Clear();
            entry.mechanisms.Clear();

            // Stable ID assignment per category.
            // Placeables with an existing ID (Id >= 0) keep it unless duplicated.
            // New placeables (Id < 0) get max(existing) + 1 for that category.
            var byCategory = placeables.GroupBy(p => p.Category);
            foreach (var group in byCategory)
            {
                var seenIds = new HashSet<int>();
                int maxId = -1;

                // First pass: collect existing IDs, detect duplicates, find max
                foreach (var p in group)
                {
                    if (p.Id >= 0)
                    {
                        if (seenIds.Contains(p.Id))
                            p.Id = -1; // duplicate — reassign
                        else
                        {
                            seenIds.Add(p.Id);
                            if (p.Id > maxId) maxId = p.Id;
                        }
                    }
                }

                // Second pass: assign new IDs to unassigned (Id < 0)
                int nextId = maxId + 1;
                foreach (var p in group.OrderBy(p => p.DisplayLabel))
                {
                    if (p.Id < 0)
                        p.Id = nextId++;
                }
            }

            var grouped = placeables
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Id)
                .ToList();

            foreach (var p in grouped)
            {
                var data = p.BakeToConfigData();
                switch (data)
                {
                    case SavePointData sp:
                        entry.savePoints.Add(sp);
                        break;
                    case EnemySpawnData es:
                        entry.enemySpawns.Add(es);
                        break;
                    case SceneTriggerData st:
                        entry.triggers.Add(st);
                        break;
                    case MechanismData me:
                        entry.mechanisms.Add(me);
                        break;
                }

                // Dynamic placeables (enemies) stay visible after baking.
                // They are hidden/destroyed at runtime by SceneExecutorSystemDefault.
            }

            // 6. Save
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 7. Mark scene dirty (IDs were assigned back to components)
            EditorSceneManager.MarkSceneDirty(scene);

            // 8. Update GameObject names to reflect assigned IDs
            foreach (var p in grouped)
            {
                if (p is MonoBehaviour mb)
                {
                    Undo.RecordObject(mb.gameObject, "Bake Name");
                    mb.gameObject.name = p.DisplayLabel;  // e.g. "[0] Initial Bonfire"
                }
            }

            Debug.Log($"[BakingPipeline] Baked scene '{sceneName}': " +
                $"{entry.savePoints.Count} save points, {entry.enemySpawns.Count} enemies, " +
                $"{entry.triggers.Count} triggers, {entry.mechanisms.Count} mechanisms");
        }

        /// <summary>
        /// Get existing config or create a new one at the standard path.
        /// </summary>
        public static SceneConfigCollection GetOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<SceneConfigCollection>(ConfigAssetPath);
            if (config != null) return config;

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(ConfigDirectory))
            {
                // Create Resources/Configs directory chain
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "Configs");
            }

            config = ScriptableObject.CreateInstance<SceneConfigCollection>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BakingPipeline] Created new SceneConfigCollection at {ConfigAssetPath}");
            return config;
        }
    }
}
#endif
