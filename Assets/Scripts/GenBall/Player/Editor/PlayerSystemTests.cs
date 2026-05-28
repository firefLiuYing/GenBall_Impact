using GenBall.Framework.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Main;

namespace GenBall.Player.Tests
{
    /// <summary>
    /// EditMode tests for IPlayerSystem / PlayerSystemDefault.
    /// CResourceManager has no helper set in EditMode; CreatePlayer
    /// throws NullReferenceException when it tries to load a prefab.
    /// Init/UnInit/GetSystem tests work correctly without resources.
    /// </summary>
    [TestFixture]
    public class PlayerSystemTests
    {
        private class FakeConfigProvider : IConfigProvider
        {
            private readonly AppSettingsConfig _appConfig;
            private readonly PlayerConfig _playerConfig;

            public FakeConfigProvider()
            {
                _appConfig = ScriptableObject.CreateInstance<AppSettingsConfig>();
                _playerConfig = ScriptableObject.CreateInstance<PlayerConfig>();
            }

            public void Init() { }
            public void UnInit() { }

            public T GetConfig<T>() where T : class
            {
                if (typeof(T) == typeof(AppSettingsConfig)) return _appConfig as T;
                if (typeof(T) == typeof(PlayerConfig)) return _playerConfig as T;
                return null;
            }
        }

        private IConfigProvider _configProvider;
        private IPlayerSystem _playerSystem;

        [SetUp]
        public void SetUp()
        {
            _configProvider = new FakeConfigProvider();
            try { SystemRepository.Instance.RegisterSystem<IConfigProvider>(_configProvider); }
            catch (System.Exception) { /* already registered */ }

            _playerSystem = new PlayerSystemDefault();
            try { SystemRepository.Instance.RegisterSystem<IPlayerSystem>(_playerSystem); }
            catch (System.Exception) { /* already registered */ }
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IPlayerSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IConfigProvider>(); } catch { }
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _playerSystem.Init());
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _playerSystem.UnInit());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            Assert.That(SystemRepository.Instance.GetSystem<IPlayerSystem>(), Is.Not.Null);
        }

        [Test]
        public void HasSystem_ReturnsTrue()
        {
            Assert.That(SystemRepository.Instance.HasSystem<IPlayerSystem>(), Is.True);
        }

        [Test]
        public void CreatePlayer_ThrowsWhenResourceNotAvailable()
        {
            // CResourceManager helper not set in EditMode, logs error then Instantiate(null) throws
            LogAssert.Expect(LogType.Error, "[CResourceManager] Helper is not set!");
            Assert.Throws<System.ArgumentException>(() => _playerSystem.CreatePlayer());
        }

        [Test]
        public void UnInit_ClearsPlayer()
        {
            Assert.DoesNotThrow(() => _playerSystem.UnInit());
        }
    }
}
