using GenBall.BattleSystem.Bullets;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets.Tests
{
    /// <summary>
    /// EditMode tests for IBulletSystem / BulletSystem.
    /// FireBullet/RecycleBullet are tested with null-input exception checks.
    /// RecycleBullet uses Object.Destroy; FireBullet accesses info.Model on null.
    /// </summary>
    [TestFixture]
    public class BulletSystemTests
    {
        private IBulletSystem _bulletSystem;

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state — SystemRepository is a singleton shared across test fixtures
            if (SystemRepository.Instance.HasSystem<IBulletSystem>())
                SystemRepository.Instance.UnregisterSystem<IBulletSystem>();

            _bulletSystem = new BulletSystem();
            SystemRepository.Instance.RegisterSystem<IBulletSystem>(_bulletSystem);
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IBulletSystem>(); } catch { }
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            // Act & Assert: BulletSystem.Init() is a no-op, should not throw
            Assert.DoesNotThrow(() => _bulletSystem.Init());
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            // Act & Assert: BulletSystem.UnInit() is a no-op, should not throw
            Assert.DoesNotThrow(() => _bulletSystem.UnInit());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            // Assert: system is retrievable via SystemRepository
            Assert.That(SystemRepository.Instance.GetSystem<IBulletSystem>(), Is.Not.Null);
        }

        [Test]
        public void HasSystem_ReturnsTrue()
        {
            // Assert
            Assert.That(SystemRepository.Instance.HasSystem<IBulletSystem>(), Is.True);
        }

        [Test]
        public void FireBullet_WithNullInfo_DoesNotThrow()
        {
            // FireBullet(BulletLaunchInfo) now has a null guard: if (info == null) return;
            Assert.DoesNotThrow(() => _bulletSystem.FireBullet(null));
        }

        [Test]
        public void RecycleBullet_WithNull_DoesNotThrow()
        {
            // RecycleBullet(BulletState) now has a null guard: if (bulletState != null) ...
            Assert.DoesNotThrow(() => _bulletSystem.RecycleBullet(null));
        }
    }
}
