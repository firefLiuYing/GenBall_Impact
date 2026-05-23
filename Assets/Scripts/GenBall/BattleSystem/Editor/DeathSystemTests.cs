using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Main;

namespace GenBall.BattleSystem.Tests
{
    [TestFixture]
    public class DeathSystemTests
    {
        private IDeathSystem _deathSystem;
        private GameObject _testObject;

        [SetUp]
        public void SetUp()
        {
            SystemUpdaterManager.Instance.Resume();
            _deathSystem = new DeathSystemDefault();
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IDeathSystem>(); } catch { }
            SystemUpdaterManager.Instance.Resume();

            if (_testObject != null)
            {
                Object.DestroyImmediate(_testObject);
                _testObject = null;
            }
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SystemRepository.Instance.RegisterSystem<IDeathSystem>(_deathSystem));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IDeathSystem>(_deathSystem);

            Assert.DoesNotThrow(() => SystemRepository.Instance.UnregisterSystem<IDeathSystem>());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            SystemRepository.Instance.RegisterSystem<IDeathSystem>(_deathSystem);

            var result = SystemRepository.Instance.GetSystem<IDeathSystem>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(_deathSystem));
        }

        [Test]
        public void ApplyDeath_NullVictim_NoException()
        {
            SystemRepository.Instance.RegisterSystem<IDeathSystem>(_deathSystem);

            // DeathSystemDefault accesses victim.GetComponentInChildren which will
            // throw NullReferenceException on null victim. This test documents the
            // expected behavior: null victim should not crash.
            var info = DeathInfo.Create(null, new List<string>());

            // NOTE: Currently throws NullReferenceException because
            // DeathSystemDefault does not null-check victim before GetComponent.
            Assert.DoesNotThrow(() => _deathSystem.ApplyDeath(info));
        }

        [Test]
        public void ApplyDeath_NoIHealth_ReleasesInfo()
        {
            SystemRepository.Instance.RegisterSystem<IDeathSystem>(_deathSystem);

            _testObject = new GameObject("test_victim");
            var info = DeathInfo.Create(_testObject, new List<string>());

            // GameObject without IHealth: system should log error and release info
            LogAssert.Expect(LogType.Error, "gzp DeathSystem: No IHealth found on test_victim");
            Assert.DoesNotThrow(() => _deathSystem.ApplyDeath(info));
        }
    }
}
