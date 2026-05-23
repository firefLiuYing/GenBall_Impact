using System.Collections.Generic;
using GenBall.BattleSystem.Buff;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Main;

namespace GenBall.BattleSystem.Tests
{
    /// <summary>
    /// Integration tests for Phase 4 ISystem migration.
    /// Verifies all four systems (IBuffRegistry, IBuffTickSystem, IDamageSystem, IDeathSystem)
    /// can coexist, register, and interact correctly through SystemRepository.
    /// </summary>
    [TestFixture]
    public class Phase4IntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            SystemUpdaterManager.Instance.Resume();
        }

        [TearDown]
        public void TearDown()
        {
            // Unregister in reverse dependency order
            try { SystemRepository.Instance.UnregisterSystem<IDeathSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IDamageSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IBuffTickSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IBuffRegistry>(); } catch { }
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void AllFourSystems_RegisterAndRetrieve()
        {
            var registry = new BuffRegistry();
            var tickSystem = new BuffTickSystem();
            var damageSystem = new DamageSystemDefault();
            var deathSystem = new DeathSystemDefault();

            var repo = SystemRepository.Instance;

            // Act: register in dependency order
            Assert.That(() => repo.RegisterSystem<IBuffRegistry>(registry), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IBuffTickSystem>(tickSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IDamageSystem>(damageSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IDeathSystem>(deathSystem), Throws.Nothing);

            // Assert: all four retrievable with correct identity
            Assert.That(repo.GetSystem<IBuffRegistry>(), Is.SameAs(registry));
            Assert.That(repo.GetSystem<IBuffTickSystem>(), Is.SameAs(tickSystem));
            Assert.That(repo.GetSystem<IDamageSystem>(), Is.SameAs(damageSystem));
            Assert.That(repo.GetSystem<IDeathSystem>(), Is.SameAs(deathSystem));

            // Cleanup: unregister in reverse order
            Assert.That(() => repo.UnregisterSystem<IDeathSystem>(), Throws.Nothing);
            Assert.That(() => repo.UnregisterSystem<IDamageSystem>(), Throws.Nothing);
            Assert.That(() => repo.UnregisterSystem<IBuffTickSystem>(), Throws.Nothing);
            Assert.That(() => repo.UnregisterSystem<IBuffRegistry>(), Throws.Nothing);
        }

        [Test]
        public void BuffTickSystem_RequiresRegistryFirst()
        {
            // BuffTickSystem.Init() calls SystemRepository.GetSystem<IBuffRegistry>().
            // Without registry, GetSystem returns null (logs error, does not throw),
            // but subsequent LogicUpdate would NRE. Registry MUST be registered first.
            var repo = SystemRepository.Instance;

            // Attempt 1: register tick system without registry
            // GetSystem returns null but does not throw
            var tickSystem1 = new BuffTickSystem();
            LogAssert.Expect(LogType.Error, "System GenBall.BattleSystem.Buff.IBuffRegistry is not registered");
            Assert.That(() => repo.RegisterSystem<IBuffTickSystem>(tickSystem1), Throws.Nothing,
                "Registering without registry does not throw, but _registry will be null internally");
            repo.UnregisterSystem<IBuffTickSystem>();

            // Attempt 2: proper order - registry first, then tick
            var registry = new BuffRegistry();
            var tickSystem2 = new BuffTickSystem();
            Assert.That(() => repo.RegisterSystem<IBuffRegistry>(registry), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IBuffTickSystem>(tickSystem2), Throws.Nothing);

            Assert.That(repo.GetSystem<IBuffRegistry>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IBuffTickSystem>(), Is.Not.Null);

            repo.UnregisterSystem<IBuffTickSystem>();
            repo.UnregisterSystem<IBuffRegistry>();
        }

        [Test]
        public void EventFire_DoesNotThrow()
        {
            var repo = SystemRepository.Instance;

            // Register all systems in correct order
            var registry = new BuffRegistry();
            var tickSystem = new BuffTickSystem();
            var damageSystem = new DamageSystemDefault();

            repo.RegisterSystem<IBuffRegistry>(registry);
            repo.RegisterSystem<IBuffTickSystem>(tickSystem);
            repo.RegisterSystem<IDamageSystem>(damageSystem);

            // Create a damage event with a simple GameObject (no IDamageable)
            var testObj = new GameObject("integration_test_target");
            var info = DamageInfo.Create(testObj, 10, new List<string>());

            // Should not throw: system handles missing IDamageable gracefully
            Assert.DoesNotThrow(() => damageSystem.ApplyDamage(info));

            // Cleanup
            Object.DestroyImmediate(testObj);
            repo.UnregisterSystem<IDamageSystem>();
            repo.UnregisterSystem<IBuffTickSystem>();
            repo.UnregisterSystem<IBuffRegistry>();
        }
    }
}
