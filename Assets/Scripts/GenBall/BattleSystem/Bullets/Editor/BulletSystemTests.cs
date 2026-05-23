using GenBall.BattleSystem.Bullets;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets.Tests
{
    /// <summary>
    /// EditMode tests for IBulletSystem / BulletSystem.
    /// FireBullet and RecycleBullet need EntityCreator which is unavailable
    /// in EditMode, so those paths are tested with null-input exception checks.
    /// </summary>
    [TestFixture]
    public class BulletSystemTests
    {
        private IBulletSystem _bulletSystem;

        [SetUp]
        public void SetUp()
        {
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
        public void FireBullet_WithNullInfo_Throws()
        {
            // FireBullet accesses info.Model on null, causing NullReferenceException
            Assert.Throws<System.NullReferenceException>(() => _bulletSystem.FireBullet(null));
        }

        [Test]
        public void RecycleBullet_WithNull_Throws()
        {
            // RecycleBullet accesses bulletState.gameObject on null, causing NullReferenceException
            Assert.Throws<System.NullReferenceException>(() => _bulletSystem.RecycleBullet(null));
        }
    }
}
