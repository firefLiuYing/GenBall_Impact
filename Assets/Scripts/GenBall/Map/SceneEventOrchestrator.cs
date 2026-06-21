using GenBall.Enemy;
using GenBall.Enemy.AI;
using GenBall.Event.Params;
using UnityEngine;
using Yueyn.Event;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.Map
{
    /// <summary>
    /// Bridges placed events (fired by triggers) to behavior systems.
    /// Manages event subscriptions scoped to the current scene.
    ///
    /// Created by SceneExecutorSystemDefault per scene setup, cleaned up on scene unload.
    /// To add a new event→behavior mapping, add a Subscribe line in RegisterAll()
    /// and a matching handler method.
    /// </summary>
    public class SceneEventOrchestrator
    {
        private readonly System.Action<SpawnEnemyParams> _onSpawnEnemy;

        public SceneEventOrchestrator()
        {
            _onSpawnEnemy = HandleSpawnEnemy;
        }

        public void RegisterAll()
        {
            CEventRouter.Instance.Subscribe(6001, _onSpawnEnemy);
            Debug.Log("[SceneEventOrchestrator] Registered.");
        }

        public void UnregisterAll()
        {
            CEventRouter.Instance.Unsubscribe(6001, _onSpawnEnemy);
            Debug.Log("[SceneEventOrchestrator] Unregistered.");
        }

        // ── Handlers ──────────────────────────────────────────────

        private static void HandleSpawnEnemy(SpawnEnemyParams p)
        {
            if (string.IsNullOrEmpty(p.enemyType))
            {
                Debug.LogWarning("[Orchestrator] SpawnEnemy: missing enemyType.");
                return;
            }

            if (!EnemyPrefabRegistry.TryGetPath(p.enemyType, out var path))
            {
                Debug.LogWarning($"[Orchestrator] No prefab for enemy type: {p.enemyType}");
                return;
            }

            var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[Orchestrator] Prefab not found: {path}");
                return;
            }

            var go = Object.Instantiate(prefab, p.spawnPosition, p.spawnRotation);
            go.name = $"[Orchestrator] {p.enemyType}";

            var configRef = go.GetComponent<EnemyConfigReference>();
            var config = configRef?.Config ?? ScriptableObject.CreateInstance<EnemyConfigSo>();
            var aiConfig = configRef?.AiConfig;

            config.detectRange = p.detectRadius;

            EnemyEntityFactory.AssembleEnemy(go, config, aiConfig);

            Debug.Log($"[Orchestrator] Spawned {p.enemyType} at {p.spawnPosition}");
        }
    }
}
