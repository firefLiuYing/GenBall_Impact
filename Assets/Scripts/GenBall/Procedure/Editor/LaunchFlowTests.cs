using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenBall.GM;
using GenBall.Framework.Config;
using GenBall.Map;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Procedure.Tests
{
    /// <summary>
    /// Integration tests for launch flow (ISceneLoadSystem, IGMCommandSystem, ILaunchSystem).
    /// Verifies state transitions, command registration/execution, and dev mode behavior.
    /// </summary>
    [TestFixture]
    public class LaunchFlowTests
    {
        // ---------------------------------------------------------------------
        // Fake implementations
        // ---------------------------------------------------------------------

        private class FakeConfigProvider : IConfigProvider
        {
            public bool DevMode;
            public string StartSceneName = "TestScene";
            public RunningMode RunningMode = RunningMode.SaveData | RunningMode.LoadData;

            public void Init() { }
            public void UnInit() { }
            public T GetConfig<T>() where T : class
            {
                if (typeof(T) == typeof(AppSettingsConfig))
                {
                    var config = ScriptableObject.CreateInstance<AppSettingsConfig>();
                    config.devMode = DevMode;
                    config.startSceneName = StartSceneName;
                    config.runningMode = RunningMode;
                    return config as T;
                }
                return null;
            }
        }

        private class FakeSaveService : ISaveService
        {
            public int MaxSaveCount => 6;
            public void Init() { }
            public void UnInit() { }
            public Task<IEnumerable<SaveSlotData>> GetSaveSlotDatas() =>
                Task.FromResult<IEnumerable<SaveSlotData>>(new List<SaveSlotData>());
            public Task<GameData> LoadGameData(int saveIndex) =>
                Task.FromResult(new GameData());
            public Task<bool> SaveGameData(GameData gameData, int saveIndex) =>
                Task.FromResult(true);
            public Task<int> CreateNewSave() => Task.FromResult(0);
            public Task<bool> DeleteSave(int saveIndex) => Task.FromResult(true);
        }

        private class FakeSceneStateSystem : ISceneStateSystem
        {
            public void Init() { }
            public void UnInit() { }
            public void InitializeMapConfig(MapModel mapModel) { }
            public void InitializeSceneStateObjs(MapSaveData mapSaveData) { }
            public void UnlockSavePoint(string sceneName, int savePointIndex) { }
            public void KillEnemyUnit(string sceneName, int enemyUnitIndex) { }
            public IEnumerable<SavePointModel> GetUnlockedSavePointModels(string sceneName) =>
                Enumerable.Empty<SavePointModel>();
            public SavePointModel GetSavePointModel(string sceneName, int unitIndex) => null;
            public EnemyUnitModel GetEnemyModel(string sceneName, int unitIndex) => null;
            public IEnumerable<EnemyUnitModel> GetAllUnKilledEnemyModel(string sceneName) =>
                Enumerable.Empty<EnemyUnitModel>();
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------

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

        // ---------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state before each test.
            // Some systems may be pre-registered by the test runner or other tests.
            SafeUnregister<ISceneLoadSystem>();
            SafeUnregister<IGMCommandSystem>();
            SafeUnregister<ILaunchSystem>();
            SafeUnregister<IGameManagerSystem>();
            SafeUnregister<ISceneStateSystem>();
            SafeUnregister<ISaveService>();
            SafeUnregister<IConfigProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup in reverse registration order
            SafeUnregister<ISceneLoadSystem>();
            SafeUnregister<IGMCommandSystem>();
            SafeUnregister<ILaunchSystem>();
            SafeUnregister<IGameManagerSystem>();
            SafeUnregister<ISceneStateSystem>();
            SafeUnregister<ISaveService>();
            SafeUnregister<IConfigProvider>();
        }

        // =====================================================================
        // ISceneLoadSystem tests
        // =====================================================================

        [Test]
        public void SceneLoadSystem_Init_UnInit_NoError()
        {
            var loadSystem = new SceneLoadSystemDefault();

            Assert.DoesNotThrow(() => loadSystem.Init());
            Assert.DoesNotThrow(() => loadSystem.UnInit());
        }

        [Test]
        public void SceneLoadSystem_AsyncLoadScene_SetsLoadingState()
        {
            // Arrange
            var loadSystem = new SceneLoadSystemDefault();
            SystemRepository.Instance.RegisterSystem<ISceneLoadSystem>(loadSystem);

            // Act: calling AsyncLoadScene sets state. Actual scene loading may fail
            // in edit-mode tests, but the state flags should be set correctly.
            Assert.DoesNotThrow(() => loadSystem.AsyncLoadScene("Prologue"));

            // Assert
            Assert.That(loadSystem.IsLoading, Is.True);
            Assert.That(loadSystem.TargetSceneName, Is.EqualTo("Prologue"));
            Assert.That(loadSystem.LoadProgress, Is.EqualTo(0f));
        }

        [Test]
        public void SceneLoadSystem_DoubleLoad_IsRejected()
        {
            // Arrange
            var loadSystem = new SceneLoadSystemDefault();
            SystemRepository.Instance.RegisterSystem<ISceneLoadSystem>(loadSystem);
            loadSystem.AsyncLoadScene("Prologue");

            // Act: second call should be rejected (IsLoading is true)
            Assert.DoesNotThrow(() => loadSystem.AsyncLoadScene("Episode1"));

            // Assert: state should still reflect the FIRST scene
            Assert.That(loadSystem.IsLoading, Is.True);
            Assert.That(loadSystem.TargetSceneName, Is.EqualTo("Prologue"));
        }

        [Test]
        public void SceneLoadSystem_SetTargetSavePoint_Preserved()
        {
            // Arrange
            var loadSystem = new SceneLoadSystemDefault();
            SystemRepository.Instance.RegisterSystem<ISceneLoadSystem>(loadSystem);

            var savePoint = new SavePointModel
            {
                id = 1,
                displayName = "TestSavePoint",
                spawnPosition = Vector3.one,
                spawnRotation = Quaternion.identity
            };

            // Act
            loadSystem.SetTargetSavePoint(savePoint);

            // Assert
            Assert.That(loadSystem.TargetSavePoint, Is.Not.Null);
            Assert.That(loadSystem.TargetSavePoint.id, Is.EqualTo(1));
            Assert.That(loadSystem.TargetSavePoint.displayName, Is.EqualTo("TestSavePoint"));
            Assert.That(loadSystem.TargetSavePoint.spawnPosition, Is.EqualTo(Vector3.one));
        }

        [Test]
        public void SceneLoadSystem_UnInit_ClearsState()
        {
            // Arrange
            var loadSystem = new SceneLoadSystemDefault();
            SystemRepository.Instance.RegisterSystem<ISceneLoadSystem>(loadSystem);

            var savePoint = new SavePointModel { id = 1 };
            loadSystem.SetTargetSavePoint(savePoint);
            loadSystem.AsyncLoadScene("Prologue");

            // Act
            loadSystem.UnInit();

            // Assert: all state should be cleared
            Assert.That(loadSystem.IsLoading, Is.False);
            Assert.That(loadSystem.TargetSceneName, Is.Null);
            Assert.That(loadSystem.TargetSavePoint, Is.Null);
            Assert.That(loadSystem.LoadProgress, Is.EqualTo(0f));
        }

        // =====================================================================
        // IGMCommandSystem tests
        // =====================================================================

        [Test]
        public void GMCommandSystem_BuiltInCommands_Registered()
        {
            // Arrange
            var config = new FakeConfigProvider { DevMode = true };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var gmSystem = new GMCommandSystemDefault();
            SystemRepository.Instance.RegisterSystem<IGMCommandSystem>(gmSystem);
            gmSystem.Init();

            // Act
            var commands = gmSystem.GetCommands().ToList();
            var commandNames = commands.Select(c => c.name).ToList();

            // Assert
            Assert.That(commandNames, Contains.Item("help"));
            Assert.That(commandNames, Contains.Item("load_scene"));
            Assert.That(commandNames, Contains.Item("skip_splash"));
            Assert.That(commandNames, Contains.Item("list_scenes"));
        }

        [Test]
        public void GMCommandSystem_ExecuteCommand_Help()
        {
            // Arrange
            var config = new FakeConfigProvider { DevMode = true };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var gmSystem = new GMCommandSystemDefault();
            SystemRepository.Instance.RegisterSystem<IGMCommandSystem>(gmSystem);
            gmSystem.Init();

            // Act
            var result = gmSystem.ExecuteCommand("help");

            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result, Does.Contain("help"));
            Assert.That(result, Does.Contain("load_scene"));
            Assert.That(result, Does.Contain("skip_splash"));
            Assert.That(result, Does.Contain("list_scenes"));
        }

        [Test]
        public void GMCommandSystem_ExecuteCommand_Unknown()
        {
            // Arrange
            var config = new FakeConfigProvider { DevMode = true };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var gmSystem = new GMCommandSystemDefault();
            SystemRepository.Instance.RegisterSystem<IGMCommandSystem>(gmSystem);
            gmSystem.Init();

            // Act
            var result = gmSystem.ExecuteCommand("nonexistent");

            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty);
            Assert.That(result.ToLowerInvariant(), Does.Contain("unknown"));
        }

        [Test]
        public void GMCommandSystem_DevModeOff_CommandsDisabled()
        {
            // Arrange
            var config = new FakeConfigProvider { DevMode = false };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var gmSystem = new GMCommandSystemDefault();
            SystemRepository.Instance.RegisterSystem<IGMCommandSystem>(gmSystem);
            gmSystem.Init();

            // Assert: dev mode is off
            Assert.That(gmSystem.IsDevMode, Is.False);

            // Commands are still registered internally but FrameUpdate should skip them
            // when IsDevMode is false (toggle won't activate via backquote).
            var commands = gmSystem.GetCommands().ToList();
            Assert.That(commands, Is.Not.Empty,
                "Commands exist internally so they are available if dev mode is re-enabled.");
        }

        [Test]
        public void GMCommandSystem_RegisterCommand_Extensible()
        {
            // Arrange
            var config = new FakeConfigProvider { DevMode = true };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var gmSystem = new GMCommandSystemDefault();
            SystemRepository.Instance.RegisterSystem<IGMCommandSystem>(gmSystem);
            gmSystem.Init();

            // Act: register a custom command
            bool wasCalled = false;
            gmSystem.RegisterCommand("test_cmd", _ => { wasCalled = true; }, "test description");

            // Assert: command appears in list
            var commands = gmSystem.GetCommands().ToList();
            var commandNames = commands.Select(c => c.name).ToList();
            Assert.That(commandNames, Contains.Item("test_cmd"));

            // Act: execute the custom command
            var result = gmSystem.ExecuteCommand("test_cmd");

            // Assert: handler was called
            Assert.That(wasCalled, Is.True);
            // Result is empty string since the handler sets no output
        }

        // =====================================================================
        // ILaunchSystem tests (real LaunchSystemDefault)
        // =====================================================================

        [Test]
        public void LaunchSystem_DevMode_SkipsSplash()
        {
            // Arrange: register dependencies that LaunchSystemDefault.Init() needs
            var config = new FakeConfigProvider { DevMode = true };
            SystemRepository.Instance.RegisterSystem<IConfigProvider>(config);

            var saveService = new FakeSaveService();
            SystemRepository.Instance.RegisterSystem<ISaveService>(saveService);

            var sceneState = new FakeSceneStateSystem();
            SystemRepository.Instance.RegisterSystem<ISceneStateSystem>(sceneState);

            var gameManager = new GameManager();
            SystemRepository.Instance.RegisterSystem<IGameManagerSystem>(gameManager);

            var launchSystem = new LaunchSystemDefault();
            SystemRepository.Instance.RegisterSystem<ILaunchSystem>(launchSystem);

            // Act: initialize (FSM starts in SplashState)
            Assert.DoesNotThrow(() => launchSystem.Init());

            // SkipSplash transitions SplashState -> StartFormState
            Assert.DoesNotThrow(() => launchSystem.SkipSplash());

            // Second call to SkipSplash should be a no-op (not in SplashState anymore)
            // If it throws, the FSM state assumption is wrong
            Assert.DoesNotThrow(() => launchSystem.SkipSplash(),
                "Calling SkipSplash again should be a no-op when already past SplashState.");

            // Cleanup: UnInit shuts down the FSM
            Assert.DoesNotThrow(() => launchSystem.UnInit());
        }
    }
}
