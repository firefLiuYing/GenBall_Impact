using System.Collections.Generic;
using GenBall.Framework.Entity;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class EventDispatcherComponentTests
    {
        private GameObject _gameObject;
        private BattleEntity _entity;
        private IEntityUpdateSystem _updateSystem;

        [SetUp]
        public void SetUp()
        {
            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("EventDispatcherTestEntity");
            _entity = _gameObject.AddComponent<BattleEntity>();
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            if (SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.UnregisterSystem<IDeathSystem>();

            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);

            SystemUpdaterManager.Instance.Resume();
        }

        #region EventDispatcherComponent Basic Tests

        [Test]
        public void Subscribe_FireNow_HandlerCalled()
        {
            var ed = new EventDispatcherComponent(_entity);
            var received = 0;

            ed.Subscribe((int)EntityEventId.StatChanged, () => received++);
            ed.FireNow((int)EntityEventId.StatChanged);

            Assert.That(received, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_WithData_HandlerReceivesData()
        {
            var ed = new EventDispatcherComponent(_entity);
            StatChangedEventData received = default;

            ed.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged, data => received = data);
            ed.FireNow((int)EntityEventId.StatChanged,
                new StatChangedEventData { StatName = "Attack", OldValue = 10f, NewValue = 20f });

            Assert.That(received.StatName, Is.EqualTo("Attack"));
            Assert.That(received.OldValue, Is.EqualTo(10f));
            Assert.That(received.NewValue, Is.EqualTo(20f));
        }

        [Test]
        public void Unsubscribe_HandlerNotCalled()
        {
            var ed = new EventDispatcherComponent(_entity);
            var received = 0;
            System.Action handler = () => received++;

            ed.Subscribe((int)EntityEventId.StatChanged, handler);
            ed.Unsubscribe((int)EntityEventId.StatChanged, handler);
            ed.FireNow((int)EntityEventId.StatChanged);

            Assert.That(received, Is.EqualTo(0));
        }

        [Test]
        public void MultipleSubscribers_AllCalled()
        {
            var ed = new EventDispatcherComponent(_entity);
            var callCount = 0;

            ed.Subscribe((int)EntityEventId.StatChanged, () => callCount++);
            ed.Subscribe((int)EntityEventId.StatChanged, () => callCount++);
            ed.FireNow((int)EntityEventId.StatChanged);

            Assert.That(callCount, Is.EqualTo(2));
        }

        #endregion

        #region StatComponent Event Tests

        [Test]
        public void StatComponent_SetBase_FiresStatChanged()
        {
            var stats = new StatComponent(_entity);
            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);
            _entity.RegisterComponent(stats);

            var receivedName = "";
            var receivedOld = 0f;
            var receivedNew = 0f;

            ed.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged, data =>
            {
                receivedName = data.StatName;
                receivedOld = data.OldValue;
                receivedNew = data.NewValue;
            });

            stats.SetBase("MaxHealth", 100f);

            Assert.That(receivedName, Is.EqualTo("MaxHealth"));
            Assert.That(receivedOld, Is.EqualTo(0f));
            Assert.That(receivedNew, Is.EqualTo(100f));
        }

        [Test]
        public void StatComponent_AddModifier_FiresStatChanged()
        {
            var stats = new StatComponent(_entity);
            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);
            _entity.RegisterComponent(stats);

            stats.SetBase("Attack", 10f);

            var received = false;
            ed.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged, data =>
            {
                received = true;
                Assert.That(data.StatName, Is.EqualTo("Attack"));
                Assert.That(data.OldValue, Is.EqualTo(10f));
                Assert.That(data.NewValue, Is.EqualTo(15f)); // 10 + 5 flat
            });

            stats.AddModifier("Attack", new StatModifier(ModifierType.FlatAdd, 5f));

            Assert.That(received, Is.True);
        }

        [Test]
        public void StatComponent_RemoveModifier_FiresStatChanged()
        {
            var stats = new StatComponent(_entity);
            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);
            _entity.RegisterComponent(stats);

            var mod = new StatModifier(ModifierType.FlatAdd, 5f);
            stats.SetBase("Attack", 10f);
            stats.AddModifier("Attack", mod);

            var received = false;
            ed.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged, data =>
            {
                received = true;
                Assert.That(data.OldValue, Is.EqualTo(15f)); // 10 + 5
                Assert.That(data.NewValue, Is.EqualTo(10f)); // removed
            });

            stats.RemoveModifier("Attack", mod);

            Assert.That(received, Is.True);
        }

        [Test]
        public void StatComponent_NoEventDispatcher_WorksSilently()
        {
            var stats = new StatComponent(_entity);
            _entity.RegisterComponent(stats);

            // Should not throw
            var mod = new StatModifier(ModifierType.FlatAdd, 50f);
            stats.SetBase("MaxHealth", 100f);
            stats.AddModifier("MaxHealth", mod);
            stats.RemoveModifier("MaxHealth", mod);

            Assert.That(stats.GetValue("MaxHealth"), Is.EqualTo(100f));
        }

        #endregion

        #region DamageReceiver Event Tests

        [Test]
        public void TakeDamage_FiresHealthChanged()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var received = false;
            ed.Subscribe<HealthChangedEventData>((int)EntityEventId.HealthChanged, data =>
            {
                received = true;
                Assert.That(data.OldHealth, Is.EqualTo(100f));
                Assert.That(data.NewHealth, Is.EqualTo(70f));
                Assert.That(data.MaxHealth, Is.EqualTo(100f));
            });

            var damageInfo = DamageInfo.Create(_gameObject, 30, null);
            receiver.TakeDamage(damageInfo);

            Assert.That(received, Is.True);
            Assert.That(receiver.Health, Is.EqualTo(70));
        }

        [Test]
        public void TakeDamage_ShieldAbsorbsFirst()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("Shield", 50f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var damageInfo = DamageInfo.Create(_gameObject, 30, null);
            receiver.TakeDamage(damageInfo);

            // Shield absorbed 30, health unchanged
            Assert.That(stats.GetValue("Shield"), Is.EqualTo(20f));
            Assert.That(receiver.Health, Is.EqualTo(100));
        }

        [Test]
        public void TakeDamage_ShieldPartial_DamageSpillsToHealth()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("Shield", 20f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var damageInfo = DamageInfo.Create(_gameObject, 50, null);
            receiver.TakeDamage(damageInfo);

            // Shield absorbed 20, remaining 30 goes to health
            Assert.That(stats.GetValue("Shield"), Is.EqualTo(0f));
            Assert.That(receiver.Health, Is.EqualTo(70));
        }

        [Test]
        public void Heal_RestoresHealth_FiresHealthChanged()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            // First deal damage
            var damageInfo = DamageInfo.Create(_gameObject, 40, null);
            receiver.TakeDamage(damageInfo);
            Assert.That(receiver.Health, Is.EqualTo(60));

            // Then heal
            var received = false;
            ed.Subscribe<HealthChangedEventData>((int)EntityEventId.HealthChanged, data =>
            {
                received = true;
                Assert.That(data.OldHealth, Is.EqualTo(60f));
                Assert.That(data.NewHealth, Is.EqualTo(90f));
            });

            receiver.Heal(30);
            Assert.That(receiver.Health, Is.EqualTo(90));
            Assert.That(received, Is.True);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHealth()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("CurrentHealth", 90f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            receiver.Heal(50);
            Assert.That(receiver.Health, Is.EqualTo(100));
        }

        [Test]
        public void Heal_DeadEntity_NoEffect()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var receiver = new DamageReceiverComponent(_entity);
            receiver.Die(DeathInfo.Create(_gameObject, new List<string> { DeathTag.HealthEmpty }));
            receiver.Heal(50);

            Assert.That(receiver.Health, Is.EqualTo(100)); // unchanged
        }

        #endregion

        #region DeathComponent Tests

        [Test]
        public void DeathComponent_TriggersOnHealthZero()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var mockDeath = new DeathMockDeathSystem();
            if (!SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.RegisterSystem<IDeathSystem>(mockDeath);

            var deathHandler = new DeathMockDeathHandler();
            var deathComponent = new DeathComponent(_entity, deathHandler);
            _entity.RegisterComponent(deathComponent);

            var damageInfo = DamageInfo.Create(_gameObject, 100, null);
            receiver.TakeDamage(damageInfo);

            Assert.That(receiver.Health, Is.EqualTo(0));
            Assert.That(mockDeath.LastDeathInfo, Is.Not.Null);
            Assert.That(deathHandler.WasCalled, Is.True);
        }

        [Test]
        public void DeathComponent_DoesNotFireTwice()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var mockDeath = new DeathMockDeathSystem();
            if (!SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.RegisterSystem<IDeathSystem>(mockDeath);

            var deathHandler = new DeathMockDeathHandler();
            var deathComponent = new DeathComponent(_entity, deathHandler);
            _entity.RegisterComponent(deathComponent);

            // Kill the entity
            receiver.TakeDamage(DamageInfo.Create(_gameObject, 100, null));
            Assert.That(deathHandler.CallCount, Is.EqualTo(1));

            // Try to deal more damage (should be blocked by IsDead check in receiver)
            receiver.TakeDamage(DamageInfo.Create(_gameObject, 50, null));
            Assert.That(deathHandler.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void DeathComponent_NoEventDispatcher_NoError()
        {
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            _entity.RegisterComponent(stats);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            // DeathComponent with no EventDispatcherComponent should not throw
            var deathHandler = new DeathMockDeathHandler();
            var deathComponent = new DeathComponent(_entity, deathHandler);
            _entity.RegisterComponent(deathComponent);

            // Damage goes through (no events, so DeathComponent won't be notified)
            var damageInfo = DamageInfo.Create(_gameObject, 100, null);
            Assert.DoesNotThrow(() => receiver.TakeDamage(damageInfo));

            Assert.That(deathHandler.CallCount, Is.EqualTo(0)); // No event dispatch
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullDamagePipeline_StatToDeath()
        {
            // Assembles all components end-to-end
            var stats = new StatComponent(_entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("CurrentHealth", 100f);
            stats.GetOrCreate("Shield", 50f);
            _entity.RegisterComponent(stats);

            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);

            var receiver = new DamageReceiverComponent(_entity);
            _entity.RegisterComponent(receiver);

            var mockDeath = new DeathMockDeathSystem();
            if (!SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.RegisterSystem<IDeathSystem>(mockDeath);

            var deathHandler = new DeathMockDeathHandler();
            var deathComponent = new DeathComponent(_entity, deathHandler);
            _entity.RegisterComponent(deathComponent);

            // Deal 150 damage: shield absorbs 50, health takes 100 → death
            var damageInfo = DamageInfo.Create(_gameObject, 150, null);
            receiver.TakeDamage(damageInfo);

            Assert.That(stats.GetValue("Shield"), Is.EqualTo(0f));
            Assert.That(receiver.Health, Is.EqualTo(0));
            Assert.That(mockDeath.LastDeathInfo, Is.Not.Null);
            Assert.That(deathHandler.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void StatModifier_ChangesMaxHealth_EventFired()
        {
            var stats = new StatComponent(_entity);
            var ed = new EventDispatcherComponent(_entity);
            _entity.RegisterComponent(ed);
            _entity.RegisterComponent(stats);

            stats.SetBase("MaxHealth", 100f);

            var eventList = new List<StatChangedEventData>();
            ed.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged, data => eventList.Add(data));

            // Add a flat modifier — should fire StatChanged
            stats.AddModifier("MaxHealth", new StatModifier(ModifierType.FlatAdd, 50f));

            Assert.That(eventList.Count, Is.GreaterThan(0));
            var lastEvent = eventList[eventList.Count - 1];
            Assert.That(lastEvent.StatName, Is.EqualTo("MaxHealth"));
            Assert.That(lastEvent.NewValue, Is.EqualTo(150f));
        }

        #endregion

        #region Mock Helpers

        private class DeathMockDeathSystem : IDeathSystem
        {
            public DeathInfo LastDeathInfo;
            public void Init() { }
            public void UnInit() { }
            public void ApplyDeath(DeathInfo deathInfo) => LastDeathInfo = deathInfo;
        }

        private class DeathMockDeathHandler : IDeathHandler
        {
            public bool WasCalled => CallCount > 0;
            public int CallCount;
            public void OnDeath(DeathInfo deathInfo) => CallCount++;
        }

        #endregion
    }
}
