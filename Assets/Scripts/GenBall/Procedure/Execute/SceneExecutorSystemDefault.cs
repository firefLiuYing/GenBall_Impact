using System.Collections.Generic;
using GenBall.Enemy;
using GenBall.Enemy.AI;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yueyn.Main;
using Yueyn.Resource;
using Object = UnityEngine.Object;

namespace GenBall.Procedure.Execute
{
    public class SceneExecutorSystemDefault : ISceneExecutorSystem
    {
        private IGameManagerSystem _gameManager;
        private ISceneStateSystem _sceneSystem;
        private ITeleportSystem _teleportSystem;
        private IPlayerSystem _playerSystem;

        public void Init()
        {
            _gameManager = SystemRepository.Instance.GetSystem<IGameManagerSystem>();
            _sceneSystem = SystemRepository.Instance.GetSystem<ISceneStateSystem>();
            _teleportSystem = SystemRepository.Instance.GetSystem<ITeleportSystem>();
            _playerSystem = SystemRepository.Instance.GetSystem<IPlayerSystem>();
        }

        public void UnInit() { }

        public void ExecuteSceneSetup()
        {
            var gameData = _gameManager.GameData;
            if (gameData == null)
            {
                Debug.LogWarning("[SceneExecutorSystem] GameData is null, skipping scene setup.");
                return;
            }

            // Reset cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Load enemies (BattleEntity framework)
            LoadEnemyUnit();

            // Initialize UI (new MVP)
            MainHudFormLogic.Open();

            // Spawn player at teleport target or default position
            var loadSystem = SystemRepository.Instance.GetSystem<ISceneLoadSystem>();
            var targetSavePoint = loadSystem.TargetSavePoint;
            if (targetSavePoint != null)
            {
                _playerSystem.CreatePlayer(targetSavePoint.spawnPosition, targetSavePoint.spawnRotation);
            }
            else
            {
                _playerSystem.CreatePlayer();
            }
            _teleportSystem.IsTeleporting = false;

            // ================================================================
            // [TEMPORARY] Test spawn a Blue Orbis near the player for B-2 validation.
            // Remove this block after runtime verification is complete.
            // ================================================================
            SpawnTestEnemy();
        }

        /// <summary>
        /// [TEMPORARY] Spawn a single NormalOrbis near the player for BattleEntity migration testing.
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

            // Spawn in front of the player (or at origin if player doesn't exist)
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

                // Try to get configs from EnemyConfigReference on the prefab
                var configRef = go.GetComponent<EnemyConfigReference>();
                EnemyConfigSo config = configRef?.Config;
                EnemyAIConfigSo aiConfig = configRef?.AiConfig;

                // Fallback: create default config. TODO: proper config loading from IConfigProvider
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<EnemyConfigSo>();
                }

                EnemyEntityFactory.AssembleEnemy(go, config, aiConfig);
            }
        }
    }
}
