using GenBall.Map;
using GenBall.Player;
using NUnit.Framework;
using UnityEngine;

namespace GenBall.Procedure.Tests
{
    [TestFixture]
    public class SaveDataProviderTests
    {
        [Test]
        public void PlayerSaveDataProvider_RoundTrip_PreservesData()
        {
            var provider = new PlayerSaveDataProvider();
            provider.RuntimeData.lastSceneName = "TestScene";
            provider.RuntimeData.lastSavePointIndex = 3;

            var json = provider.CollectSaveData();
            Assert.That(json, Is.Not.Null.And.Not.Empty);

            // Apply to a fresh provider
            var target = new PlayerSaveDataProvider();
            target.ApplySaveData(json);

            Assert.That(target.RuntimeData.lastSceneName, Is.EqualTo("TestScene"));
            Assert.That(target.RuntimeData.lastSavePointIndex, Is.EqualTo(3));
        }

        [Test]
        public void PlayerSaveDataProvider_EmptyJson_KeepsDefaults()
        {
            var provider = new PlayerSaveDataProvider();
            provider.ApplySaveData("");

            Assert.That(provider.RuntimeData.lastSceneName, Is.EqualTo(""));
            Assert.That(provider.RuntimeData.lastSavePointIndex, Is.EqualTo(0));
        }

        [Test]
        public void PlayerSaveDataProvider_NullJson_KeepsDefaults()
        {
            var provider = new PlayerSaveDataProvider();
            provider.ApplySaveData(null);

            Assert.That(provider.RuntimeData.lastSceneName, Is.EqualTo(""));
            Assert.That(provider.RuntimeData.lastSavePointIndex, Is.EqualTo(0));
        }

        [Test]
        public void MapSaveDataProvider_RoundTrip_PreservesData()
        {
            var provider = new MapSaveDataProvider();
            provider.RuntimeData.unlockedScenes.Add(new SceneSaveData
            {
                sceneName = "Prologue",
                unlockedSavePoints = { 1, 2 },
                killedEnemyUnits = { 10, 20 },
            });

            var json = provider.CollectSaveData();
            Assert.That(json, Is.Not.Null.And.Not.Empty);

            var target = new MapSaveDataProvider();
            target.ApplySaveData(json);

            Assert.That(target.RuntimeData.unlockedScenes.Count, Is.EqualTo(1));
            Assert.That(target.RuntimeData.unlockedScenes[0].sceneName, Is.EqualTo("Prologue"));
            Assert.That(target.RuntimeData.unlockedScenes[0].unlockedSavePoints, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(target.RuntimeData.unlockedScenes[0].killedEnemyUnits, Is.EquivalentTo(new[] { 10, 20 }));
        }

        [Test]
        public void MapSaveDataProvider_EmptyJson_KeepsDefaults()
        {
            var provider = new MapSaveDataProvider();
            provider.ApplySaveData("");

            Assert.That(provider.RuntimeData.unlockedScenes, Is.Not.Null);
            Assert.That(provider.RuntimeData.unlockedScenes.Count, Is.EqualTo(0));
        }

        [Test]
        public void GameData_SetData_GetData_RoundTrip()
        {
            var gameData = new GameData();
            gameData.SetData("Player", "{\"lastSceneName\":\"Test\"}");
            gameData.SetData("Map", "{\"unlockedScenes\":[]}");

            Assert.That(gameData.GetData("Player"), Is.EqualTo("{\"lastSceneName\":\"Test\"}"));
            Assert.That(gameData.GetData("Map"), Is.EqualTo("{\"unlockedScenes\":[]}"));
            Assert.That(gameData.GetData("Nonexistent"), Is.Null);
        }

        [Test]
        public void GameData_HasData_ReturnsCorrectly()
        {
            var gameData = new GameData();
            Assert.That(gameData.HasData("Player"), Is.False);

            gameData.SetData("Player", "data");
            Assert.That(gameData.HasData("Player"), Is.True);
        }

        [Test]
        public void GameData_JsonUtility_RoundTrip_PreservesDataBlocks()
        {
            var original = new GameData();
            original.SetData("Player", "player_json");
            original.SetData("Map", "map_json");

            var json = JsonUtility.ToJson(original);
            var restored = JsonUtility.FromJson<GameData>(json);

            Assert.That(restored.GetData("Player"), Is.EqualTo("player_json"));
            Assert.That(restored.GetData("Map"), Is.EqualTo("map_json"));
        }

        [Test]
        public void GameData_DataBlocks_IteratesAllBlocks()
        {
            var gameData = new GameData();
            gameData.SetData("A", "1");
            gameData.SetData("B", "2");

            int count = 0;
            foreach (var block in gameData.DataBlocks)
            {
                count++;
            }

            Assert.That(count, Is.EqualTo(2));
        }
    }
}
