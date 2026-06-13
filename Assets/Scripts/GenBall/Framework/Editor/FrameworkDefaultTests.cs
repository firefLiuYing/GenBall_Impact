using System.Collections.Generic;
using System.Threading.Tasks;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using GenBall.Interact;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using NUnit.Framework;
using Yueyn.Main;

namespace GenBall.Framework.Tests
{
    /// <summary>
    /// Tests verifying the registration order from FrameworkDefault.DoInit().
    /// Cannot instantiate FrameworkDefault directly (it is a MonoBehaviour),
    /// so we simulate the registration logic.
    /// </summary>
    [TestFixture]
    public class FrameworkDefaultTests
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
            public void Init() { }
            public void UnInit() { }
            public void SkipStartupLoading() { }
            public void StartGameWithContext(GameStartContext context) { }
        }

        private class TestSceneExecutorSystem : ISceneExecutorSystem
        {
            public void Init() { }
            public void UnInit() { }
            public void ExecuteSceneSetup(SceneInitContext context) { }
        }

        [SetUp]
        public void SetUp()
        {
            // Ensure clean state: unregister systems that may be pre-registered
            // by startup code or other tests (SystemRepository is a singleton).
            // TearDown handles cleanup after the test.
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
            if (SystemRepository.Instance.HasSystem<ISaveService>())
                SystemRepository.Instance.UnregisterSystem<ISaveService>();
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup any leftover registrations in reverse order
            if (SystemRepository.Instance.HasSystem<ISceneExecutorSystem>())
                SystemRepository.Instance.UnregisterSystem<ISceneExecutorSystem>();
            if (SystemRepository.Instance.HasSystem<IEvolutionSystem>())
                SystemRepository.Instance.UnregisterSystem<IEvolutionSystem>();
            if (SystemRepository.Instance.HasSystem<IBulletSystem>())
                SystemRepository.Instance.UnregisterSystem<IBulletSystem>();
            if (SystemRepository.Instance.HasSystem<IPlayerSystem>())
                SystemRepository.Instance.UnregisterSystem<IPlayerSystem>();
            if (SystemRepository.Instance.HasSystem<ILaunchSystem>())
                SystemRepository.Instance.UnregisterSystem<ILaunchSystem>();
            if (SystemRepository.Instance.HasSystem<IGameManagerSystem>())
                SystemRepository.Instance.UnregisterSystem<IGameManagerSystem>();
            if (SystemRepository.Instance.HasSystem<IPauseSystem>())
                SystemRepository.Instance.UnregisterSystem<IPauseSystem>();
            if (SystemRepository.Instance.HasSystem<ITeleportSystem>())
                SystemRepository.Instance.UnregisterSystem<ITeleportSystem>();
            if (SystemRepository.Instance.HasSystem<ISceneStateSystem>())
                SystemRepository.Instance.UnregisterSystem<ISceneStateSystem>();
            if (SystemRepository.Instance.HasSystem<IInteractSystem>())
                SystemRepository.Instance.UnregisterSystem<IInteractSystem>();
            if (SystemRepository.Instance.HasSystem<IDeathSystem>())
                SystemRepository.Instance.UnregisterSystem<IDeathSystem>();
            if (SystemRepository.Instance.HasSystem<IDamageSystem>())
                SystemRepository.Instance.UnregisterSystem<IDamageSystem>();
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            if (SystemRepository.Instance.HasSystem<IBuffTickSystem>())
                SystemRepository.Instance.UnregisterSystem<IBuffTickSystem>();
            if (SystemRepository.Instance.HasSystem<IBuffRegistry>())
                SystemRepository.Instance.UnregisterSystem<IBuffRegistry>();
            if (SystemRepository.Instance.HasSystem<ISaveService>())
                SystemRepository.Instance.UnregisterSystem<ISaveService>();
            if (SystemRepository.Instance.HasSystem<IConfigProvider>())
                SystemRepository.Instance.UnregisterSystem<IConfigProvider>();
        }

        [Test]
        public void RegistrationOrder_NoExceptions()
        {
            // Arrange: simulate FrameworkDefault.DoInit() registration order
            var config = new FakeConfigProvider();
            var save = new FakeSaveService();
            var buffRegistry = new BuffRegistry();
            var buffTick = new BuffTickSystem();
            var entityUpdate = new EntityUpdateSystem();
            var damageSystem = new DamageSystemDefault();
            var deathSystem = new DeathSystemDefault();
            var interact = new InteractSystem();
            var scene = new SceneSystem();
            var teleport = new TeleportSystem();
            var pause = new PauseManager();
            var gameManager = new GameManager();
            var launch = new TestLaunchSystem();
            var playerSystem = new PlayerSystemDefault();
            var bulletSystem = new BulletSystem();
            var evolutionSystem = new EvolutionSystem();
            var sceneExecutor = new TestSceneExecutorSystem();

            var repo = SystemRepository.Instance;

            // Act: register in FrameworkDefault.DoInit() order
            Assert.That(() => repo.RegisterSystem<IConfigProvider>(config), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISaveService>(save), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IBuffRegistry>(buffRegistry), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IBuffTickSystem>(buffTick), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IEntityUpdateSystem>(entityUpdate), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IDamageSystem>(damageSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IDeathSystem>(deathSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IInteractSystem>(interact), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISceneStateSystem>(scene), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ITeleportSystem>(teleport), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IPauseSystem>(pause), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IGameManagerSystem>(gameManager), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ILaunchSystem>(launch), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IPlayerSystem>(playerSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IBulletSystem>(bulletSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<IEvolutionSystem>(evolutionSystem), Throws.Nothing);
            Assert.That(() => repo.RegisterSystem<ISceneExecutorSystem>(sceneExecutor), Throws.Nothing);

            // Assert: verify all systems are retrievable with correct types
            Assert.That(repo.GetSystem<IConfigProvider>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISaveService>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IBuffRegistry>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IBuffTickSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IEntityUpdateSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IDamageSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IDeathSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IInteractSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISceneStateSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ITeleportSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IPauseSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IGameManagerSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ILaunchSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IPlayerSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IBulletSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<IEvolutionSystem>(), Is.Not.Null);
            Assert.That(repo.GetSystem<ISceneExecutorSystem>(), Is.Not.Null);

            // Cleanup: unregister in reverse order
            repo.UnregisterSystem<ISceneExecutorSystem>();
            repo.UnregisterSystem<IEvolutionSystem>();
            repo.UnregisterSystem<IBulletSystem>();
            repo.UnregisterSystem<IPlayerSystem>();
            repo.UnregisterSystem<ILaunchSystem>();
            repo.UnregisterSystem<IGameManagerSystem>();
            repo.UnregisterSystem<IPauseSystem>();
            repo.UnregisterSystem<ITeleportSystem>();
            repo.UnregisterSystem<ISceneStateSystem>();
            repo.UnregisterSystem<IInteractSystem>();
            repo.UnregisterSystem<IDeathSystem>();
            repo.UnregisterSystem<IDamageSystem>();
            repo.UnregisterSystem<IEntityUpdateSystem>();
            repo.UnregisterSystem<IBuffTickSystem>();
            repo.UnregisterSystem<IBuffRegistry>();
            repo.UnregisterSystem<ISaveService>();
            repo.UnregisterSystem<IConfigProvider>();
        }
    }
}
