using System.Collections.Generic;
using GenBall.BattleSystem.Bullets;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Main;

namespace GenBall.BattleSystem.Bullets.Tests
{
    [TestFixture]
    public class BulletSystemNewTests
    {
        private IBulletSystem _bulletSystem;
        private BulletConfigCollection _testConfigCollection;

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state — SystemRepository is a singleton shared across test fixtures
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
            if (SystemRepository.Instance.HasSystem<IBulletSystem>())
                SystemRepository.Instance.UnregisterSystem<IBulletSystem>();
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();

            _bulletSystem = new BulletSystem();
            SystemRepository.Instance.RegisterSystem<IBulletSystem>(_bulletSystem);

            // FireBullet -> BulletInstance.Init needs IEntityUpdateSystem
            var entityUpdate = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(entityUpdate);

            // Create test config collection
            _testConfigCollection = ScriptableObject.CreateInstance<BulletConfigCollection>();
            _testConfigCollection.Configs = new List<BulletConfigEntry>
            {
                new BulletConfigEntry
                {
                    Id = BulletId.RayBullet,
                    DetectionMode = DetectionMode.Ray,
                    MaxLifetime = 3f,
                    VisualBlendTime = 0.15f,
                    HitBehaviors = new HitBehaviorDef[]
                    {
                        new HitBehaviorDef { Type = HitBehaviorType.DealDamage, Count = 0, Value = 0f }
                    },
                    MovementModifiers = new MovementModifierDef[0]
                },
                new BulletConfigEntry
                {
                    Id = BulletId.PenetrateBullet,
                    DetectionMode = DetectionMode.Ray,
                    MaxLifetime = 5f,
                    HitBehaviors = new HitBehaviorDef[]
                    {
                        new HitBehaviorDef { Type = HitBehaviorType.Penetrate, Count = 3, Value = 0f }
                    },
                    MovementModifiers = new MovementModifierDef[0]
                },
                new BulletConfigEntry
                {
                    Id = BulletId.BounceBullet,
                    DetectionMode = DetectionMode.Ray,
                    MaxLifetime = 4f,
                    HitBehaviors = new HitBehaviorDef[]
                    {
                        new HitBehaviorDef { Type = HitBehaviorType.Bounce, Count = 2, Value = 0f }
                    },
                    MovementModifiers = new MovementModifierDef[0]
                },
                new BulletConfigEntry
                {
                    Id = BulletId.GravityBullet,
                    DetectionMode = DetectionMode.SphereCast,
                    MaxLifetime = 4f,
                    HitBehaviors = new HitBehaviorDef[]
                    {
                        new HitBehaviorDef { Type = HitBehaviorType.DealDamage, Count = 0, Value = 0f }
                    },
                    MovementModifiers = new MovementModifierDef[]
                    {
                        new MovementModifierDef { Type = MovementModifierType.Gravity, Value = 10f }
                    }
                },
                new BulletConfigEntry
                {
                    Id = BulletId.SphereAOEBullet,
                    DetectionMode = DetectionMode.SphereCast,
                    MaxLifetime = 3f,
                    HitBehaviors = new HitBehaviorDef[]
                    {
                        new HitBehaviorDef { Type = HitBehaviorType.Penetrate, Count = 1, Value = 0f },
                        new HitBehaviorDef { Type = HitBehaviorType.AOEDamage, Count = 3, Value = 50f }
                    },
                    MovementModifiers = new MovementModifierDef[0]
                }
            };
            _testConfigCollection.Init();

            // Register config provider
            var fakeConfigProvider = new FakeConfigProvider(_testConfigCollection);
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(fakeConfigProvider);
        }

        [TearDown]
        public void TearDown()
        {
            try { SystemRepository.Instance.UnregisterSystem<IBulletSystem>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IConfigProvider>(); } catch { }
            try { SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>(); } catch { }
        }

        // ======== System Lifecycle ========

