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
            // Assert
            Assert.That(_gameManager.GameData, Is.Null);
            Assert.That(_gameManager.CurSaveIndex, Is.EqualTo(0));
        }

        [Test]
        public void GameData_GetSet()
        {
            // Arrange
            var data = new GameData();

            // Act
            _gameManager.GameData = data;

            // Assert
            Assert.That(_gameManager.GameData, Is.SameAs(data));
        }

        [Test]
        public void Mode_GetSet()
        {
            // Act
            _gameManager.Mode = RunningMode.SaveData | RunningMode.LoadData;

            // Assert
            Assert.That(_gameManager.Mode, Is.EqualTo(RunningMode.SaveData | RunningMode.LoadData));
        }

        [Test]
        public void CurSaveIndex_GetSet()
        {
            // Act
            _gameManager.CurSaveIndex = 3;

            // Assert
            Assert.That(_gameManager.CurSaveIndex, Is.EqualTo(3));
        }

        [Test]
        public void SaveGame_NoSaveMode_ReturnsTrue()
        {
            // Arrange: Mode has no SaveData flag
            _gameManager.Mode = RunningMode.LoadData;

            // Act: SaveGame early-returns synchronously when SaveData flag not set
            var result = _gameManager.SaveGame().GetAwaiter().GetResult();

            // Assert: early exit returns true
            Assert.That(result, Is.True);
        }

        [Test]
        public void SaveGame_WithSaveMode_NoGameData_ReturnsFalse()
        {
            // Arrange: Mode has SaveData, but GameData is null (default)
            _gameManager.Mode = RunningMode.SaveData;
            // GameData is null by default

            // Act: InternalSaveGame returns false synchronously when GameData is null
            var result = _gameManager.SaveGame().GetAwaiter().GetResult();

            // Assert: fails because GameData is null
            Assert.That(result, Is.False);
        }

        [Test]
        public void Init_UnInit_NoError()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _gameManager.Init());
            Assert.DoesNotThrow(() => _gameManager.UnInit());
        }
    }
}
