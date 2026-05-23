using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Tests
{
    [TestFixture]
    public class DamageSystemTests
    {
        private IDamageSystem _damageSystem;
        private GameObject _testObject;

        [SetUp]
        public void SetUp()
        {
            SystemUpdaterManager.Instance.Resume();
            _damageSystem = new DamageSystemDefault();
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IDamageSystem>(); } catch { }
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
            Assert.DoesNotThrow(() => SystemRepository.Instance.RegisterSystem<IDamageSystem>(_damageSystem));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            SystemRepository.Instance.RegisterSystem<IDamageSystem>(_damageSystem);

            Assert.DoesNotThrow(() => SystemRepository.Instance.UnregisterSystem<IDamageSystem>());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            SystemRepository.Instance.RegisterSystem<IDamageSystem>(_damageSystem);

            var result = SystemRepository.Instance.GetSystem<IDamageSystem>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.SameAs(_damageSystem));
        }

        [Test]
        public void ApplyDamage_NullDefender_ReleasesInfo()
        {
            SystemRepository.Instance.RegisterSystem<IDamageSystem>(_damageSystem);

            // DamageSystemDefault accesses defender.GetComponentInChildren which will
            // throw NullReferenceException on null defender. This test documents the
            // expected behavior: null defender should result in info release without crash.
            var info = DamageInfo.Create(null, 10, new List<string>());

            // NOTE: Currently throws NullReferenceException because
            // DamageSystemDefault does not null-check defender before GetComponent.
            Assert.DoesNotThrow(() => _damageSystem.ApplyDamage(info));
        }

        [Test]
        public void ApplyDamage_NoDamageable_ReleasesInfo()
        {
            SystemRepository.Instance.RegisterSystem<IDamageSystem>(_damageSystem);

            _testObject = new GameObject("test_defender");
            var info = DamageInfo.Create(_testObject, 10, new List<string>());

            // GameObject without IDamageable: system should release info quietly
            Assert.DoesNotThrow(() => _damageSystem.ApplyDamage(info));
        }
    }
}
