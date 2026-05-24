using GenBall.Procedure;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Procedure.Game.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        private IGameManagerSystem _gameManager;

        [SetUp]
        public void SetUp()
        {
            _gameManager = new GameManager();
            SystemRepository.Instance.RegisterSystem<IGameManagerSystem>(_gameManager);
        }

        [TearDown]
        public void TearDown()
        {
            SystemRepository.Instance.UnregisterSystem<IGameManagerSystem>();
        }

        [Test]
        public void DefaultProperties()
        {
            Assert.That(_gameManager.GameData, Is.Null);
            Assert.That(_gameManager.CurSaveIndex, Is.EqualTo(0));
        }

        [Test]
        public void GameData_GetSet()
        {
            var data = new GameData();
            _gameManager.GameData = data;
            Assert.That(_gameManager.GameData, Is.SameAs(data));
        }

        [Test]
        public void Mode_GetSet()
        {
            _gameManager.Mode = RunningMode.SaveData | RunningMode.LoadData;
            Assert.That(_gameManager.Mode, Is.EqualTo(RunningMode.SaveData | RunningMode.LoadData));
        }

        [Test]
        public void CurSaveIndex_GetSet()
        {
            _gameManager.CurSaveIndex = 3;
            Assert.That(_gameManager.CurSaveIndex, Is.EqualTo(3));
        }

        [Test]
        public void SaveGame_NoSaveMode_ReturnsTrue()
        {
            _gameManager.Mode = RunningMode.LoadData;
            var result = _gameManager.SaveGame().GetAwaiter().GetResult();
            Assert.That(result, Is.True);
        }

        [Test]
        public void SaveGame_WithSaveMode_NegativeSaveIndex_ReturnsFalse()
        {
            _gameManager.Mode = RunningMode.SaveData;
            _gameManager.CurSaveIndex = -1;
            var result = _gameManager.SaveGame().GetAwaiter().GetResult();
            Assert.That(result, Is.False);
        }

        [Test]
        public void RegisterSaveDataProvider_GetProvider_ReturnsSameInstance()
        {
            var provider = new MockSaveDataProvider("Test");
            _gameManager.RegisterSaveDataProvider(provider);

            var retrieved = _gameManager.GetProvider("Test");
            Assert.That(retrieved, Is.SameAs(provider));
        }

        [Test]
        public void UnregisterSaveDataProvider_GetProvider_ReturnsNull()
        {
            var provider = new MockSaveDataProvider("Test");
            _gameManager.RegisterSaveDataProvider(provider);
            _gameManager.UnregisterSaveDataProvider(provider);

            var retrieved = _gameManager.GetProvider("Test");
            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void GetProvider_UnregisteredKey_ReturnsNull()
        {
            var retrieved = _gameManager.GetProvider("Nonexistent");
            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void LoadGameData_NoLoadMode_ReturnsFalse()
        {
            _gameManager.Mode = RunningMode.SaveData;
            var result = _gameManager.LoadGameData(0).GetAwaiter().GetResult();
            Assert.That(result, Is.False);
        }

        [Test]
        public void Init_UnInit_NoError()
        {
            Assert.DoesNotThrow(() => _gameManager.Init());
            Assert.DoesNotThrow(() => _gameManager.UnInit());
        }

        private class MockSaveDataProvider : ISaveDataProvider
        {
            public string DataKey { get; }
            public string LastCollected { get; private set; }
            public string LastApplied { get; private set; }

            public MockSaveDataProvider(string key)
            {
                DataKey = key;
            }

            public string CollectSaveData()
            {
                LastCollected = "mock_data";
                return LastCollected;
            }

            public void ApplySaveData(string json)
            {
                LastApplied = json;
            }
        }
    }
}
