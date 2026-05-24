using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.Framework.Config;
using GenBall.Interact;
using GenBall.Map;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Procedure.Tests
{
    /// <summary>
    /// Integration tests for Phase 2 ISystem migration.
    /// Verifies all systems can be registered and retrieved through SystemRepository.
    /// </summary>
    [TestFixture]
    public class Phase2IntegrationTests
    {
        private class FakeConfigProvider : IConfigProvider
        {
            public void Init() { }
            public void UnInit() { }
            public T GetConfig<T>() where T : class => null;
        }

        private class FakeSaveService : ISaveService
        {
            public int MaxSaveCount => 6;
            public Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas() =>
                Task.FromResult<IEnumerable<SaveSlotData>>(new List<SaveSlotData>());
            public Task<GameData> LoadGameData(int saveIndex) =>
                Task.FromResult<GameData>(null);
            public Task<bool> SaveGameData(GameData gameData, int saveIndex) =>
                Task.FromResult(true);
            public Task<int> CreateNewSave() => Task.FromResult(0);
            public Task<bool> DeleteSave(int saveIndex) => Task.FromResult(true);
            public void Init() { }
            public void UnInit() { }
        }

        private class TestLaunchSystem : ILaunchSystem
        {
            public RunningMode Mode => RunningMode.SaveData | RunningMode.LoadData;
            public string StartSceneName => "Test";
            public float SceneLoadProgress => 0f;
            public bool IsSceneLoading => false;
            public void Init() { }
            public void UnInit() { }
            public void StartNewGame() { }
            public void ContinueLastGame() { }
            public void LoadGame(int saveIndex) { }
            public void SkipSplash() { }
        }

        private class TestSceneExecutorSystem : ISceneExecutorSystem
        {
            public void Init() { }
            public void UnInit() { }
            public void ExecuteSceneSetup() { }
        }

        /// <summary>
        /// Unregisters a system if registered. Helper for cleanup between tests.
        /// </summary>
        private static void SafeUnregister<T>() where T : ISystem
        {
            if (SystemRepository.Instance.HasSystem<T>())
            {
                SystemRepository.Instance.UnregisterSystem<T>();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Unregister all in reverse order (cleanup any leftover registrations)
            SafeUnregister<ISceneExecutorSystem>();
            SafeUnregister<ILaunchSystem>();
            SafeUnregister<IGameManagerSystem>();
            SafeUnregister<IPauseSystem>();
            SafeUnregister<ITeleportSystem>();
            SafeUnregister<ISceneStateSystem>();
            SafeUnregister<IInteractSystem>();
            SafeUnregister<ISaveService>();
            SafeUnregister<IConfigProvider>();
        }

        [Test]
        public void AllSystems_RegisterAndRetrieve()
        {
            // Arrange
            var config = new FakeConfigProvider();
            var save = new FakeSaveService();
            var interact = new InteractSystem();
            var scene = new SceneSystem();
            var teleport = new TeleportSystem();
            var pause = new PauseManager();
            var gameManager = new GameManager();
            var launch = new TestLaunchSystem();
            var sceneExecutor = new TestSceneExecutorSystem();

            var repo = SystemRepository.Instance;

            // Act: register all systems in FrameworkDefault order
            Assert.That(() => repo.RegisterSystem<IConfigProvider>(config), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISaveService>(save), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IInteractSystem>(interact), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISceneStateSystem>(scene), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ITeleportSystem>(teleport), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IPauseSystem>(pause), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IGameManagerSystem>(gameManager), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ILaunchSystem>(launch), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISceneExecutorSystem>(sceneExecutor), Throws.Nothing);

            // Assert: all systems are retrievable
            Assert.That(repo.GetSystem<IConfigProvider>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISaveService>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IInteractSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISceneStateSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ITeleportSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IPauseSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IGameManagerSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ILaunchSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISceneExecutorSystem>(), Is.Not.Null);

            // Cleanup: unregister in reverse order
            repo.UnregisterSystem<ISceneExecutorSystem>();
            repo.UnregisterSystem<ILaunchSystem>();
            repo.UnregisterSystem<IGameManagerSystem>();
            repo.UnregisterSystem<IPauseSystem>();
            repo.UnregisterSystem<ITeleportSystem>();
            repo.UnregisterSystem<ISceneStateSystem>();
            repo.UnregisterSystem<IInteractSystem>();
            repo.UnregisterSystem<ISaveService>();
            repo.UnregisterSystem<IConfigProvider>();
        }

        [Test]
        public void DuplicateRegistration_Throws()
        {
            // Arrange
            var interact1 = new InteractSystem();
            var interact2 = new InteractSystem();

            // Act
            SystemRepository.Instance.RegisterSystem<IInteractSystem>(interact1);

            // Assert: duplicate registration throws
            Assert.That(() => SystemRepository.Instance.RegisterSystem<IInteractSystem>(interact2),
                Throws.Exception);

            // Cleanup
            SystemRepository.Instance.UnregisterSystem<IInteractSystem>();
        }

        [Test]
        public void HasSystem_ReturnsCorrectly()
        {
            // Arrange: nothing registered yet
            Assert.That(SystemRepository.Instance.HasSystem<IInteractSystem>(), Is.False);

            // Act
            var interact = new InteractSystem();
            SystemRepository.Instance.RegisterSystem<IInteractSystem>(interact);

            // Assert
            Assert.That(SystemRepository.Instance.HasSystem<IInteractSystem>(), Is.True);

            // Cleanup
            SystemRepository.Instance.UnregisterSystem<IInteractSystem>();
            Assert.That(SystemRepository.Instance.HasSystem<IInteractSystem>(), Is.False);
        }
    }
}
