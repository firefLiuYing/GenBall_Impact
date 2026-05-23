using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Map.Tests
{
    [TestFixture]
    public class SceneSystemTests
    {
        private ISceneStateSystem _scene;

        [SetUp]
        public void SetUp()
        {
            _scene = new SceneSystem();
            SystemRepository.Instance.RegisterSystem<ISceneStateSystem>(_scene);
        }

        [TearDown]
        public void TearDown()
        {
            SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();
        }

        private static MapModel CreateTestMapModel()
        {
            var map = ScriptableObject.CreateInstance<MapModel>();

            var scene1 = new SceneModel
            {
                sceneName = "SceneA",
                displayName = "Scene A",
                savePoints = new()
                {
                    new SavePointModel { id = 0, displayName = "Spawn A0", spawnPosition = Vector3.zero, spawnRotation = Quaternion.identity },
                    new SavePointModel { id = 1, displayName = "Checkpoint A1", spawnPosition = Vector3.one, spawnRotation = Quaternion.identity },
                },
                enemyUnits = new()
                {
                    new EnemyUnitModel { id = 0, enemyType = "NormalOrbis", spawnPosition = Vector3.forward, spawnRotation = Quaternion.identity },
                    new EnemyUnitModel { id = 1, enemyType = "EliteOrbis", spawnPosition = Vector3.back, spawnRotation = Quaternion.identity },
                }
            };

            var scene2 = new SceneModel
            {
                sceneName = "SceneB",
                displayName = "Scene B",
                savePoints = new()
                {
                    new SavePointModel { id = 0, displayName = "Spawn B0", spawnPosition = Vector3.right, spawnRotation = Quaternion.identity },
                },
                enemyUnits = new()
                {
                    new EnemyUnitModel { id = 0, enemyType = "BossOrbis", spawnPosition = Vector3.left, spawnRotation = Quaternion.identity },
                }
            };

            map.scenes.Add(scene1);
            map.scenes.Add(scene2);
            return map;
        }

        private static MapSaveData CreateTestMapSaveData()
        {
            return new MapSaveData
            {
                unlockedScenes = new()
                {
                    new SceneSaveData
                    {
                        sceneName = "SceneA",
                        unlockedSavePoints = new() { 0 },
                        killedEnemyUnits = new() { 0 },
                    },
                }
            };
        }

        [Test]
        public void InitializeMapConfig_PopulatesConfigs()
        {
            // Arrange
            var mapModel = CreateTestMapModel();

            // Act
            _scene.InitializeMapConfig(mapModel);

            // Assert: save points are accessible
            var savePointA0 = _scene.GetSavePointModel("SceneA", 0);
            Assert.That(savePointA0, Is.Not.Null);
            Assert.That(savePointA0.displayName, Is.EqualTo("Spawn A0"));

            var savePointA1 = _scene.GetSavePointModel("SceneA", 1);
            Assert.That(savePointA1, Is.Not.Null);
            Assert.That(savePointA1.displayName, Is.EqualTo("Checkpoint A1"));

            var savePointB0 = _scene.GetSavePointModel("SceneB", 0);
            Assert.That(savePointB0, Is.Not.Null);
            Assert.That(savePointB0.displayName, Is.EqualTo("Spawn B0"));

            // Assert: enemy units are accessible
            var enemyA0 = _scene.GetEnemyModel("SceneA", 0);
            Assert.That(enemyA0, Is.Not.Null);
            Assert.That(enemyA0.enemyType, Is.EqualTo("NormalOrbis"));

            var enemyA1 = _scene.GetEnemyModel("SceneA", 1);
            Assert.That(enemyA1, Is.Not.Null);
            Assert.That(enemyA1.enemyType, Is.EqualTo("EliteOrbis"));
        }

        [Test]
        public void InitializeMapConfig_Idempotent()
        {
            // Arrange
            var mapModel1 = CreateTestMapModel();

            var mapModel2 = ScriptableObject.CreateInstance<MapModel>();
            mapModel2.scenes.Add(new SceneModel
            {
                sceneName = "SceneZ",
                displayName = "Should Not Appear",
                savePoints = new(),
                enemyUnits = new(),
            });

            // Act: first call initializes, second is a no-op
            _scene.InitializeMapConfig(mapModel1);
            _scene.InitializeMapConfig(mapModel2);

            // Assert: data from first call is still intact
            var savePoint = _scene.GetSavePointModel("SceneA", 0);
            Assert.That(savePoint, Is.Not.Null);
            Assert.That(savePoint.displayName, Is.EqualTo("Spawn A0"));

            // SceneZ from second call should not exist
            var ghostPoint = _scene.GetSavePointModel("SceneZ", 0);
            Assert.That(ghostPoint, Is.Null);
        }

        [Test]
        public void InitializeSceneStateObjs_TracksState()
        {
            // Arrange
            _scene.InitializeMapConfig(CreateTestMapModel());
            var mapSaveData = CreateTestMapSaveData();

            // Act
            _scene.InitializeSceneStateObjs(mapSaveData);

            // Assert: unlocked save points return only the unlocked ones
            var unlocked = _scene.GetUnlockedSavePointModels("SceneA").ToList();
            Assert.That(unlocked.Count, Is.EqualTo(1));
            Assert.That(unlocked[0].id, Is.EqualTo(0));

            // Assert: unkilled enemies return only the alive ones
            var alive = _scene.GetAllUnKilledEnemyModel("SceneA").ToList();
            Assert.That(alive.Count, Is.EqualTo(1));
            Assert.That(alive[0].id, Is.EqualTo(1));
            Assert.That(alive[0].enemyType, Is.EqualTo("EliteOrbis"));
        }

        [Test]
        public void UnlockSavePoint_NewScene()
        {
            // Arrange: no scene state initialized for SceneB

            // Act
            _scene.UnlockSavePoint("SceneB", 0);

            // Assert: does not throw, and scene state is created lazily
            Assert.DoesNotThrow(() => _scene.UnlockSavePoint("SceneB", 1));
        }

        [Test]
        public void KillEnemyUnit_RemovesFromAliveList()
        {
            // Arrange
            _scene.InitializeMapConfig(CreateTestMapModel());
            _scene.InitializeSceneStateObjs(CreateTestMapSaveData());

            // Pre-assert: enemy 1 is still alive
            var aliveBefore = _scene.GetAllUnKilledEnemyModel("SceneA").ToList();
            Assert.That(aliveBefore.Count, Is.EqualTo(1));
            Assert.That(aliveBefore[0].id, Is.EqualTo(1));

            // Act
            _scene.KillEnemyUnit("SceneA", 1);

            // Assert: no more alive enemies
            var aliveAfter = _scene.GetAllUnKilledEnemyModel("SceneA").ToList();
            Assert.That(aliveAfter.Count, Is.EqualTo(0));
        }

        [Test]
        public void Init_UnInit_ClearsState()
        {
            // Arrange: populate data via a first instance
            var scene1 = new SceneSystem();
            SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();
            SystemRepository.Instance.RegisterSystem<ISceneStateSystem>(scene1);
            scene1.InitializeMapConfig(CreateTestMapModel());

            // Act: unregister the first instance
            SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();

            // Register a fresh instance
            var scene2 = new SceneSystem();
            SystemRepository.Instance.RegisterSystem<ISceneStateSystem>(scene2);

            // Assert: fresh instance has no data
            var savePoint = scene2.GetSavePointModel("SceneA", 0);
            Assert.That(savePoint, Is.Null);
            Assert.That(_scene, Is.Not.SameAs(scene2));
        }
    }
}
