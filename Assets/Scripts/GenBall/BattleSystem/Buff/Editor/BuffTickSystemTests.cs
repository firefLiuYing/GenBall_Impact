using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff.Tests
{
    [TestFixture]
    public class BuffTickSystemTests
    {
        private IBuffRegistry _registry;
        private IBuffTickSystem _tickSystem;

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state — SystemRepository is a singleton shared across test fixtures
            if (SystemRepository.Instance.HasSystem<IBuffTickSystem>())
                SystemRepository.Instance.UnregisterSystem<IBuffTickSystem>();
            if (SystemRepository.Instance.HasSystem<IBuffRegistry>())
                SystemRepository.Instance.UnregisterSystem<IBuffRegistry>();
            SystemUpdaterManager.Instance.Resume();
            _registry = new BuffRegistry();
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IBuffTickSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IBuffRegistry>(); } catch { }
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            _tickSystem = new BuffTickSystem();

            // Init is called inside RegisterSystem; registry must be registered first
            Assert.DoesNotThrow(() => SystemRepository.Instance.RegisterSystem<IBuffTickSystem>(_tickSystem));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            _tickSystem = new BuffTickSystem();
            SystemRepository.Instance.RegisterSystem<IBuffTickSystem>(_tickSystem);

            // Unregister calls UnInit, which unsubscribes from CEventRouter
            Assert.DoesNotThrow(() => SystemRepository.Instance.UnregisterSystem<IBuffTickSystem>());
        }

        [Test]
        public void LogicUpdateScope_IsGame()
        {
            _tickSystem = new BuffTickSystem();

            Assert.That(_tickSystem.LogicUpdateScope, Is.EqualTo(SystemScope.Game));
        }

        [Test]
        public void LogicUpdate_EmptyRegistry_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            _tickSystem = new BuffTickSystem();
            SystemRepository.Instance.RegisterSystem<IBuffTickSystem>(_tickSystem);

            // When registry is empty (no active buffs), LogicUpdate should not crash
            Assert.DoesNotThrow(() => _tickSystem.LogicUpdate(0.02f));
        }

        [Test]
        public void Dependency_RegistryRegistered()
        {
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            _tickSystem = new BuffTickSystem();
            SystemRepository.Instance.RegisterSystem<IBuffTickSystem>(_tickSystem);

            var retrievedTick = SystemRepository.Instance.GetSystem<IBuffTickSystem>();
            var retrievedRegistry = SystemRepository.Instance.GetSystem<IBuffRegistry>();

            Assert.That(retrievedTick, Is.Not.Null);
            Assert.That(retrievedRegistry, Is.Not.Null);
            Assert.That(retrievedTick, Is.SameAs(_tickSystem));
            Assert.That(retrievedRegistry, Is.SameAs(_registry));
        }

        [Test]
        public void EventSubscriptions_Init_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            _tickSystem = new BuffTickSystem();

            // Init subscribes to CEventRouter events; verifying no exception
            Assert.DoesNotThrow(() => _tickSystem.Init());
        }
    }
}
