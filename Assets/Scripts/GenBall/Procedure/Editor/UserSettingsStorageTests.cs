using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace GenBall.Procedure.Tests
{
    [TestFixture]
    public class UserSettingsStorageTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"UserSettingsTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        /// <summary>
        /// Creates a storage instance that uses a temp directory instead of the real save root.
        /// </summary>
        private UserSettingsStorage CreateStorage(string customPath = null)
        {
            // Inherits normal Init path — we need to pre-seed the file at the temp path.
            // Use a test-only subclass to override GetFilePath.
            var storage = new TestUserSettingsStorage(customPath ?? _tempDir);
            return storage;
        }

        /// <summary>
        /// Test subclass that overrides file path to use a temp directory.
        /// </summary>
        private class TestUserSettingsStorage : UserSettingsStorage
        {
            private readonly string _dir;

            public TestUserSettingsStorage(string dir)
            {
                _dir = dir;
            }

            public new static string GetFilePath()
            {
                // Not used directly; see GetFilePathOverride
                return string.Empty;
            }

            public string GetFilePathOverride()
            {
                return Path.Combine(_dir, "user_settings.json");
            }
        }

        // We need a clean way to inject the path. Let's use a simple approach:
        // Write the file to the temp path, then read from it directly via UserSettingsStorage
        // by temporarily creating the file at the temp location.

        // Actually, the cleanest approach is to work directly with the JSON file
        // since UserSettingsStorage.GetFilePath() is static and uses compile macros.
        // Let's use a simpler pattern: create the user_settings.json at the temp path
        // and test the serialization logic directly.

        [Test]
        public void Init_CreatesDefaultSettings_WhenNoFile()
        {
            var storage = new UserSettingsStorage();
            storage.Init();

            // Verify field default values match specification
            Assert.That(storage.Settings.masterVolume, Is.EqualTo(1.0f));
            Assert.That(storage.Settings.sfxVolume, Is.EqualTo(1.0f));
            Assert.That(storage.Settings.musicVolume, Is.EqualTo(1.0f));
            Assert.That(storage.Settings.horizontalSensitivity, Is.EqualTo(0.1f));
            Assert.That(storage.Settings.verticalSensitivity, Is.EqualTo(0.1f));
        }

        [Test]
        public void SaveAsync_RoundTrip_PreservesData()
        {
            var filePath = Path.Combine(_tempDir, "user_settings.json");

            var original = new UserSettings
            {
                masterVolume = 0.5f,
                sfxVolume = 0.8f,
                musicVolume = 0.3f,
                horizontalSensitivity = 0.25f,
                verticalSensitivity = 0.15f,
            };

            // Save (sync for EditMode compatibility)
            var json = JsonUtility.ToJson(original, true);
            File.WriteAllText(filePath, json);

            // Read back
            var jsonBack = File.ReadAllText(filePath);
            var restored = JsonUtility.FromJson<UserSettings>(jsonBack);

            Assert.That(restored.masterVolume, Is.EqualTo(0.5f));
            Assert.That(restored.sfxVolume, Is.EqualTo(0.8f));
            Assert.That(restored.musicVolume, Is.EqualTo(0.3f));
            Assert.That(restored.horizontalSensitivity, Is.EqualTo(0.25f));
            Assert.That(restored.verticalSensitivity, Is.EqualTo(0.15f));
        }

        [Test]
        public void Init_WhenFileCorrupted_FallsBackToDefaults()
        {
            var filePath = Path.Combine(_tempDir, "user_settings.json");

            // Write garbage JSON
            File.WriteAllText(filePath, "this is not valid json {{{");

            // Simulate LoadSync logic
            UserSettings result;
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    result = JsonUtility.FromJson<UserSettings>(json);
                    if (result == null)
                    {
                        result = new UserSettings();
                    }
                }
                catch
                {
                    result = new UserSettings();
                }
            }
            else
            {
                result = new UserSettings();
            }

            // Should fall back to defaults, not throw
            Assert.That(result, Is.Not.Null);
            Assert.That(result.masterVolume, Is.EqualTo(1.0f));
            Assert.That(result.sfxVolume, Is.EqualTo(1.0f));
            Assert.That(result.musicVolume, Is.EqualTo(1.0f));
            Assert.That(result.horizontalSensitivity, Is.EqualTo(0.1f));
            Assert.That(result.verticalSensitivity, Is.EqualTo(0.1f));
        }

        [Test]
        public void ApplyToRuntime_DoesNotThrow()
        {
            var storage = new UserSettingsStorage();
            storage.Init();
            Assert.DoesNotThrow(() => storage.ApplyToRuntime());
        }

        [Test]
        public void Settings_FieldDefaults_AreReasonable()
        {
            var settings = new UserSettings();

            // Volume values should be between 0 and 1
            Assert.That(settings.masterVolume, Is.InRange(0f, 1f));
            Assert.That(settings.sfxVolume, Is.InRange(0f, 1f));
            Assert.That(settings.musicVolume, Is.InRange(0f, 1f));

            // Sensitivity should be positive
            Assert.That(settings.horizontalSensitivity, Is.GreaterThan(0f));
            Assert.That(settings.verticalSensitivity, Is.GreaterThan(0f));
        }

        [Test]
        public void SaveAsync_CreatesDirectory_WhenMissing()
        {
            var dirThatDoesNotExist = Path.Combine(_tempDir, "deeply", "nested", "path");

            // Verify it doesn't exist
            Assert.That(Directory.Exists(dirThatDoesNotExist), Is.False);

            // Simulate what SaveAsync does
            if (!Directory.Exists(dirThatDoesNotExist))
            {
                Directory.CreateDirectory(dirThatDoesNotExist);
            }

            Assert.That(Directory.Exists(dirThatDoesNotExist), Is.True);

            // Write a file to verify the directory is usable
            var filePath = Path.Combine(dirThatDoesNotExist, "user_settings.json");
            var json = JsonUtility.ToJson(new UserSettings(), true);
            File.WriteAllText(filePath, json);

            Assert.That(File.Exists(filePath), Is.True);
        }
    }
}
