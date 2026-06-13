using System.Collections.Generic;
using GenBall.Enemy;
using GenBall.Enemy.AI;
using GenBall.Event;
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

        public void Init()
        {
            _sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _playerSystem = SystemRepository.Instance.GetSystem<IPlayerSystem>();
        }

        public void UnInit() { }

        public void ExecuteSceneSetup(SceneInitContext context)
        {
            if (context == null)
            {
                Debug.LogError("[SceneExecutorSystem] SceneInitContext is null, skipping scene setup.");
                return;
            }

            // TODO: 敌人出生列表应从 SceneInitContext 获取，待场景配置系统设计
            LoadEnemyUnit();

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
            SpawnTestEnemy();

            // Notify: scene is ready — UI layer listens and opens HUD
            CEventRouter.Instance.FireNow((int)GlobalEventId.SceneReady);
            Debug.Log("[SceneExecutorSystem] Scene setup complete, SceneReady fired.");
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

        private static readonly Dictionary<string, string> EnemyPrefabPaths = new()
        {
            { "NormalOrbis", "Assets/AssetBundles/Common/Orbis/NormalOrbis/Prefab/NormalOrbis.prefab" },
        };

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
                if (!EnemyPrefabPaths.TryGetValue(enemyUnitModel.enemyType, out var path))
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
