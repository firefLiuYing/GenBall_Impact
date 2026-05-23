using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Framework.Entity.Tests
{
    [TestFixture]
    public class EntityUpdateSystemTests
    {
        private EntityUpdateSystem _system;

        private class TestFrameEntity : IEntityFrameUpdate
        {
            public int TickCount;
            public void FrameUpdate(float dt) { TickCount++; }
        }

        private class TestLogicEntity : IEntityLogicUpdate
        {
            public int TickCount;
            public void LogicUpdate(float dt) { TickCount++; }
        }

        [SetUp]
        public void SetUp()
        {
            _system = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_system);
            SystemUpdaterManager.Instance.Resume();
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            // Init is called by RegisterSystem, so we test
            // that re-registering after cleanup works without exception.
            // Cleanup first, then re-register to exercise Init.
            SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            Assert.That(() => SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(new EntityUpdateSystem()), Throws.Nothing);
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            Assert.That(() => SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>(), Throws.Nothing);
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            Assert.That(SystemRepository.Instance.GetSystem<IEntityUpdateSystem>(), Is.Not.Null);
        }

        [Test]
        public void FrameUpdateScope_IsGame()
        {
            Assert.That(_system.FrameUpdateScope, Is.EqualTo(SystemScope.Game));
        }

        [Test]
        public void LogicUpdateScope_IsGame()
        {
            Assert.That(_system.LogicUpdateScope, Is.EqualTo(SystemScope.Game));
        }

        [Test]
        public void FrameUpdate_EmptyList_DoesNotThrow()
        {
            Assert.That(() => _system.FrameUpdate(0.02f), Throws.Nothing);
        }

        [Test]
        public void LogicUpdate_EmptyList_DoesNotThrow()
        {
            Assert.That(() => _system.LogicUpdate(0.02f), Throws.Nothing);
        }

        [Test]
        public void AddFrameUpdate_EntityReceivesTick()
        {
            var entity = new TestFrameEntity();
            _system.AddFrameUpdate(entity);
            _system.FrameUpdate(0.02f);

            Assert.That(entity.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void AddLogicUpdate_EntityReceivesTick()
        {
            var entity = new TestLogicEntity();
            _system.AddLogicUpdate(entity);
            _system.LogicUpdate(0.02f);

            Assert.That(entity.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void RemoveFrameUpdate_EntityStopsReceivingTick()
        {
            var entity = new TestFrameEntity();
            _system.AddFrameUpdate(entity);
            _system.RemoveFrameUpdate(entity);
            _system.FrameUpdate(0.02f);

            Assert.That(entity.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveLogicUpdate_EntityStopsReceivingTick()
        {
            var entity = new TestLogicEntity();
            _system.AddLogicUpdate(entity);
            _system.RemoveLogicUpdate(entity);
            _system.LogicUpdate(0.02f);

            Assert.That(entity.TickCount, Is.EqualTo(0));
        }

        [Test]
        public void MultipleEntities_AllReceiveTick()
        {
            var e1 = new TestFrameEntity();
            var e2 = new TestFrameEntity();
            var e3 = new TestFrameEntity();

            _system.AddFrameUpdate(e1);
            _system.AddFrameUpdate(e2);
            _system.AddFrameUpdate(e3);
            _system.FrameUpdate(0.02f);

            Assert.That(e1.TickCount, Is.EqualTo(1));
            Assert.That(e2.TickCount, Is.EqualTo(1));
            Assert.That(e3.TickCount, Is.EqualTo(1));
        }
    }
}
