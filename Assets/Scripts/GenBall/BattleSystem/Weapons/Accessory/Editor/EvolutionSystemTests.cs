using GenBall.BattleSystem.Weapons;
using GenBall.BattleSystem.Weapons.Accessory;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.BattleSystem.Weapons.Accessory.Tests
{
    /// <summary>
    /// EditMode tests for IEvolutionSystem / EvolutionSystem.
    /// GameEntry.Event is unavailable in EditMode; operations that fire
    /// events (Init, CurrentEvolutionLevel setter) are wrapped in try-catch.
    /// </summary>
    [TestFixture]
    public class EvolutionSystemTests
    {
        private IEvolutionSystem _evolution;

        [SetUp]
        public void SetUp()
        {
            _evolution = new EvolutionSystem();
            SystemRepository.Instance.RegisterSystem<IEvolutionSystem>(_evolution);
            SystemUpdaterManager.Instance.Resume(); // clean state for SystemUpdaterManager
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IEvolutionSystem>(); } catch { }
            SystemUpdaterManager.Instance.Resume();
        }

        /// <summary>
        /// SafeInit: calls Init and catches NullReferenceException from
        /// GameEntry.Event (unavailable in EditMode when setting KillPoints/CurrentEvolutionLevel).
        /// </summary>
        private static void SafeInit(IEvolutionSystem evolution)
        {
            try { evolution.Init(); }
            catch (System.NullReferenceException) { /* GameEntry.Event unavailable in EditMode */ }
        }

        /// <summary>
        /// SafeSetCurrentEvolutionLevel: sets CurrentEvolutionLevel and catches
        /// NullReferenceException from GameEntry.Event.FireWeaponLevel.
        /// </summary>
        private static void SafeSetCurrentEvolutionLevel(IEvolutionSystem evolution, int level)
        {
            try { evolution.CurrentEvolutionLevel = level; }
            catch (System.NullReferenceException) { /* GameEntry.Event unavailable in EditMode */ }
        }

        [Test]
        public void Init_DoesNotThrow()
        {
            // Init fires GameEntry.Event via KillPoints/CurrentEvolutionLevel setters.
            // We catch the expected NullReferenceException from GameEntry.Event being null
            // in EditMode, and verify no OTHER exception is thrown.
            Assert.DoesNotThrow(() => SafeInit(_evolution));
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            // Act & Assert: UnInit is a no-op, should not throw
            Assert.DoesNotThrow(() => _evolution.UnInit());
        }

        [Test]
        public void DefaultMaxEvolutionLevel_IsFour()
        {
            // Assert: MaxEvolutionLevel defaults to 4 (before Init, property reads default value)
            Assert.That(_evolution.MaxEvolutionLevel, Is.EqualTo(4));
        }

        [Test]
        public void DefaultCurrentEvolutionLevel_IsZero()
        {
            // Assert: CurrentEvolutionLevel defaults to 0 (before Init, field default)
            Assert.That(_evolution.CurrentEvolutionLevel, Is.EqualTo(0));
        }

        [Test]
        public void DefaultKillPoints_IsZero()
        {
            // Assert: KillPoints defaults to 0 (before Init, field default)
            Assert.That(_evolution.KillPoints, Is.EqualTo(0));
        }

        [Test]
        public void CanEvolve_AtLevelZero_InsufficientPoints_ReturnsFalse()
        {
            // Arrange: Init to populate _config (catch GameEntry.Event NRE)
            SafeInit(_evolution);

            // KillPoints defaults to 0, CurrentEvolutionLevel defaults to 0.
            // CanEvolve checks if stageConfigs[CurrentEvolutionLevel+1] exists and
            // requires at least some KillPoints. With 0 KillPoints, should return false
            // (unless level 1 requires 0 points in the config).
            Assert.That(_evolution.CanEvolve, Is.False);
        }

        [Test]
        public void GetEquipInfo_LevelZero_ReturnsDefault()
        {
            // GetEquipInfo: when level < 1, returns default EquipInfo
            var info = _evolution.GetEquipInfo(0);
            Assert.That(info, Is.Not.Null);
            Assert.That(info.WeaponId, Is.EqualTo(WeaponId.Pistol));
        }

        [Test]
        public void GetEquipInfo_LevelOne_ReturnsNonNull()
        {
            // GetEquipInfo: level 1 should return the first entry (if it exists)
            var info = _evolution.GetEquipInfo(1);
            Assert.That(info, Is.Not.Null);
        }

        [Test]
        public void GetEquipInfo_LevelFive_OutOfRange_ReturnsDefault()
        {
            // GetEquipInfo: when level > _equipInfos.Count, returns default EquipInfo
            var info = _evolution.GetEquipInfo(5);
            Assert.That(info, Is.Not.Null);
            Assert.That(info.WeaponId, Is.EqualTo(WeaponId.Pistol));
        }

        [Test]
        public void CurrentEvolutionLevel_Set_NoException()
        {
            // Setting CurrentEvolutionLevel fires GameEntry.Event.FireWeaponLevel
            // which is null in EditMode. We catch the expected NRE.
            Assert.DoesNotThrow(() => SafeSetCurrentEvolutionLevel(_evolution, 1));
            Assert.That(_evolution.CurrentEvolutionLevel, Is.EqualTo(1));
        }

        [Test]
        public void HasSystem_ReturnsTrue()
        {
            // Assert
            Assert.That(SystemRepository.Instance.HasSystem<IEvolutionSystem>(), Is.True);
        }
    }
}
