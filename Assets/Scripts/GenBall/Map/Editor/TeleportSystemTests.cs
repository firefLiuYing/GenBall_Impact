using GenBall.Procedure.Execute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Main;

namespace GenBall.Map.Tests
{
    [TestFixture]
    public class TeleportSystemTests
    {
        private ITeleportSystem _teleport;

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state — SystemRepository is a singleton shared across test fixtures
            if (SystemRepository.Instance.HasSystem<ITeleportSystem>())
                SystemRepository.Instance.UnregisterSystem<ITeleportSystem>();
            if (SystemRepository.Instance.HasSystem<ISceneStateSystem>())
                SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();
            if (SystemRepository.Instance.HasSystem<ISceneLoadSystem>())
                SystemRepository.Instance.UnregisterSystem<ISceneLoadSystem>();

            // Register ISceneLoadSystem first (new dependency of TeleportSystem)
            var loadSystem = new SceneLoadSystemDefault();
            SystemRepository.Instance.RegisterSystem<ISceneLoadSystem>(loadSystem);

            // Register ISceneStateSystem (dependency of TeleportSystem)
            var scene = new SceneSystem();
            SystemRepository.Instance.RegisterSystem<ISceneStateSystem>(scene);

            _teleport = new TeleportSystem();
            SystemRepository.Instance.RegisterSystem<ITeleportSystem>(_teleport);
        }

        [TearDown]
        public void TearDown()
        {
            // Unregister in reverse order
            SystemRepository.Instance.UnregisterSystem<ITeleportSystem>();
            SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();
            SystemRepository.Instance.UnregisterSystem<ISceneLoadSystem>();
        }

        private static MapModel CreateTestMapModel()
        {
            var map = ScriptableObject.CreateInstance<MapModel>();
            map.scenes.Add(new SceneModel
            {
                sceneName = "TestScene",
                displayName = "Test Scene",
                savePoints = new()
                {
                    new SavePointModel { id = 0, displayName = "SavePoint 0", spawnPosition = Vector3.zero, spawnRotation = Quaternion.identity },
                    new SavePointModel { id = 1, displayName = "SavePoint 1", spawnPosition = Vector3.one, spawnRotation = Quaternion.identity },
                },
                enemyUnits = new(),
            });
            return map;
        }

        [Test]
        public void Teleport_ValidRequest_SetsIsTeleporting()
        {
            // Arrange
            var mapModel = CreateTestMapModel();
            SystemRepository.Instance.GetSystem<ISceneStateSystem>().InitializeMapConfig(mapModel);

            var request = new TeleportRequestInfo { SceneName = "TestScene", SavePointIndex = 0 };

            // Act: Teleport sets IsTeleporting before calling LoadScene.
            // In EditMode, LoadScene returns early (no async op), but the flag is set.
            var result = _teleport.Teleport(request);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_teleport.IsTeleporting, Is.True);
        }

        [Test]
        public void Teleport_InvalidScene_ReturnsFalse()
        {
            // Arrange
            var mapModel = CreateTestMapModel();
            SystemRepository.Instance.GetSystem<ISceneStateSystem>().InitializeMapConfig(mapModel);

            var request = new TeleportRequestInfo { SceneName = "NonExistentScene", SavePointIndex = 0 };

            // Act
            LogAssert.Expect(LogType.Error, "[TeleportSystem] SavePoint not found: scene=NonExistentScene, index=0");
            var result = _teleport.Teleport(request);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(_teleport.IsTeleporting, Is.False);
        }

        [Test]
        public void Teleport_DoubleCall_ReturnsFalse()
        {
            // Arrange
            var mapModel = CreateTestMapModel();
            SystemRepository.Instance.GetSystem<ISceneStateSystem>().InitializeMapConfig(mapModel);

            var request = new TeleportRequestInfo { SceneName = "TestScene", SavePointIndex = 0 };

            // Act: first call
            _teleport.Teleport(request);

            // Act: second call should fail due to re-entrancy guard
            var result = _teleport.Teleport(request);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Teleport_NullSceneName_ReturnsFalse()
        {
            // Arrange
            var mapModel = CreateTestMapModel();
            SystemRepository.Instance.GetSystem<ISceneStateSystem>().InitializeMapConfig(mapModel);

            var request = new TeleportRequestInfo { SceneName = null, SavePointIndex = 0 };

            // Act
            LogAssert.Expect(LogType.Error, "[TeleportSystem] SceneName is null or empty.");
            var result = _teleport.Teleport(request);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(_teleport.IsTeleporting, Is.False);
        }

        [Test]
        public void Teleport_EmptySceneName_ReturnsFalse()
        {
            // Arrange
            var mapModel = CreateTestMapModel();
            SystemRepository.Instance.GetSystem<ISceneStateSystem>().InitializeMapConfig(mapModel);

            var request = new TeleportRequestInfo { SceneName = "", SavePointIndex = 0 };

            // Act
            LogAssert.Expect(LogType.Error, "[TeleportSystem] SceneName is null or empty.");
            var result = _teleport.Teleport(request);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Init_UnInit_NoError()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _teleport.Init());
            Assert.DoesNotThrow(() => _teleport.UnInit());
        }

        [Test]
        public void DefaultState_IsTeleportingFalse()
        {
            // Assert
            Assert.That(_teleport.IsTeleporting, Is.False);
            // CachedSavePointModel removed from interface — save point info is
            // passed via SceneInitContext to ISceneExecutorSystem instead.
        }
    }
}
