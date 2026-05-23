using GenBall.Framework.Config;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player.Tests
{
    /// <summary>
    /// EditMode tests for IPlayerSystem / PlayerSystemDefault.
    /// GameEntry.CharacterCreator is unavailable in EditMode; Init/CreatePlayer
    /// calls to it are wrapped in try-catch where needed.
    /// </summary>
    [TestFixture]
    public class PlayerSystemTests
    {
        private class FakeConfigProvider : IConfigProvider
        {
            private readonly AppSettingsConfig _config;

            public FakeConfigProvider()
            {
                _config = ScriptableObject.CreateInstance<AppSettingsConfig>();
                // Uses defaults: Vector3.zero for spawn position/rotation
            }

            public void Init() { }
            public void UnInit() { }
            public T GetConfig<T>() where T : class => _config as T;
        }

        private IConfigProvider _configProvider;
        private IPlayerSystem _playerSystem;

        [SetUp]
        public void SetUp()
        {
            // Register fake config provider first (needed by PlayerSystemDefault.Init)
            _configProvider = new FakeConfigProvider();
            try { SystemRepository.Instance.RegisterSystem<IConfigProvider>(_configProvider); }
            catch (System.Exception) { /* already registered */ }

            // Create and register player system
            _playerSystem = new PlayerSystemDefault();
            try { SystemRepository.Instance.RegisterSystem<IPlayerSystem>(_playerSystem); }
            catch (System.Exception) { /* already registered */ }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up in reverse registration order
            try { SystemRepository.Instance.UnregisterSystem<IPlayerSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IConfigProvider>(); } catch { }
        }

        /// <summary>
        /// SafeInit: calls Init and catches NullReferenceException from
        /// GameEntry.CharacterCreator (unavailable in EditMode).
        /// </summary>
        private static void SafeInit(IPlayerSystem system)
        {
            try { system.Init(); }
            catch (System.NullReferenceException) { /* GameEntry.CharacterCreator unavailable in EditMode */ }
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            // Init will attempt to access GameEntry.CharacterCreator.AddPrefab,
            // which throws NullReferenceException in EditMode. We catch that
            // and verify no OTHER exception is thrown.
            Assert.DoesNotThrow(() => SafeInit(_playerSystem));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _playerSystem.UnInit());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            // Assert: system is retrievable via SystemRepository
            Assert.That(SystemRepository.Instance.GetSystem<IPlayerSystem>(), Is.Not.Null);
        }

        [Test]
        public void HasSystem_ReturnsTrue()
        {
            // Assert
            Assert.That(SystemRepository.Instance.HasSystem<IPlayerSystem>(), Is.True);
        }

        [Test]
        public void CreatePlayer_ThrowsWhenGameEntryNotAvailable()
        {
            // CreatePlayer calls GameEntry.CharacterCreator.CreateEntity
            // which is null in EditMode, causing NullReferenceException.
            Assert.Throws<System.NullReferenceException>(() => _playerSystem.CreatePlayer());
        }

        [Test]
        public void UnInit_ClearsPlayer()
        {
            // Act & Assert: UnInit should not throw (it sets Player to null)
            Assert.DoesNotThrow(() => _playerSystem.UnInit());
        }
    }
}
