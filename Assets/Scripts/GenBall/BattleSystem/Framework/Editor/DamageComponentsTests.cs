using System.Collections.Generic;
using GenBall.Framework.Entity;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class DamageComponentsTests
    {
        private GameObject _gameObject;
        private BattleEntity _entity;
        private StatComponent _stats;
        private DamageReceiverComponent _receiver;
        private IEntityUpdateSystem _updateSystem;

        [SetUp]
        public void SetUp()
        {
            // Register EntityUpdateSystem for BattleEntity
            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("TestEntity");
            _entity = _gameObject.AddComponent<BattleEntity>();
            _stats = new StatComponent();
            _entity.RegisterComponent(_stats);
            _receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(_receiver);
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();

            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);

            SystemUpdaterManager.Instance.Resume();
        }

        // --- Calculator Tests ---

        [Test]
        public void SimpleDamageCalculator_UsesAttackStat()
        {
            _stats.SetBase("Attack", 25f);
            var calc = new SimpleDamageCalculator();
            var ctx = new DamageContext { AttackerStats = _stats };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(25));
        }

        [Test]
        public void SimpleDamageCalculator_NullStats_ReturnsZero()
        {
            var calc = new SimpleDamageCalculator();
            var ctx = new DamageContext { AttackerStats = null };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(0));
        }

        [Test]
        public void WeaponDamageCalculator_AddsAttackAndWeapon()
        {
            _stats.SetBase("Attack", 10f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 15f);

            var calc = new WeaponDamageCalculator();
            var ctx = new DamageContext { AttackerStats = _stats, WeaponStats = weaponStats };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(25));
        }

        [Test]
        public void WeaponDamageCalculator_NullWeaponStats_UsesOnlyAttack()
        {
            _stats.SetBase("Attack", 10f);
            var calc = new WeaponDamageCalculator();
            var ctx = new DamageContext { AttackerStats = _stats, WeaponStats = null };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(10));
        }

        [Test]
        public void BulletDamageCalculator_AddsAllThreeSources()
        {
            _stats.SetBase("Attack", 10f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 7f);
            var bulletStats = new StatComponent();
            bulletStats.SetBase("Damage", 5f);

            var calc = new BulletDamageCalculator();
            var ctx = new DamageContext { AttackerStats = _stats, WeaponStats = weaponStats, BulletStats = bulletStats };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(22)); // 10 + 7 + 5
        }

        [Test]
        public void BulletDamageCalculator_NullBulletStats_UsesAttackAndWeapon()
        {
            _stats.SetBase("Attack", 10f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 7f);

            var calc = new BulletDamageCalculator();
            var ctx = new DamageContext { AttackerStats = _stats, WeaponStats = weaponStats, BulletStats = null };
            Assert.That(calc.Calculate(ctx), Is.EqualTo(17));
        }

        // --- DamageReceiverComponent Tests ---
        // Note: These tests use only the receiver's internal health tracking since
        // actual damage application requires IDamageSystem/IDeathSystem to be registered.

        [Test]
        public void DamageReceiverComponent_InitialHealth_EqualsMaxHealth()
        {
            _stats.SetBase("MaxHealth", 100f);
            var receiver = new DamageReceiverComponent(_entity);
            Assert.That(receiver.Health, Is.EqualTo(100));
        }

        [Test]
        public void DamageReceiverComponent_MaxHealth_ReadsFromStats()
        {
            _stats.SetBase("MaxHealth", 200f);
            var receiver = new DamageReceiverComponent(_entity);
            Assert.That(receiver.MaxHealth, Is.EqualTo(200));
        }

        [Test]
        public void DamageReceiverComponent_IsDead_InitiallyFalse()
        {
            Assert.That(_receiver.IsDead, Is.False);
        }

        [Test]
        public void DamageReceiverComponent_Die_SetsIsDead()
        {
            var deathInfo = DeathInfo.Create(_gameObject, new List<string> { DeathTag.HealthEmpty });
            _receiver.Die(deathInfo);
            Assert.That(_receiver.IsDead, Is.True);
        }

        [Test]
        public void DamageReceiverComponent_MaxHealth_ZeroWhenNoStats()
        {
            var entityNoStats = new GameObject("NoStatsEntity").AddComponent<BattleEntity>();
            var receiver = new DamageReceiverComponent(entityNoStats);
            Assert.That(receiver.MaxHealth, Is.EqualTo(0));
            Object.DestroyImmediate(entityNoStats.gameObject);
        }

        // --- AttackComponent Tests ---

        [Test]
        public void AttackComponent_UsesConfiguredCalculator()
        {
            _stats.SetBase("Attack", 30f);
            var calc = new SimpleDamageCalculator();
            var attackComp = new AttackComponent(_entity, calc);

            var damage = attackComp.CalculateDamage();
            Assert.That(damage, Is.EqualTo(30));
        }

        [Test]
        public void AttackComponent_CanSwapCalculator()
        {
            _stats.SetBase("Attack", 10f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 20f);

            var calc = new WeaponDamageCalculator();
            var attackComp = new AttackComponent(_entity, calc);
            var damage = attackComp.CalculateDamage(weaponStats);
            Assert.That(damage, Is.EqualTo(30));
        }
    }
}
