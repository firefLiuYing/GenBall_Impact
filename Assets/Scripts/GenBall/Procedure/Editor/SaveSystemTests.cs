using System;
using GenBall.Framework.Config;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Tests
{
    public class SaveSystemTests
    {
        private MockConfigProvider _mockConfig;

        [SetUp]
        public void SetUp()
        {
            _mockConfig = new MockConfigProvider();
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
            {
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
            }
        }

        [Test]
        public void Init_NoConfigProvider_UsesDefaultMaxSaveCount()
        {
            // Arrange: IConfigProvider not registered
            // SystemRepository starts empty for this test

            // Act
            var saveSystem = new SaveSystem();
            saveSystem.Init();

            // Assert
            Assert.That(saveSystem.MaxSaveCount, Is.EqualTo(6));
        }

        [Test]
        public void Init_WithConfigProvider_ReadsMaxSaveCountFromConfig()
        {
            // Arrange
            _mockConfig.MaxSaveCount = 10;
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(_mockConfig);

            // Act
            var saveSystem = new SaveSystem();
            saveSystem.Init();

            // Assert
            Assert.That(saveSystem.MaxSaveCount, Is.EqualTo(10));
        }

        [Test]
        public void Init_UnInit_DoesNotThrow()
        {
            // Arrange
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(_mockConfig);
            var saveSystem = new SaveSystem();
            saveSystem.Init();

            // Act & Assert
            Assert.DoesNotThrow(() => saveSystem.UnInit());
        }
    }

    internal class MockConfigProvider : IConfigProvider
    {
        public int MaxSaveCount { get; set; } = 6;

        public void Init()
        {
            Debug.Log("[MockConfigProvider] Init");
        }

        public void UnInit()
        {
            Debug.Log("[MockConfigProvider] UnInit");
        }

        public T GetConfig<T>() where T : class
        {
            if (typeof(T) == typeof(AppSettingsConfig))
            {
                var config = ScriptableObject.CreateInstance<AppSettingsConfig>();
                config.maxSaveCount = MaxSaveCount;
                return config as T;
            }
            return null;
        }
    }
}
 
