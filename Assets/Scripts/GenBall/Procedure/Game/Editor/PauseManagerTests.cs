using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Procedure.Game.Tests
{
    [TestFixture]
    public class PauseManagerTests
    {
        private IPauseSystem _pause;

        [SetUp]
        public void SetUp()
        {
            // Ensure SystemUpdaterManager is in a clean state
            SystemUpdaterManager.Instance.Resume();

            _pause = new PauseManager();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(_pause);
        }

        [TearDown]
        public void TearDown()
        {
            SystemRepository.Instance.UnregisterSystem<IPauseSystem>();

            // Leave SystemUpdaterManager in a clean state for other tests
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void DefaultState_IsUnpaused()
        {
            // Assert
            Assert.That(_pause.IsPaused, Is.False);
        }

        [Test]
        public void SetPause_UpdatesState()
        {
            // Act
            _pause.SetPause(true);

            // Assert
            Assert.That(_pause.IsPaused, Is.True);
        }

        [Test]
        public void SetPause_False_ResumesSystemUpdater()
        {
            // Arrange: first pause, then unpause to test resume
            _pause.SetPause(true);

            // Act
            _pause.SetPause(false);

            // Assert
            Assert.That(_pause.IsPaused, Is.False);
            Assert.That(SystemUpdaterManager.Instance.IsPaused, Is.False);
        }

        [Test]
        public void SetPause_True_PausesSystemUpdater()
        {
            // Act
            _pause.SetPause(true);

            // Assert
            Assert.That(SystemUpdaterManager.Instance.IsPaused, Is.True);
        }

        [Test]
        public void SetPause_Toggle()
        {
            // Act: pause then unpause
            _pause.SetPause(true);
            Assert.That(_pause.IsPaused, Is.True);
            Assert.That(SystemUpdaterManager.Instance.IsPaused, Is.True);

            _pause.SetPause(false);
            Assert.That(_pause.IsPaused, Is.False);
            Assert.That(SystemUpdaterManager.Instance.IsPaused, Is.False);
        }

        [Test]
        public void Init_UnInit_NoError()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _pause.Init());
            Assert.DoesNotThrow(() => _pause.UnInit());
        }
    }
}
