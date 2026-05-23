using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Buff.Accessory;
using GenBall.BattleSystem.Buff.Player;
using GenBall.Framework.Config;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Yueyn.Base.ReferencePool;
using Yueyn.Main;

namespace GenBall.BattleSystem.Buff.Tests
{
    [TestFixture]
    public class BuffConfigTests
    {
        private GameObject _carrier;

        private class FakeConfigProvider : IConfigProvider
        {
            private readonly Dictionary<Type, object> _configs = new();
            public FakeConfigProvider(BuffModelConfig config) => _configs[typeof(BuffModelConfig)] = config;
            public void Init() { }
            public void UnInit() { }
            public T GetConfig<T>() where T : class => _configs.TryGetValue(typeof(T), out var c) ? c as T : null;
        }

        [SetUp]
        public void SetUp()
        {
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
            if (SystemRepository.Instance.HasSystem<IBuffRegistry>())
                SystemRepository.Instance.UnregisterSystem<IBuffRegistry>();
            _carrier = new GameObject("test_carrier");
        }

        [TearDown]
        public void TearDown()
        {
            if (_carrier != null)
                GameObject.DestroyImmediate(_carrier);
            if (SystemRepository.Instance.HasSystem<IBuffRegistry>())
                SystemRepository.Instance.UnregisterSystem<IBuffRegistry>();
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
        }

        /// <summary>
        /// Helper: create a BuffModelConfig with programmatic entries via SerializedObject.
        /// </summary>
        private static BuffModelConfig CreateConfigWithModels(params (string id, string typeName)[] entries)
        {
            var config = ScriptableObject.CreateInstance<BuffModelConfig>();
            var so = new SerializedObject(config);
            var list = so.FindProperty("buffModels");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("buffId").stringValue = entries[i].id;
                el.FindPropertyRelative("buffType").stringValue = entries[i].typeName;
            }
            so.ApplyModifiedProperties();
            return config;
        }

        [Test]
        public void BuffIdConstants_HaveCorrectValues()
        {
            Assert.That(BuffIdConstants.PlayerArmor, Is.EqualTo("PlayerArmor"));
            Assert.That(BuffIdConstants.BulletDamageUp, Is.EqualTo("BulletDamageUp"));
        }

        [Test]
        public void BuffModelConfig_Init_BuildsTypeCache()
        {
            var config = CreateConfigWithModels(
                ("PlayerArmor", typeof(ArmorBuff).AssemblyQualifiedName),
                ("BulletDamageUp", typeof(BulletDamageUpBuff).AssemblyQualifiedName)
            );
            config.Init();

            Assert.That(config.GetBuffType("PlayerArmor"), Is.EqualTo(typeof(ArmorBuff)));
            Assert.That(config.GetBuffType("BulletDamageUp"), Is.EqualTo(typeof(BulletDamageUpBuff)));
            Assert.That(config.GetBuffType("NonExistent"), Is.Null);
        }

        [Test]
        public void BuffModelConfig_GetBuffModel_ReturnsCorrectModel()
        {
            var config = CreateConfigWithModels(
                ("PlayerArmor", typeof(ArmorBuff).AssemblyQualifiedName)
            );
            config.Init();

            var model = config.GetBuffModel("PlayerArmor");
            Assert.That(model, Is.Not.Null);
            Assert.That(model.BuffId, Is.EqualTo("PlayerArmor"));
            Assert.That(model.BuffType, Is.EqualTo(typeof(ArmorBuff).AssemblyQualifiedName));
        }

        [Test]
        public void BuffModelConfig_GetBuffType_UnknownId_ReturnsNull()
        {
            var config = ScriptableObject.CreateInstance<BuffModelConfig>();
            config.Init();

            Assert.That(config.GetBuffType("NonExistent"), Is.Null);
        }

        [Test]
        public void BuffModelConfig_Init_SkipsNullOrEmptyType()
        {
            var config = CreateConfigWithModels(
                ("SomeBuff", string.Empty)
            );
            config.Init();

            // Entry exists in buff dict but NOT in type cache
            Assert.That(config.GetBuffModel("SomeBuff"), Is.Not.Null);
            Assert.That(config.GetBuffType("SomeBuff"), Is.Null);
        }