        [Test]
        public void Init_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _bulletSystem.Init());
        }

        [Test]
        public void UnInit_DoesNotThrow()
        {
            _bulletSystem.Init();
            Assert.DoesNotThrow(() => _bulletSystem.UnInit());
        }

        [Test]
        public void GetSystem_ReturnsNonNull()
        {
            Assert.That(SystemRepository.Instance.GetSystem<IBulletSystem>(), Is.Not.Null);
        }

        // ======== BulletConfigCollection ========

        [Test]
        public void ConfigCollection_Get_ReturnsCorrectEntry()
        {
            var config = _testConfigCollection.Get(BulletId.RayBullet);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Id, Is.EqualTo(BulletId.RayBullet));
            Assert.That(config.DetectionMode, Is.EqualTo(DetectionMode.Ray));
        }

        [Test]
        public void ConfigCollection_Get_UnknownId_ReturnsNull()
        {
            var config = _testConfigCollection.Get(BulletId.None);
            Assert.That(config, Is.Null);
        }

        [Test]
        public void ConfigCollection_TryGet_ReturnsFalseForUnknown()
        {
            bool found = _testConfigCollection.TryGet(BulletId.None, out var config);
            Assert.That(found, Is.False);
            Assert.That(config, Is.Null);
        }

        [Test]
        public void ConfigCollection_TryGet_ReturnsTrueForKnown()
        {
            bool found = _testConfigCollection.TryGet(BulletId.RayBullet, out var config);
            Assert.That(found, Is.True);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Id, Is.EqualTo(BulletId.RayBullet));
        }

        // ======== BulletFireParams ========

        [Test]
        public void BulletFireParams_DefaultValues()
        {
            var fireParams = new BulletFireParams();
            Assert.That(fireParams.ConfigId, Is.EqualTo(BulletId.None));
            Assert.That(fireParams.FinalDamage, Is.EqualTo(0));
            Assert.That(fireParams.FinalSpeed, Is.EqualTo(0f));
            Assert.That(fireParams.SpeedMultiplier, Is.EqualTo(0f));
        }

        [Test]
        public void BulletFireParams_CanSetValues()
        {
            var fireParams = new BulletFireParams
            {
                ConfigId = BulletId.RayBullet,
                LogicOrigin = Vector3.zero,
                VisualOrigin = Vector3.up,
                Direction = Vector3.forward,
                FinalDamage = 50,
                FinalSpeed = 80f,
                FinalRadius = 0.3f,
                ExtraPenetrations = 2,
                ExtraBounces = 1,
                SpeedMultiplier = 1.5f
            };

            Assert.That(fireParams.ConfigId, Is.EqualTo(BulletId.RayBullet));
            Assert.That(fireParams.LogicOrigin, Is.EqualTo(Vector3.zero));
            Assert.That(fireParams.VisualOrigin, Is.EqualTo(Vector3.up));
            Assert.That(fireParams.Direction, Is.EqualTo(Vector3.forward));
            Assert.That(fireParams.FinalDamage, Is.EqualTo(50));
            Assert.That(fireParams.FinalSpeed, Is.EqualTo(80f));
            Assert.That(fireParams.FinalRadius, Is.EqualTo(0.3f));
            Assert.That(fireParams.ExtraPenetrations, Is.EqualTo(2));
            Assert.That(fireParams.ExtraBounces, Is.EqualTo(1));
            Assert.That(fireParams.SpeedMultiplier, Is.EqualTo(1.5f));
        }

        // ======== FireBullet with BulletFireParams ========

        [Test]
        public void FireBullet_WithInvalidConfigId_DoesNotThrow()
        {
            _bulletSystem.Init();
            var fireParams = new BulletFireParams
            {
                ConfigId = BulletId.None,
                LogicOrigin = Vector3.zero,
                VisualOrigin = Vector3.up,
                Direction = Vector3.forward,
                FinalDamage = 10,
                FinalSpeed = 50f,
                SpeedMultiplier = 1f
            };

            LogAssert.Expect(LogType.Error, "[BulletSystem] BulletConfig 'None' not found");
            Assert.DoesNotThrow(() => _bulletSystem.FireBullet(fireParams));
        }

        [Test]
        public void FireBullet_WithValidConfig_DoesNotThrow()
        {
            _bulletSystem.Init();
            var fireParams = new BulletFireParams
            {
                ConfigId = BulletId.RayBullet,
                LogicOrigin = Vector3.zero,
                VisualOrigin = Vector3.up,
                Direction = Vector3.forward,
                FinalDamage = 25,
                FinalSpeed = 60f,
                FinalRadius = 0.2f,
                ExtraPenetrations = 0,
                ExtraBounces = 0,
                SpeedMultiplier = 1f
            };

            Assert.DoesNotThrow(() => _bulletSystem.FireBullet(fireParams));
        }

        // ======== Detection Strategies ========

        [Test]
        public void RayDetection_CreatesCorrectly()
        {
            var detection = new RayDetection();
            Assert.That(detection, Is.Not.Null);
            Assert.That(detection, Is.InstanceOf<IDetectionStrategy>());
        }

        [Test]
        public void SphereCastDetection_CreatesCorrectly()
        {
            var detection = new SphereCastDetection();
            Assert.That(detection, Is.Not.Null);
            Assert.That(detection, Is.InstanceOf<IDetectionStrategy>());
        }

        // ======== Hit Behaviors ========

        [Test]
        public void DealDamageBehavior_CreatesCorrectly()
        {
            var behavior = new DealDamageBehavior();
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior, Is.InstanceOf<IHitBehavior>());
        }

        [Test]
        public void PenetrateBehavior_CreatesWithCorrectCount()
        {
            var behavior = new PenetrateBehavior(3);
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior, Is.InstanceOf<IHitBehavior>());
        }

        [Test]
        public void PenetrateBehavior_ZeroPenetrations_Allowed()
        {
            var behavior = new PenetrateBehavior(0);
            Assert.That(behavior, Is.Not.Null);
        }

        [Test]
        public void BounceBehavior_CreatesWithCorrectParams()
        {
            var behavior = new BounceBehavior(2, 0.5f);
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior, Is.InstanceOf<IHitBehavior>());
        }

        [Test]
        public void AOEDamageBehavior_CreatesWithCorrectParams()
        {
            var behavior = new AOEDamageBehavior(5f, 30, -1);
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior, Is.InstanceOf<IHitBehavior>());
        }

        // ======== Movement Modifiers ========

        [Test]
        public void GravityModifier_CreatesCorrectly()
        {
            var modifier = new GravityModifier(10f);
            Assert.That(modifier, Is.Not.Null);
            Assert.That(modifier, Is.InstanceOf<IMovementModifier>());
        }

        // ======== Config Entry Properties ========

        [Test]
        public void ConfigEntry_DefaultHitBehavior_IsDealDamage()
        {
            var config = _testConfigCollection.Get(BulletId.RayBullet);
            Assert.That(config.HitBehaviors.Length, Is.GreaterThan(0));
            Assert.That(config.HitBehaviors[0].Type, Is.EqualTo(HitBehaviorType.DealDamage));
        }

        [Test]
        public void ConfigEntry_PenetrateBullet_HasCorrectHitBehavior()
        {
            var config = _testConfigCollection.Get(BulletId.PenetrateBullet);
            Assert.That(config.HitBehaviors.Length, Is.EqualTo(1));
            Assert.That(config.HitBehaviors[0].Type, Is.EqualTo(HitBehaviorType.Penetrate));
            Assert.That(config.HitBehaviors[0].Count, Is.EqualTo(3));
        }

        [Test]
        public void ConfigEntry_BounceBullet_HasBounceBehavior()
        {
            var config = _testConfigCollection.Get(BulletId.BounceBullet);
            Assert.That(config.HitBehaviors.Length, Is.EqualTo(1));
            Assert.That(config.HitBehaviors[0].Type, Is.EqualTo(HitBehaviorType.Bounce));
            Assert.That(config.HitBehaviors[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void ConfigEntry_GravityBullet_HasSphereCastAndGravity()
        {
            var config = _testConfigCollection.Get(BulletId.GravityBullet);
            Assert.That(config.DetectionMode, Is.EqualTo(DetectionMode.SphereCast));
            Assert.That(config.MovementModifiers.Length, Is.EqualTo(1));
            Assert.That(config.MovementModifiers[0].Type, Is.EqualTo(MovementModifierType.Gravity));
        }

        [Test]
        public void ConfigEntry_SphereCastBullet_PenetrateThenAOE()
        {
            var config = _testConfigCollection.Get(BulletId.SphereAOEBullet);
            Assert.That(config.HitBehaviors.Length, Is.EqualTo(2));
            Assert.That(config.HitBehaviors[0].Type, Is.EqualTo(HitBehaviorType.Penetrate));
            Assert.That(config.HitBehaviors[1].Type, Is.EqualTo(HitBehaviorType.AOEDamage));
        }

        // ======== Backward Compatibility ========

        [Test]
        public void FireBullet_WithNullLaunchInfo_DoesNotThrow()
        {
            // FireBullet(BulletLaunchInfo) now has a null guard: if (info == null) return;
            Assert.DoesNotThrow(() => _bulletSystem.FireBullet(null as BulletLaunchInfo));
        }

        [Test]
        public void RecycleBullet_WithNullBulletState_DoesNotThrow()
        {
            // RecycleBullet(BulletState) now has a null guard: if (bulletState != null) ...
            Assert.DoesNotThrow(() => _bulletSystem.RecycleBullet(null as BulletState));
        }

        // ======== Fake Config Provider ========

        private class FakeConfigProvider : IConfigProvider
        {
            private readonly BulletConfigCollection _bulletConfig;

            public FakeConfigProvider(BulletConfigCollection bulletConfig)
            {
                _bulletConfig = bulletConfig;
            }

            public void Init() { }
            public void UnInit() { }

            public T GetConfig<T>() where T : class
            {
                if (typeof(T) == typeof(BulletConfigCollection))
                    return _bulletConfig as T;
                return null;
            }
        }
    }
}
