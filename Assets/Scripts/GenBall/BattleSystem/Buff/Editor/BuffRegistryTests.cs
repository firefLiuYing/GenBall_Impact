using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff.Tests
{
    [TestFixture]
    public class BuffRegistryTests
    {
        private IBuffRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            SystemUpdaterManager.Instance.Resume();
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IBuffRegistry>(); } catch { }
            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            _registry = new BuffRegistry();
            Assert.DoesNotThrow(() => SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            _registry = new BuffRegistry();
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);
            Assert.DoesNotThrow(() => SystemRepository.Instance.UnregisterSystem<IBuffRegistry>());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            _registry = new BuffRegistry();
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);

            var result = SystemRepository.Instance.GetSystem<IBuffRegistry>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(_registry));
        }

        [Test]
        public void ActiveBuffs_IsEmpty_Initially()
        {
            _registry = new BuffRegistry();
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);

            Assert.That(_registry.ActiveBuffs.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddBuff_WithNullModel_ReturnsNull()
        {
            _registry = new BuffRegistry();
            SystemRepository.Instance.RegisterSystem<IBuffRegistry>(_registry);

            // Acquire AddBuffInfo directly from ReferencePool with null Model
            // (AddBuffInfo.Create requires a valid BuffModelConfig in Resources)
            var info = ReferencePool.Acquire<AddBuffInfo>();
            Assert.That(info.Model, Is.Null);

            LogAssert.Expect(LogType.Error, "gzp BuffModel不能为null");
            var result = _registry.AddBuff(info);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void UnInit_ClearsActiveBuffs()
        {
            _registry = new BuffRegistry();
            _registry.Init();
            Assert.That(_registry.ActiveBuffs.Count, Is.EqualTo(0));

            _registry.UnInit();

            // After UnInit, ActiveBuffs should remain empty and not throw
            Assert.That(_registry.ActiveBuffs.Count, Is.EqualTo(0));
        }
    }
}
