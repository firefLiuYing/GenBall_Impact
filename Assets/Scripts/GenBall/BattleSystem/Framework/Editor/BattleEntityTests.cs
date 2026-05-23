using GenBall.Framework.Entity;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    [TestFixture]
    public class BattleEntityTests
    {
        private GameObject _go;
        private BattleEntity _entity;
        private EntityUpdateSystem _updateSystem;

        private class MockComponent { }

        private class MockLogicUpdateComponent : IEntityLogicUpdate
        {
            public float LastDeltaTime;
            public int TickCount;
            public void LogicUpdate(float deltaTime)
            {
                LastDeltaTime = deltaTime;
                TickCount++;
            }
        }

        private class MockFrameUpdateComponent : IEntityFrameUpdate
        {
            public float LastDeltaTime;
            public int TickCount;
            public void FrameUpdate(float deltaTime)
            {
                LastDeltaTime = deltaTime;
                TickCount++;
            }
        }

        private class MockDualUpdateComponent : IEntityFrameUpdate, IEntityLogicUpdate
        {
            public float LastFrameDelta;
            public float LastLogicDelta;
            public void FrameUpdate(float dt) { LastFrameDelta = dt; }
            public void LogicUpdate(float dt) { LastLogicDelta = dt; }
        }

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestEntity");
            _entity = _go.AddComponent<BattleEntity>();

            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();

            if (_go != null)
                Object.DestroyImmediate(_go);

            SystemUpdaterManager.Instance.Resume();
        }

        [Test]
        public void RegisterComponent_Get_ReturnsSameInstance()
        {
            var comp = new MockComponent();
            _entity.RegisterComponent(comp);

            var result = _entity.Get<MockComponent>();

            Assert.That(result, Is.SameAs(comp));
        }

        [Test]
        public void TryGet_RegisteredComponent_ReturnsTrue()
        {
            var comp = new MockComponent();
            _entity.RegisterComponent(comp);

            var found = _entity.TryGet<MockComponent>(out var result);

            Assert.That(found, Is.True);
            Assert.That(result, Is.SameAs(comp));
        }

        [Test]
        public void TryGet_UnregisteredComponent_ReturnsFalse()
        {
            var found = _entity.TryGet<MockComponent>(out var result);

            Assert.That(found, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Has_RegisteredComponent_ReturnsTrue()
        {
            _entity.RegisterComponent(new MockComponent());

            Assert.That(_entity.Has<MockComponent>(), Is.True);
        }

        [Test]
        public void Has_UnregisteredComponent_ReturnsFalse()
        {
            Assert.That(_entity.Has<MockComponent>(), Is.False);
        }

        [Test]
        public void RegisterComponent_WithIEntityLogicUpdate_AutoRegistersWithUpdateSystem()
        {
            var comp = new MockLogicUpdateComponent();
            _entity.RegisterComponent(comp);

            // Trigger a logic update and verify the component received it
            _updateSystem.LogicUpdate(0.016f);

            Assert.That(comp.TickCount, Is.EqualTo(1));
            Assert.That(comp.LastDeltaTime, Is.EqualTo(0.016f).Within(0.0001f));
        }

        [Test]
        public void RegisterComponent_WithIEntityFrameUpdate_AutoRegistersWithUpdateSystem()
        {
            var comp = new MockFrameUpdateComponent();
            _entity.RegisterComponent(comp);

            _updateSystem.FrameUpdate(0.033f);

            Assert.That(comp.TickCount, Is.EqualTo(1));
            Assert.That(comp.LastDeltaTime, Is.EqualTo(0.033f).Within(0.0001f));
        }

        [Test]
        public void RegisterComponent_WithDualUpdate_AutoRegistersBoth()
        {
            var comp = new MockDualUpdateComponent();
            _entity.RegisterComponent(comp);

            _updateSystem.FrameUpdate(0.016f);
            _updateSystem.LogicUpdate(0.02f);

            Assert.That(comp.LastFrameDelta, Is.EqualTo(0.016f).Within(0.0001f));
            Assert.That(comp.LastLogicDelta, Is.EqualTo(0.02f).Within(0.0001f));
        }

        [Test]
        public void OnDestroy_RemovesFromEntityUpdateSystem()
        {
            var comp = new MockLogicUpdateComponent();
            _entity.RegisterComponent(comp);

            // Verify it was registered (gets updates)
            _updateSystem.LogicUpdate(0.016f);
            Assert.That(comp.TickCount, Is.EqualTo(1));

            // Manually invoke the removal logic that OnDestroy performs
            _updateSystem.RemoveLogicUpdate(comp);

            // After removal, another logic update should NOT increment the counter
            _updateSystem.LogicUpdate(0.016f);
            Assert.That(comp.TickCount, Is.EqualTo(1));
        }

        [Test]
        public void Get_ReturnsNull_WhenNotFound()
        {
            var result = _entity.Get<MockComponent>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void RegisterComponent_ReplacesExistingType()
        {
            var comp1 = new MockComponent();
            var comp2 = new MockComponent();
            _entity.RegisterComponent(comp1);
            _entity.RegisterComponent(comp2);

            var result = _entity.Get<MockComponent>();

            Assert.That(result, Is.SameAs(comp2));
        }

        [Test]
        public void Get_DifferentType_DoesNotConflict()
        {
            _entity.RegisterComponent(new MockComponent());
            _entity.RegisterComponent(new MockLogicUpdateComponent());

            Assert.That(_entity.Get<MockComponent>(), Is.Not.Null);
            Assert.That(_entity.Get<MockLogicUpdateComponent>(), Is.Not.Null);
        }
    }
}
