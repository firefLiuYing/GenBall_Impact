using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Tests
{
    [TestFixture]
    public class IncrementalSaveTests
    {
        private GameManager _gameManager;
        private FakeSaveService _fakeSaveService;

        [SetUp]
        public void SetUp()
        {
            // Clean up shared singleton state (other test fixtures may have registered these)
            if (SystemRepository.Instance.HasSystem<ISaveService>())
            {
                SystemRepository.Instance.UnregisterSystem<ISaveService>();
            }

            _gameManager = new GameManager();
            _gameManager.Init();

            _fakeSaveService = new FakeSaveService();
            SystemRepository.Instance.RegisterSystem<ISaveService>(_fakeSaveService);
        }

        [TearDown]
        public void TearDown()
        {
            _gameManager.UnInit();
            if (SystemRepository.Instance.HasSystem<ISaveService>())
            {
                SystemRepository.Instance.UnregisterSystem<ISaveService>();
            }
        }

        // ---- GameManager integration tests (with FakeSaveService) ----

        [Test]
        public void UpdateSaveFields_NoActiveSlot_ReturnsFalse()
        {
            _gameManager.CurSaveIndex = -1;

            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSceneName, "SomeScene" }
            };

            Assert.That(_gameManager.UpdateSaveFields("Player", fields).Result, Is.False);
        }

        [Test]
        public void UpdateSaveFields_UnregisteredProvider_ReturnsFalse()
        {
            _gameManager.CurSaveIndex = 0;

            var fields = new Dictionary<string, string>
            {
                { "someField", "someValue" }
            };

            Assert.That(_gameManager.UpdateSaveFields("UnknownProvider", fields).Result, Is.False);
        }

        [Test]
        public void UpdateSaveFields_NullFields_ReturnsTrue()
        {
            _gameManager.CurSaveIndex = 0;

            Assert.That(_gameManager.UpdateSaveFields("Player", null).Result, Is.True);
        }

        [Test]
        public void UpdateSaveFields_EmptyFields_ReturnsTrue()
        {
            _gameManager.CurSaveIndex = 0;

            Assert.That(_gameManager.UpdateSaveFields("Player", new Dictionary<string, string>()).Result, Is.True);
        }

        [Test]
        public void UpdateSaveFields_ValidPlayerUpdate_PersistsAndMerges()
        {
            // Arrange: register player provider and pre-populate save slot
            var playerProvider = new PlayerSaveDataProvider();
            _gameManager.RegisterSaveDataProvider(playerProvider);
            _gameManager.CurSaveIndex = 0;

            var initialData = new GameData();
            initialData.SetData("Player", playerProvider.CollectSaveData());
            _fakeSaveService.SaveGameData(initialData, 0).Wait();

            // Act: update a single field
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSceneName, "Forest" }
            };
            var result = _gameManager.UpdateSaveFields("Player", fields).Result;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(playerProvider.RuntimeData.lastSceneName, Is.EqualTo("Forest"));

            // Verify persistence round-trip
            var loaded = _fakeSaveService.LoadGameData(0).Result;
            Assert.That(loaded, Is.Not.Null);
            var playerJson = loaded.GetData("Player");
            var restored = JsonUtility.FromJson<PlayerSaveData>(playerJson);
            Assert.That(restored.lastSceneName, Is.EqualTo("Forest"));
        }

        // ---- Provider MergeSaveFields tests (unit tests, no disk I/O) ----

        [Test]
        public void PlayerSaveDataProvider_MergeSaveFields_UpdatesLastSavePointIndex()
        {
            var provider = new PlayerSaveDataProvider();
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSavePointIndex, "5" }
            };

            provider.MergeSaveFields(fields);

            Assert.That(provider.RuntimeData.lastSavePointIndex, Is.EqualTo(5));
        }

        [Test]
        public void PlayerSaveDataProvider_MergeSaveFields_UpdatesLastSceneName()
        {
            var provider = new PlayerSaveDataProvider();
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSceneName, "Village" }
            };

            provider.MergeSaveFields(fields);

            Assert.That(provider.RuntimeData.lastSceneName, Is.EqualTo("Village"));
        }

        [Test]
        public void PlayerSaveDataProvider_MergeSaveFields_InvalidInt_Ignored()
        {
            var provider = new PlayerSaveDataProvider();
            provider.RuntimeData.lastSavePointIndex = 3;
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSavePointIndex, "abc" }
            };

            Assert.DoesNotThrow(() => provider.MergeSaveFields(fields));
            Assert.That(provider.RuntimeData.lastSavePointIndex, Is.EqualTo(3),
                "Invalid int should be ignored, original value preserved");
        }

        [Test]
        public void PlayerSaveDataProvider_MergeSaveFields_MultipleFieldsUpdated()
        {
            var provider = new PlayerSaveDataProvider();
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Player.LastSavePointIndex, "7" },
                { SaveFieldKeys.Player.LastSceneName, "Castle" }
            };

            provider.MergeSaveFields(fields);

            Assert.That(provider.RuntimeData.lastSavePointIndex, Is.EqualTo(7));
            Assert.That(provider.RuntimeData.lastSceneName, Is.EqualTo("Castle"));
        }

        [Test]
        public void MapSaveDataProvider_MergeSaveFields_UpdatesUnlockedScenes()
        {
            var provider = new MapSaveDataProvider();
            var newScenes = new List<SceneSaveData>
            {
                new SceneSaveData
                {
                    sceneName = "Forest",
                    unlockedSavePoints = new List<int> { 1, 2 },
                }
            };
            var wrapper = new SceneSaveDataListWrapperTest { scenes = newScenes };
            var scenesJson = JsonUtility.ToJson(wrapper);

            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Map.UnlockedScenes, scenesJson }
            };

            provider.MergeSaveFields(fields);

            Assert.That(provider.RuntimeData.unlockedScenes.Count, Is.EqualTo(1));
            Assert.That(provider.RuntimeData.unlockedScenes[0].sceneName, Is.EqualTo("Forest"));
            Assert.That(provider.RuntimeData.unlockedScenes[0].unlockedSavePoints, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void MapSaveDataProvider_MergeSaveFields_NullScenesJson_Ignored()
        {
            var provider = new MapSaveDataProvider();
            provider.RuntimeData.unlockedScenes.Add(new SceneSaveData { sceneName = "Existing" });

            // This would parse to null from empty json for SceneSaveDataListWrapper
            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Map.UnlockedScenes, "" }
            };

            Assert.DoesNotThrow(() => provider.MergeSaveFields(fields));
            // Empty json produces a wrapper with null scenes list, so original data is preserved
        }

        [Test]
        public void MapSaveDataProvider_MergeSaveFields_EmptyList_ReplacesScenes()
        {
            var provider = new MapSaveDataProvider();
            provider.RuntimeData.unlockedScenes.Add(new SceneSaveData { sceneName = "Existing" });

            var wrapper = new SceneSaveDataListWrapperTest { scenes = new List<SceneSaveData>() };
            var scenesJson = JsonUtility.ToJson(wrapper);

            var fields = new Dictionary<string, string>
            {
                { SaveFieldKeys.Map.UnlockedScenes, scenesJson }
            };

            provider.MergeSaveFields(fields);

            Assert.That(provider.RuntimeData.unlockedScenes.Count, Is.EqualTo(0));
        }

        // ---- SaveFieldKeys constant validity tests ----

        [Test]
        public void SaveFieldKeys_Player_ConstantsValid()
        {
            Assert.That(SaveFieldKeys.Player.LastSavePointIndex, Is.EqualTo("lastSavePointIndex"));
            Assert.That(SaveFieldKeys.Player.LastSceneName, Is.EqualTo("lastSceneName"));
        }

        [Test]
        public void SaveFieldKeys_Map_ConstantsValid()
        {
            Assert.That(SaveFieldKeys.Map.UnlockedScenes, Is.EqualTo("unlockedScenes"));
        }

        [Test]
        public void SaveFieldKeys_Player_ConstantsMatchSerializedFields()
        {
            // Verify constants match actual [Serializable] field names in PlayerSaveData
            var data = new PlayerSaveData { lastSavePointIndex = 99, lastSceneName = "Verify" };
            var json = JsonUtility.ToJson(data);

            Assert.That(json, Does.Contain(SaveFieldKeys.Player.LastSavePointIndex));
            Assert.That(json, Does.Contain(SaveFieldKeys.Player.LastSceneName));
        }

        [Test]
        public void SaveFieldKeys_Map_ConstantsMatchSerializedFields()
        {
            // Verify constant matches actual [Serializable] field name in MapSaveData
            var data = new MapSaveData();
            var json = JsonUtility.ToJson(data);

            Assert.That(json, Does.Contain(SaveFieldKeys.Map.UnlockedScenes));
        }
    }

    /// <summary>
    /// In-memory fake ISaveService for testing without disk I/O.
    /// </summary>
    internal class FakeSaveService : ISaveService
    {
        private readonly Dictionary<int, GameData> _store = new();

        public int MaxSaveCount => 6;

        public void Init() { }
        public void UnInit() { }

        public Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas()
        {
            return Task.FromResult<IEnumerable<SaveSlotData>>(new List<SaveSlotData>());
        }

        public Task<GameData> LoadGameData(int saveIndex)
        {
            _store.TryGetValue(saveIndex, out var data);
            return Task.FromResult(data);
        }

        public Task<bool> SaveGameData(GameData gameData, int saveIndex)
        {
            _store[saveIndex] = gameData;
            return Task.FromResult(true);
        }

        public Task<int> CreateNewSave()
        {
            return Task.FromResult(0);
        }

        public Task<bool> DeleteSave(int saveIndex)
        {
            _store.Remove(saveIndex);
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Helper wrapper for testing MapSaveDataProvider.MergeSaveFields.
    /// Mirrors the internal SceneSaveDataListWrapper used by the provider.
    /// </summary>
    [System.Serializable]
    internal class SceneSaveDataListWrapperTest
    {
        public List<SceneSaveData> scenes;
    }
}
