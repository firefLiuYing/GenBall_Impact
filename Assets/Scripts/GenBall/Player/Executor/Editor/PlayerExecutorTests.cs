using System.Reflection;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Entity;
using GenBall.Player.Input;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Player.Executor.Tests
{
    // ================================================================
    // MOCK / STUB CLASSES
    // ================================================================

    public class MockPauseSystem : IPauseSystem
    {
        public bool IsLogicPaused { get; set; }
        public bool IsPhysicsPaused { get; set; }
        public int StackDepth { get; private set; }
        public event System.Action OnPauseChanged;

        public void Init() { }
        public void UnInit() { }
        public void PushPause(bool pausePhysics)
        {
            StackDepth++;
            IsLogicPaused = true;
            IsPhysicsPaused = pausePhysics;
            OnPauseChanged?.Invoke();
        }
        public void PopPause()
        {
            if (StackDepth > 0) StackDepth--;
            IsLogicPaused = StackDepth > 0;
            OnPauseChanged?.Invoke();
        }
    }

    public class MockCharacterGroundDetect : ICharacterGroundDetect
    {
        public bool IsOnGround { get; set; } = true;
    }

    // ================================================================
    // PLAYER JUMP EXECUTOR TESTS
    // ================================================================

    [TestFixture]
    public class PlayerJumpExecutorTests
    {
        private GameObject _gameObject;
        private Rigidbody _rigidbody;
        private RigidbodyMover _mover;
        private InputHandler _inputHandler;
        private MockCharacterGroundDetect _groundDetect;
        private PlayerConfig _config;
        private PlayerJumpExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            var pauseSystem = new MockPauseSystem();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(pauseSystem);

            var updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("TestJump");
            _rigidbody = _gameObject.AddComponent<Rigidbody>();
            _mover = _gameObject.AddComponent<RigidbodyMover>();

            var inputGo = new GameObject("InputHandler");
            inputGo.transform.SetParent(_gameObject.transform);
            _inputHandler = inputGo.AddComponent<InputHandler>();

            _groundDetect = new MockCharacterGroundDetect();

            _config = ScriptableObject.CreateInstance<PlayerConfig>();
            _config.longPressMaxTime = 1.0f;
            _config.longPressJumpMaxHeight = 4.0f;
            _config.shortPressJumpHeight = 3.0f;
            _config.shortPressJustifyTime = 0.25f;
            _config.speed = 5f;

            _executor = new PlayerJumpExecutor(_rigidbody, _mover, _config, _groundDetect);
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            if (SystemRepository.Instance.HasSystem<IPauseSystem>())
                SystemRepository.Instance.UnregisterSystem<IPauseSystem>();
            SystemUpdaterManager.Instance.Resume();

            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Jump_SetsIsJumpingAndAppliesInitialVelocity()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));

            Assert.That(_executor.IsJumping, Is.True);
            Assert.That(_rigidbody.velocity.y, Is.EqualTo(8f).Within(0.001f));
        }

        [Test]
        public void LogicUpdate_AppliesPressedAcceleration_WhenHolding()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            float velocityBefore = _rigidbody.velocity.y;

            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 0.3f);

            _executor.LogicUpdate(0.016f);

            Assert.That(_rigidbody.velocity.y, Is.Not.EqualTo(velocityBefore));
        }

        [Test]
        public void LogicUpdate_StopsJumping_WhenHoldTimeExceedsMax()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            Assert.That(_executor.IsJumping, Is.True);

            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 999f);

            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
        }

        [Test]
        public void LogicUpdate_StopsJumping_WhenLanded()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            Assert.That(_executor.IsJumping, Is.True);

            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 0.3f);

            _groundDetect.IsOnGround = true;

            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
        }

        [Test]
        public void OnComplete_ZerosVerticalVelocity()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));

            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 999f);
            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
            Assert.That(_rigidbody.velocity.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void IsJumping_InitiallyFalse()
        {
            Assert.That(_executor.IsJumping, Is.False);
        }

        private static void SetPrivateProperty(object target, string name, object value)
        {
            var type = target.GetType();
            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField != null)
            {
                backingField.SetValue(target, value);
                return;
            }
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                try { prop.SetValue(target, value); return; }
                catch { }
            }
            var exactField = type.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            exactField?.SetValue(target, value);
        }
    }

    // ================================================================
    // PLAYER DASH EXECUTOR TESTS
    // ================================================================

    [TestFixture]
    public class PlayerDashExecutorTests
    {
        private GameObject _gameObject;
        private Rigidbody _rigidbody;
        private RigidbodyMover _mover;
        private PlayerConfig _config;
        private PlayerDashExecutor _executor;

        private const float InvincibleTime = 0.15f;
        private const float EndingTime = 0.1f;
        private const float DashSpeed = 10f;

        [SetUp]
        public void SetUp()
        {
            var pauseSystem = new MockPauseSystem();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(pauseSystem);

            var updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("TestDash");
            _rigidbody = _gameObject.AddComponent<Rigidbody>();
            _mover = _gameObject.AddComponent<RigidbodyMover>();

            _config = ScriptableObject.CreateInstance<PlayerConfig>();
            _config.invincibleTime = InvincibleTime;
            _config.endingTime = EndingTime;
            _config.dashSpeed = DashSpeed;

            var entity = _gameObject.AddComponent<BattleEntity>();
            _executor = new PlayerDashExecutor(_rigidbody, _mover, _config, entity);
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            if (SystemRepository.Instance.HasSystem<IPauseSystem>())
                SystemRepository.Instance.UnregisterSystem<IPauseSystem>();
            SystemUpdaterManager.Instance.Resume();

            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Dash_SetsIsDashingAndAppliesVelocity()
        {
            var cmd = new DashCommand(Vector3.forward, DashSpeed);
            _executor.Dash(cmd);

            Assert.That(_executor.IsDashing, Is.True);
            Assert.That(_rigidbody.velocity.z, Is.EqualTo(DashSpeed).Within(0.001f));
            Assert.That(_rigidbody.velocity.x, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Dash_PreservesHorizontalVelocity_DuringDash()
        {
            _executor.Dash(new DashCommand(Vector3.forward, DashSpeed));

            // Dash uses RigidbodyMover which handles pause — velocity set directly
            Assert.That(_rigidbody.velocity.z, Is.EqualTo(DashSpeed).Within(0.001f));
        }

        [Test]
        public void LogicUpdate_MaintainsVelocity_DuringInvincibleTime()
        {
            _executor.Dash(new DashCommand(Vector3.right, DashSpeed));
            float velBefore = _rigidbody.velocity.magnitude;

            // LogicUpdate during invincible phase (elapsed < invincibleTime)
            // Since dash was just started, Time.time - _dashStartTime ≈ 0
            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsDashing, Is.True);
            // Velocity should be maintained (re-set to direction * speed)
            Assert.That(_rigidbody.velocity.magnitude, Is.EqualTo(DashSpeed).Within(0.001f));
        }

        [Test]
        public void LogicUpdate_CompletesDash_AfterEndingTime()
        {
            _executor.Dash(new DashCommand(Vector3.forward, DashSpeed));
            Assert.That(_executor.IsDashing, Is.True);

            // Use reflection to set _dashStartTime into the past,
            // making elapsed > invincibleTime + endingTime
            var dashField = typeof(PlayerDashExecutor).GetField("_dashStartTime",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(dashField, Is.Not.Null, "Field _dashStartTime not found on PlayerDashExecutor");
            dashField.SetValue(_executor, Time.time - InvincibleTime - EndingTime - 0.01f);

            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsDashing, Is.False);
            Assert.That(_rigidbody.velocity.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void IsDashing_InitiallyFalse()
        {
            Assert.That(_executor.IsDashing, Is.False);
        }
    }

    // ================================================================
    // PLAYER ATTACK EXECUTOR TESTS
    // ================================================================

    // ================================================================
    // WEAPON EXECUTOR TESTS
    // ================================================================

    [TestFixture]
    public class WeaponExecutorTests
    {
        private GameObject _gameObject;
        private WeaponExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestWeapon");
            _executor = _gameObject.AddComponent<WeaponExecutor>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Attack_DoesNotThrow_WhenNoWeaponEquipped()
        {
            // _currentWeapon is null (Init never called), Trigger is a no-op.
            Assert.DoesNotThrow(() => _executor.Attack(new AttackCommand(0)));
        }

        [Test]
        public void IsAttacking_AlwaysReturnsFalse()
        {
            Assert.That(_executor.IsAttacking, Is.False);
            _executor.Attack(new AttackCommand(0));
            Assert.That(_executor.IsAttacking, Is.False);
        }

        [Test]
        public void Reload_DoesNotThrow_WhenNoWeaponEquipped()
        {
            Assert.DoesNotThrow(() => _executor.Reload(new ReloadCommand()));
        }

        [Test]
        public void SwitchWeapon_DoesNotThrow_WhenNoWeaponEquipped()
        {
            Assert.DoesNotThrow(() => _executor.SwitchWeapon(new SwitchWeaponCommand()));
        }
    }

    // ================================================================
    // PLAYER INPUT ADAPTER TESTS
    // ================================================================

    [TestFixture]
    public class PlayerInputAdapterTests
    {
        private GameObject _inputGo;
        private InputHandler _inputHandler;
        private PlayerInputAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            _inputGo = new GameObject("TestInputHandler");
            _inputHandler = _inputGo.AddComponent<InputHandler>();
            _adapter = new PlayerInputAdapter(_inputHandler);
        }

        [TearDown]
        public void TearDown()
        {
            if (_inputGo != null)
                Object.DestroyImmediate(_inputGo);
        }

        [Test]
        public void MoveDirection_MapsFromInputHandler()
        {
            SetPrivateProperty(_inputHandler, "MoveDirection", new Vector3(1, 0, 0));

            Assert.That(_adapter.MoveDirection, Is.EqualTo(new Vector3(1, 0, 0)));
        }

        [Test]
        public void ViewDelta_MapsFromInputHandler()
        {
            SetPrivateProperty(_inputHandler, "ViewDelta", new Vector2(0.5f, -0.3f));

            Assert.That(_adapter.ViewDelta, Is.EqualTo(new Vector2(0.5f, -0.3f)));
        }

        [Test]
        public void OnJump_ForwardsFromInputHandler()
        {
            ButtonState? received = null;
            _adapter.OnJump += s => received = s;

            // Simulate jump started on InputHandler
            var onJumpField = typeof(InputHandler).GetField("OnJump",
                BindingFlags.Public | BindingFlags.Instance);
            var onJump = (System.Action<ButtonState>)onJumpField.GetValue(_inputHandler);
            onJump?.Invoke(ButtonState.Down);

            Assert.That(received, Is.EqualTo(ButtonState.Down));
        }

        [Test]
        public void OnDash_ForwardsFromInputHandler()
        {
            ButtonState? received = null;
            _adapter.OnDash += s => received = s;

            var onDashField = typeof(InputHandler).GetField("OnDash",
                BindingFlags.Public | BindingFlags.Instance);
            var onDash = (System.Action<ButtonState>)onDashField.GetValue(_inputHandler);
            onDash?.Invoke(ButtonState.Down);

            Assert.That(received, Is.EqualTo(ButtonState.Down));
        }

        [Test]
        public void OnFire_ForwardsFromInputHandler()
        {
            ButtonState? received = null;
            _adapter.OnFire += s => received = s;

            var onFireField = typeof(InputHandler).GetField("OnFire",
                BindingFlags.Public | BindingFlags.Instance);
            var onFire = (System.Action<ButtonState>)onFireField.GetValue(_inputHandler);
            onFire?.Invoke(ButtonState.Hold);
            
            

            Assert.That(received, Is.EqualTo(ButtonState.Hold));
        }

        private static void SetPrivateProperty(object target, string name, object value)
        {
            var type = target.GetType();

            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField != null)
            {
                backingField.SetValue(target, value);
                return;
            }

            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                try { prop.SetValue(target, value); return; }
                catch { }
            }

            var exactField = type.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            exactField?.SetValue(target, value);
        }
    }
}
