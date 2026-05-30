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
            SystemUpdaterManager.Instance.Resume();

            _pause = new PauseManager();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(_pause);
        }

        [TearDown]
        public void TearDown()
        {
            SystemRepository.Instance.UnregisterSystem<IPauseSystem>();
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void DefaultState_IsUnpaused()
        {
            Assert.That(_pause.IsLogicPaused, Is.False);
            Assert.That(_pause.IsPhysicsPaused, Is.False);
            Assert.That(_pause.StackDepth, Is.EqualTo(0));
        }

        [Test]
        public void PushPause_AllPause_UpdatesState()
        {
            _pause.PushPause(true);

            Assert.That(_pause.IsLogicPaused, Is.True);
            Assert.That(_pause.IsPhysicsPaused, Is.True);
        }

        [Test]
        public void PushPause_LogicOnly_UpdatesState()
        {
            _pause.PushPause(false);

            Assert.That(_pause.IsLogicPaused, Is.True);
            Assert.That(_pause.IsPhysicsPaused, Is.False);
        }

        [Test]
        public void PopPause_RestoresState()
        {
            _pause.PushPause(true);
            _pause.PopPause();

            Assert.That(_pause.IsLogicPaused, Is.False);
            Assert.That(_pause.IsPhysicsPaused, Is.False);
        }

        [Test]
        public void Stack_NestedPauses()
        {
            _pause.PushPause(false);
            Assert.That(_pause.IsPhysicsPaused, Is.False);

            _pause.PushPause(true);
            Assert.That(_pause.IsPhysicsPaused, Is.True);

            _pause.PopPause();
            Assert.That(_pause.IsPhysicsPaused, Is.False);

            _pause.PopPause();
            Assert.That(_pause.IsLogicPaused, Is.False);
        }

        [Test]
        public void PopPause_EmptyStack_NoError()
        {
            Assert.DoesNotThrow(() => _pause.PopPause());
            Assert.That(_pause.IsLogicPaused, Is.False);
        }

        [Test]
        public void StackDepth_TracksCorrectly()
        {
            Assert.That(_pause.StackDepth, Is.EqualTo(0));
            _pause.PushPause(true);
            Assert.That(_pause.StackDepth, Is.EqualTo(1));
            _pause.PushPause(false);
            Assert.That(_pause.StackDepth, Is.EqualTo(2));
            _pause.PopPause();
            Assert.That(_pause.StackDepth, Is.EqualTo(1));
        }

        [Test]
        public void Init_UnInit_NoError()
        {
            Assert.DoesNotThrow(() => _pause.Init());
            Assert.DoesNotThrow(() => _pause.UnInit());
        }
    }
}
