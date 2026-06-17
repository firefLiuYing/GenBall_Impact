#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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

            // 4. Get scene display name from SceneConfig component
            var sceneConfig = Object.FindObjectOfType<SceneConfig>();
            entry.displayName = sceneConfig != null ? sceneConfig.DisplayName : sceneName;

            // 5. Clear existing lists and repopulate
            entry.savePoints.Clear();
            entry.enemySpawns.Clear();
            entry.triggers.Clear();
            entry.mechanisms.Clear();

            var grouped = placeables
                .OrderBy(p => p.Category)
                .ThenBy(p => p.DisplayLabel)
                .ToList();

            // Assign IDs per category
            var categoryCounters = new Dictionary<string, int>();
            foreach (var p in grouped)
            {
                if (!categoryCounters.ContainsKey(p.Category))
                    categoryCounters[p.Category] = 0;
                p.Id = categoryCounters[p.Category]++;
            }

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