        [Test]
        public void BuffModelConfig_Init_InvalidTypeString_SkippedGracefully()
        {
            var config = CreateConfigWithModels(
                ("BadBuff", "This.Is.Not.A.Real.Type, FakeAssembly")
            );

            Assert.DoesNotThrow(() => config.Init());
            Assert.That(config.GetBuffType("BadBuff"), Is.Null);
        }

        [Test]
        public void BuffModelConfig_Init_DuplicateId_FirstWins()
        {
            var config = ScriptableObject.CreateInstance<BuffModelConfig>();
            var so = new SerializedObject(config);
            var list = so.FindProperty("buffModels");
            list.arraySize = 2;

            var el0 = list.GetArrayElementAtIndex(0);
            el0.FindPropertyRelative("buffId").stringValue = "SameId";
            el0.FindPropertyRelative("displayName").stringValue = "First";
            var el1 = list.GetArrayElementAtIndex(1);
            el1.FindPropertyRelative("buffId").stringValue = "SameId";
            el1.FindPropertyRelative("displayName").stringValue = "Second";
            so.ApplyModifiedProperties();

            config.Init();
            var model = config.GetBuffModel("SameId");

            Assert.That(model, Is.Not.Null);
            Assert.That(model.DisplayName, Is.EqualTo("First"));
        }

        [Test]
        public void AddBuffInfo_Create_WithValidId_ReturnsNonNull()
        {
            var config = CreateConfigWithModels(
                ("PlayerArmor", typeof(ArmorBuff).AssemblyQualifiedName)
            );
            config.Init();
            var fakeProvider = new FakeConfigProvider(config);
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(fakeProvider);

            var info = AddBuffInfo.Create("PlayerArmor", _carrier);

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Model, Is.Not.Null);
            Assert.That(info.Model.BuffId, Is.EqualTo("PlayerArmor"));
            Assert.That(info.Carrier, Is.SameAs(_carrier));
            ReferencePool.Release(info);
        }

        [Test]
        public void AddBuffInfo_Create_WithInvalidId_ReturnsNullAndLogsError()
        {
            var config = ScriptableObject.CreateInstance<BuffModelConfig>();
            config.Init();
            var fakeProvider = new FakeConfigProvider(config);
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(fakeProvider);

            LogAssert.Expect(LogType.Error, "gzp 未找到BuffId：NonExistent对应的BuffModel配置");
            var info = AddBuffInfo.Create("NonExistent", _carrier);

            Assert.That(info, Is.Null);
        }

        [Test]
        public void BuffObj_Create_MissingBuffType_ReturnsNullAndLogsError()
        {
            var config = ScriptableObject.CreateInstance<BuffModelConfig>();
            config.Init();
            var fakeProvider = new FakeConfigProvider(config);
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(fakeProvider);

            // Acquire AddBuffInfo with a model whose BuffId won't resolve to a type
            var info = ReferencePool.Acquire<AddBuffInfo>();
            var modelField = typeof(AddBuffInfo).GetProperty("Model");
            var modelSo = new SerializedObject(config);
            var list = modelSo.FindProperty("buffModels");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).FindPropertyRelative("buffId").stringValue = "NoTypeBuff";
            list.GetArrayElementAtIndex(0).FindPropertyRelative("buffType").stringValue = string.Empty;
            modelSo.ApplyModifiedProperties();
            config.Init();

            // Build a minimal model manually
            var model = config.GetBuffModel("NoTypeBuff");
            Assert.That(model, Is.Not.Null, "Sanity: model should exist even without type");

            // Set model on info via reflection (property has private set)
            typeof(AddBuffInfo).GetProperty("Model")?.SetMethod?.Invoke(info, new object[] { model });
            info.GetType().GetProperty("Carrier")?.SetMethod?.Invoke(info, new object[] { _carrier });

            LogAssert.Expect(LogType.Error, "gzp 创建BuffObj失败: Type not found for BuffId=NoTypeBuff");
            var buffObj = BuffObj.Create(info);

            Assert.That(buffObj, Is.Null);
            ReferencePool.Release(info);
        }
    }
}
