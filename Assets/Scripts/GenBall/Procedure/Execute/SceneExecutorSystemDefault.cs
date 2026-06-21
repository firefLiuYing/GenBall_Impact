using System.Collections.Generic;
using System.Linq;
using GenBall.Enemy;
using GenBall.Enemy.AI;
using GenBall.Event;
using GenBall.Framework.Config;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Event;
using Yueyn.Main;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.Procedure.Execute
{
    public class SceneExecutorSystemDefault : ISceneExecutorSystem
    {
        private ISceneStateSystem _sceneSystem;
        private IPlayerSystem _playerSystem;
        private SceneEventOrchestrator _orchestrator;

        public void Init()
        {
            _sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _playerSystem = SystemRepository.Instance.GetSystem<IPlayerSystem>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void UnInit()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _orchestrator?.UnregisterAll();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _orchestrator?.UnregisterAll();
        }

        public void ExecuteSceneSetup(SceneInitContext context)
        {
            if (context == null)
            {
                Debug.LogError("[SceneExecutorSystem] SceneInitContext is null, skipping scene setup.");
                return;
            }

            // TODO: 敌人出生列表应从 SceneInitContext 获取，待场景配置系统设计
            LoadEnemyUnit();

            // Notify UI to open HUD before spawning entities, so HUD subscribes
            // to stat change events before initial values are fired.
            CEventRouter.Instance.FireNow((int)GlobalEventId.InGameUIReady);

            // Spawn player at specified position
            if (context.SpawnPosition != default || context.SpawnRotation != default)
            {
                _playerSystem.CreatePlayer(context.SpawnPosition, context.SpawnRotation);
            }
            else
            {
                _playerSystem.CreatePlayer();
            }

            // ================================================================
            // [TEMPORARY] Test spawn a Blue Orbis for B-2 validation.
            // Remove this block after runtime verification is complete.
            // ================================================================
            // SpawnTestEnemy();

            // Spawn runtime triggers from editor-placed EventTrigger data before cleanup.
            SpawnTriggers();

            // Register orchestrator: listens for placed events fired by triggers and
            // dispatches to the appropriate behavior systems.
            RegisterOrchestrator();

            // Spawn bonfires from editor-placed SavePointConfig before cleanup.
            SpawnBonfires();

            // Hide editor placeholders for dynamically spawned objects (enemies, triggers, save points).
            // These objects were placed in the editor for configuration and visual reference;
            // at runtime they are replaced by dynamically spawned instances.
            CleanupDynamicPlaceables();

            // Notify: scene is fully ready, player can start playing.
            CEventRouter.Instance.FireNow((int)GlobalEventId.SceneReady);
            Debug.Log("[SceneExecutorSystem] Scene setup complete, SceneReady fired.");
        }

        private void RegisterOrchestrator()
        {
            if (_orchestrator != null) return;
            _orchestrator = new SceneEventOrchestrator();
            _orchestrator.RegisterAll();
        }

        // ── Scene Setup Helpers ──────────────────────────────────────

        /// <summary>
        /// Spawn bonfire prefabs from editor-placed SavePointConfig components.
        /// Bonfires with initiallyActive=false are skipped (awaiting event-based unlock).
        /// </summary>
        private static void SpawnBonfires()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var spawned = 0;
            foreach (var root in roots)
            {
                var configs = root.GetComponentsInChildren<SavePointConfig>(true);
                foreach (var sp in configs)
                {
                    if (string.IsNullOrEmpty(sp.BonfireType)) continue;
                    if (!sp.InitiallyActive) continue;

                    if (!BonfirePrefabRegistry.TryGetPath(sp.BonfireType, out var path))
                    {
                        Debug.LogWarning($"[SceneExecutorSystem] Bonfire type '{sp.BonfireType}' not registered in BonfirePrefabRegistry.");
                        continue;
                    }

                    var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[SceneExecutorSystem] Bonfire prefab not found at: {path}");
                        continue;
                    }

                    var go = Object.Instantiate(prefab, sp.transform.position, sp.transform.rotation);
                    go.name = $"[{sp.Index}] {sp.DisplayName} (Runtime)";

                    var savePoint = go.GetComponent<SavePoint>();
                    if (savePoint == null)
                        savePoint = go.AddComponent<SavePoint>();
                    savePoint.SetConfig(sp.DisplayName);

                    spawned++;
                }
            }
            if (spawned > 0)
                Debug.Log($"[SceneExecutorSystem] Spawned {spawned} bonfire(s).");
        }

        /// <summary>
        /// Spawn runtime event triggers from editor-placed EventTrigger components.
        /// Reads baked SceneTriggerData from SceneConfigCollection and creates
        /// RuntimeEventTrigger GameObjects that handle the actual trigger logic.
        /// </summary>
        private static void SpawnTriggers()
        {
            var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
            if (configProvider == null) return;

            var config = configProvider.GetConfig<SceneConfigCollection>();
            if (config == null || config.scenes.Count == 0) return;

            var sceneName = SceneManager.GetActiveScene().name;
            var entry = config.scenes.FirstOrDefault(s => s.sceneName == sceneName);
            if (entry == null || entry.triggers == null || entry.triggers.Count == 0) return;

            foreach (var data in entry.triggers)
            {
                var go = new GameObject($"[Trigger] {data.triggerName} (Runtime)");
                go.transform.position = data.position;

                var trigger = go.AddComponent<RuntimeEventTrigger>();
                trigger.Initialize(data);
            }

            if (entry.triggers.Count > 0)
                Debug.Log($"[SceneExecutorSystem] Spawned {entry.triggers.Count} runtime trigger(s).");
        }
        /// since they have been replaced by dynamically spawned instances.
        /// Uses SetActive(false) rather than Destroy to avoid issues with
        /// other systems that may hold references.
        /// </summary>
        private static void CleanupDynamicPlaceables()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var cleaned = 0;
            foreach (var root in roots)
            {
                var placeables = root.GetComponentsInChildren<IScenePlaceable>(true);
                foreach (var p in placeables)
                {
                    if (p.IsDynamic && p is MonoBehaviour mb && mb.gameObject.activeSelf)
                    {
                        mb.gameObject.SetActive(false);
                        cleaned++;
                    }
                }
            }
            if (cleaned > 0)
                Debug.Log($"[SceneExecutorSystem] Cleaned up {cleaned} dynamic placeables.");
        }

        /// <summary>
        /// [TEMPORARY] Spawn a single NormalOrbis for BattleEntity migration testing.
        /// TODO: Remove after B-2 runtime verification.
        /// </summary>
        private void SpawnTestEnemy()
        {
            const string testPrefabPath = "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/NormalOrbis.prefab";

            var prefab = CResourceManager.Instance.LoadSync<GameObject>(testPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[SceneExecutorSystem] Test enemy prefab not found, skipping test spawn.");
                return;
            }

            var player = _playerSystem.Player;
            var spawnPos = player != null
                ? player.transform.position + player.transform.forward * 5f
                : new Vector3(0, 0, 5);
            var spawnRot = Quaternion.identity;

            var go = Object.Instantiate(prefab, spawnPos, spawnRot);
            go.name = "[TEST] NormalOrbis";

            var configRef = go.GetComponent<EnemyConfigReference>();
            var config = configRef?.Config ?? ScriptableObject.CreateInstance<EnemyConfigSo>();
            var aiConfig = configRef?.AiConfig;

            EnemyEntityFactory.AssembleEnemy(go, config, aiConfig);

            Debug.Log($"[SceneExecutorSystem] Test enemy spawned at {spawnPos}");
        }

        // EnemyPrefabPaths replaced by EnemyPrefabRegistry for centralized registration.

        /// <summary>
        /// [TEMPORARY] 从 ISceneStateSystem 加载敌人。
        /// TODO: 替换为从 SceneInitContext 获取敌人列表，待场景配置系统设计。
        /// </summary>
        private void LoadEnemyUnit()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            var enemyUnitModels = _sceneSystem.GetAllUnKilledEnemyModel(sceneName);
            foreach (var enemyUnitModel in enemyUnitModels)
            {
                if (!EnemyPrefabRegistry.TryGetPath(enemyUnitModel.enemyType, out var path))
                {
                    Debug.LogWarning($"[SceneExecutorSystem] No prefab path registered for enemy type: {enemyUnitModel.enemyType}");
                    continue;
                }

                var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
                var go = Object.Instantiate(prefab, enemyUnitModel.spawnPosition, enemyUnitModel.spawnRotation);

                var configRef = go.GetComponent<EnemyConfigReference>();
                EnemyConfigSo config = configRef?.Config;
                EnemyAIConfigSo aiConfig = configRef?.AiConfig;

                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<EnemyConfigSo>();
                }

                EnemyEntityFactory.AssembleEnemy(go, config, aiConfig);
            }
        }
    }
}
