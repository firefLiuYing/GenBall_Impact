using System.Collections.Generic;
using GenBall.Framework.Entity;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class BattleEntityIntegrationTests
    {
        private GameObject _gameObject;
        private BattleEntity _entity;
        private StatComponent _stats;
        private BuffContainerComponent _buffs;
        private DamageReceiverComponent _receiver;
        private AttackComponent _attacker;
        private IEntityUpdateSystem _updateSystem;

        [SetUp]
        public void SetUp()
        {
            // Register EntityUpdateSystem (required by BattleEntity.RegisterComponent)
            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("IntegrationTestEntity");
            _entity = _gameObject.AddComponent<BattleEntity>();

            // Register all components for a typical "player" entity
            _stats = new StatComponent();
            _entity.RegisterComponent(_stats);

            _buffs = new BuffContainerComponent();
            _entity.RegisterComponent(_buffs);

            _receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(_receiver);

            _attacker = new AttackComponent(_entity, new SimpleDamageCalculator());
            _entity.RegisterComponent(_attacker);
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

        // --- Entity Type Factory Tests ---

        [Test]
        public void PlayerEntity_HasAllComponents()
        {
            _stats.SetBase("MaxHealth", 100f);
            _stats.SetBase("Attack", 15f);

            Assert.That(_entity.Has<StatComponent>(), Is.True);
            Assert.That(_entity.Has<BuffContainerComponent>(), Is.True);
            Assert.That(_entity.Has<DamageReceiverComponent>(), Is.True);
            Assert.That(_entity.Has<AttackComponent>(), Is.True);
            // MaxHealth reads from StatComponent dynamically; Health is set only at construction time
            Assert.That(_receiver.MaxHealth, Is.EqualTo(100));
        }

        [Test]
        public void TrapEntity_OnlyHasAttack()
        {
            var trapObj = new GameObject("Trap");
            var trapEntity = trapObj.AddComponent<BattleEntity>();
            var trapAttacker = new AttackComponent(trapEntity, new SimpleDamageCalculator());
            trapEntity.RegisterComponent(trapAttacker);

            Assert.That(trapEntity.Has<AttackComponent>(), Is.True);
            Assert.That(trapEntity.Has<StatComponent>(), Is.False);
            Assert.That(trapEntity.Has<DamageReceiverComponent>(), Is.False);
            Assert.That(trapEntity.Has<BuffContainerComponent>(), Is.False);

            Object.DestroyImmediate(trapObj);
        }

        [Test]
        public void BarrelEntity_HasStatsAndReceiver_NoAttack()
        {
            var barrelObj = new GameObject("Barrel");
            var barrelEntity = barrelObj.AddComponent<BattleEntity>();

            var barrelStats = new StatComponent();
            barrelStats.SetBase("MaxHealth", 30f);
            barrelEntity.RegisterComponent(barrelStats);

            var barrelReceiver = new DamageReceiverComponent(barrelEntity);
            barrelEntity.RegisterComponent(barrelReceiver);

            Assert.That(barrelEntity.Has<StatComponent>(), Is.True);
            Assert.That(barrelEntity.Has<DamageReceiverComponent>(), Is.True);
            Assert.That(barrelEntity.Has<AttackComponent>(), Is.False);
            Assert.That(barrelEntity.Has<BuffContainerComponent>(), Is.False);
            Assert.That(barrelReceiver.MaxHealth, Is.EqualTo(30));

            Object.DestroyImmediate(barrelObj);
        }

        // --- Component Interaction Tests ---

        [Test]
        public void DamageReceiver_ReadsMaxHealthFromStats()
        {
            _stats.SetBase("MaxHealth", 200f);
            var receiver = new DamageReceiverComponent(_entity);

            Assert.That(receiver.MaxHealth, Is.EqualTo(200));
            Assert.That(receiver.Health, Is.EqualTo(200));
        }

        [Test]
        public void DamageReceiver_MaxHealthChanges_WhenStatsChange()
        {
            _stats.SetBase("MaxHealth", 100f);
            Assert.That(_receiver.MaxHealth, Is.EqualTo(100));

            // Add a buff that increases max health via FlatAdd modifier
            _stats.AddModifier("MaxHealth", new StatModifier(ModifierType.FlatAdd, 50f));
            Assert.That(_receiver.MaxHealth, Is.EqualTo(150));
        }

        [Test]
        public void StatModifier_AffectsAttackCalculation()
        {
            _stats.SetBase("Attack", 20f);
            var calc = new SimpleDamageCalculator();
            var attacker = new AttackComponent(_entity, calc);

            int baseDamage = attacker.CalculateDamage();
            Assert.That(baseDamage, Is.EqualTo(20));

            // Apply a 50% attack buff via PercentAdd modifier
            _stats.AddModifier("Attack", new StatModifier(ModifierType.PercentAdd, 0.5f));
            int buffedDamage = attacker.CalculateDamage();
            Assert.That(buffedDamage, Is.EqualTo(30)); // 20 * (1 + 0.5) = 30
        }

        [Test]
        public void AllComponents_Accessible_ThroughEntity()
        {
            Assert.That(_entity.Get<StatComponent>(), Is.SameAs(_stats));
            Assert.That(_entity.Get<BuffContainerComponent>(), Is.SameAs(_buffs));
            Assert.That(_entity.Get<DamageReceiverComponent>(), Is.SameAs(_receiver));
            Assert.That(_entity.Get<AttackComponent>(), Is.SameAs(_attacker));
        }

        // --- Damage Pipeline Test (within entity boundary) ---

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            _stats.SetBase("MaxHealth", 100f);
            var receiver = new DamageReceiverComponent(_entity);

            var damageInfo = DamageInfo.Create(_gameObject, 30, null);
            int healthBefore = receiver.Health;

            receiver.TakeDamage(damageInfo);

            Assert.That(receiver.Health, Is.EqualTo(healthBefore - 30));
        }

        [Test]
        public void TakeDamage_DeadEntity_NoFurtherDamage()
        {
            _stats.SetBase("MaxHealth", 100f);
            var receiver = new DamageReceiverComponent(_entity);
            receiver.Die(DeathInfo.Create(_gameObject, new List<string> { DeathTag.HealthEmpty }));

            var damageInfo = DamageInfo.Create(_gameObject, 50, null);
            receiver.TakeDamage(damageInfo);

            Assert.That(receiver.IsDead, Is.True);
            Assert.That(receiver.Health, Is.EqualTo(100)); // Unchanged because entity was already dead
        }

        [Test]
        public void WeaponDamageCalculator_WithEntityStats()
        {
            _stats.SetBase("Attack", 15f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 25f);

            var calc = new WeaponDamageCalculator();
            var attacker = new AttackComponent(_entity, calc);
            int damage = attacker.CalculateDamage(weaponStats);

            Assert.That(damage, Is.EqualTo(40)); // 15 attack + 25 weapon damage
        }

        [Test]
        public void BulletDamageCalculator_WithFullContext()
        {
            _stats.SetBase("Attack", 12f);
            var weaponStats = new StatComponent();
            weaponStats.SetBase("Damage", 18f);
            var bulletStats = new StatComponent();
            bulletStats.SetBase("Damage", 8f);

            var calc = new BulletDamageCalculator();
            var attacker = new AttackComponent(_entity, calc);
            int damage = attacker.CalculateDamage(weaponStats, bulletStats);

            Assert.That(damage, Is.EqualTo(38)); // 12 attack + 18 weapon + 8 bullet
        }

        // --- Buff Container Integration ---

        [Test]
        public void BuffContainer_WorksWithStatComponent()
        {
            _stats.SetBase("Defense", 10f);
            _stats.AddModifier("Defense", new StatModifier(ModifierType.FlatAdd, 5f));

            Assert.That(_buffs.Buffs.Count, Is.EqualTo(0));
            Assert.That(_stats.GetValue("Defense"), Is.EqualTo(15f).Within(0.0001f));
        }

        // --- Entity Lifecycle Integration ---

        [Test]
        public void RegisteredComponents_SurviveEntityAccess()
        {
            // Components should be accessible via TryGet as well
            Assert.That(_entity.TryGet<StatComponent>(out var s), Is.True);
            Assert.That(s, Is.SameAs(_stats));

            Assert.That(_entity.TryGet<BuffContainerComponent>(out var b), Is.True);
            Assert.That(b, Is.SameAs(_buffs));

            Assert.That(_entity.TryGet<DamageReceiverComponent>(out var r), Is.True);
            Assert.That(r, Is.SameAs(_receiver));
        }

        [Test]
        public void DamageReceiver_InitialHealth_MatchesMaxHealth()
        {
            _stats.SetBase("MaxHealth", 75f);
            var receiver = new DamageReceiverComponent(_entity);

            Assert.That(receiver.MaxHealth, Is.EqualTo(75));
            Assert.That(receiver.Health, Is.EqualTo(75));
        }

        [Test]
        public void TakeDamage_BelowZero_ClampsToZero()
        {
            // Register a mock IDeathSystem to avoid log errors
            var mockDeath = new MockDeathSystem();
            if (!SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.RegisterSystem<IDeathSystem>(mockDeath);

            _stats.SetBase("MaxHealth", 10f);
            var receiver = new DamageReceiverComponent(_entity);

            var damageInfo = DamageInfo.Create(_gameObject, 50, null);
            receiver.TakeDamage(damageInfo);

            // DamageReceiverComponent clamps health to 0 when depleted
            Assert.That(receiver.Health, Is.EqualTo(0));
            // Death was applied via IDeathSystem
            Assert.That(mockDeath.LastDeathInfo, Is.Not.Null);

            if (SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.UnregisterSystem<IDeathSystem>();
        }

        [Test]
        public void DamageReceiver_TakeDamage_ThenDie_HealthPreserved()
        {
            _stats.SetBase("MaxHealth", 80f);
            var receiver = new DamageReceiverComponent(_entity);

            var damageInfo = DamageInfo.Create(_gameObject, 30, null);
            receiver.TakeDamage(damageInfo);
            Assert.That(receiver.Health, Is.EqualTo(50));
            Assert.That(receiver.IsDead, Is.False);

            receiver.Die(DeathInfo.Create(_gameObject, new List<string> { DeathTag.HealthEmpty }));
            Assert.That(receiver.IsDead, Is.True);
            Assert.That(receiver.Health, Is.EqualTo(50)); // Health preserved after death
        }

        [Test]
        public void StatModifier_ModifierTypeMultiply_AffectsFinalValue()
        {
            _stats.SetBase("Power", 10f);
            _stats.AddModifier("Power", new StatModifier(ModifierType.Multiply, 2f));

            Assert.That(_stats.GetValue("Power"), Is.EqualTo(20f).Within(0.0001f));
        }

        [Test]
        public void StatModifier_CombinedModifierTypes_CalculatesCorrectly()
        {
            // Formula: (Base + flatSum) * (1 + percentSum) * multiplyProduct
            _stats.SetBase("Power", 10f);
            _stats.AddModifier("Power", new StatModifier(ModifierType.FlatAdd, 5f));       // base + 5 = 15
            _stats.AddModifier("Power", new StatModifier(ModifierType.PercentAdd, 0.2f));   // 15 * 1.2 = 18
            _stats.AddModifier("Power", new StatModifier(ModifierType.Multiply, 1.5f));     // 18 * 1.5 = 27

            Assert.That(_stats.GetValue("Power"), Is.EqualTo(27f).Within(0.0001f));
        }

        [Test]
        public void AttackComponent_NoStats_ReturnsZero()
        {
            // Entity with no StatComponent at all
            var emptyObj = new GameObject("EmptyEntity");
            var emptyEntity = emptyObj.AddComponent<BattleEntity>();
            var calc = new SimpleDamageCalculator();
            var attacker = new AttackComponent(emptyEntity, calc);

            int damage = attacker.CalculateDamage();

            Assert.That(damage, Is.EqualTo(0));

            Object.DestroyImmediate(emptyObj);
        }

        /// <summary>Minimal IDeathSystem mock for testing death flow.</summary>
        private class MockDeathSystem : IDeathSystem
        {
            public DeathInfo LastDeathInfo;
            public void Init() { }
            public void UnInit() { }
            public void ApplyDeath(DeathInfo deathInfo) => LastDeathInfo = deathInfo;
        }
    }
}
