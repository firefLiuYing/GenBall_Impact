using System.Reflection;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using GenBall.Player.Controller;
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
        public bool IsPaused { get; set; }
        public void Init() { }
        public void UnInit() { }
        public void SetPause(bool paused) { IsPaused = paused; }
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
        private PlayerMover _playerMover;
        private InputHandler _inputHandler;
        private MockCharacterGroundDetect _groundDetect;
        private AppSettingsConfig _config;
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
            _playerMover = _gameObject.AddComponent<PlayerMover>();

            var inputGo = new GameObject("InputHandler");
            inputGo.transform.SetParent(_gameObject.transform);
            _inputHandler = inputGo.AddComponent<InputHandler>();

            _groundDetect = new MockCharacterGroundDetect();

            _config = ScriptableObject.CreateInstance<AppSettingsConfig>();
            _config.longPressMaxTime = 1.0f;
            _config.longPressJumpMaxHeight = 4.0f;
            _config.shortPressJumpHeight = 3.0f;
            _config.shortPressJustifyTime = 0.25f;

            _executor = new PlayerJumpExecutor(_rigidbody, _playerMover, _config, _inputHandler, _groundDetect);
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

            // _initialVelocity = 2 * longPressJumpMaxHeight / longPressMaxTime = 2 * 4 / 1 = 8
            Assert.That(_rigidbody.velocity.y, Is.EqualTo(8f).Within(0.001f));
        }

        [Test]
        public void Jump_LocksPlayerMoverVertical()
        {
            Assert.That(_playerMover.LockVertical, Is.False);

            _executor.Jump(new JumpCommand(Vector3.up * 8f));

            Assert.That(_playerMover.LockVertical, Is.True);
        }

        [Test]
        public void LogicUpdate_AppliesPressedAcceleration_WhenHolding()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            float velocityBefore = _rigidbody.velocity.y;

            // Simulate jump button still held within max time
            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 0.3f); // < _longPressMaxTime (1.0f)

            _executor.LogicUpdate(0.016f);

            // Velocity should have changed due to acceleration
            Assert.That(_rigidbody.velocity.y, Is.Not.EqualTo(velocityBefore));
        }

        [Test]
        public void LogicUpdate_StopsJumping_WhenHoldTimeExceedsMax()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            Assert.That(_executor.IsJumping, Is.True);

            // Simulate hold time beyond max, but button still held
            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 999f); // > _longPressMaxTime

            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
        }

        [Test]
        public void LogicUpdate_StopsJumping_WhenLanded()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));
            Assert.That(_executor.IsJumping, Is.True);

            // Keep jump in valid hold range so the press condition doesn't stop it
            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 0.3f);

            _groundDetect.IsOnGround = true;

            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
        }

        [Test]
        public void OnComplete_ReleasesLock_AndZerosVerticalVelocity()
        {
            _executor.Jump(new JumpCommand(Vector3.up * 8f));

            // Simulate the jump has completed by setting IsJumping to false,
            // then call LogicUpdate to trigger the cleanup code.
            // Use the "hold time exceeded" path to trigger completion.
            SetPrivateProperty(_inputHandler, "IsJumpPressed", true);
            SetPrivateProperty(_inputHandler, "JumpHoldTime", 999f);
            _executor.LogicUpdate(0.016f);

            Assert.That(_executor.IsJumping, Is.False);
            Assert.That(_playerMover.LockVertical, Is.False);
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

            // Prefer backing field for auto-properties (works regardless of setter accessibility)
            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField != null)
            {
                backingField.SetValue(target, value);
                return;
            }

            // Try property setter (may fail for private setters depending on runtime)
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                try { prop.SetValue(target, value); return; }
                catch { }
            }

            // Last fallback: try field with exact name
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
        private PlayerMover _playerMover;
        private AppSettingsConfig _config;
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
            _playerMover = _gameObject.AddComponent<PlayerMover>();

            _config = ScriptableObject.CreateInstance<AppSettingsConfig>();
            _config.invincibleTime = InvincibleTime;
            _config.endingTime = EndingTime;
            _config.dashSpeed = DashSpeed;

            _executor = new PlayerDashExecutor(_rigidbody, _playerMover, _config);
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
        public void Dash_LocksPlayerMoverHorizontalAndVertical()
        {
            Assert.That(_playerMover.LockHorizontal, Is.False);
            Assert.That(_playerMover.LockVertical, Is.False);

            _executor.Dash(new DashCommand(Vector3.forward, DashSpeed));

            Assert.That(_playerMover.LockHorizontal, Is.True);
            Assert.That(_playerMover.LockVertical, Is.True);
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
            Assert.That(_playerMover.LockHorizontal, Is.False);
            Assert.That(_playerMover.LockVertical, Is.False);
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

    [TestFixture]
    public class PlayerAttackExecutorTests
    {
        private GameObject _gameObject;
        private WeaponController _weaponController;
        private PlayerAttackExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestAttack");
            _weaponController = _gameObject.AddComponent<WeaponController>();

            _executor = new PlayerAttackExecutor(_weaponController);
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void Attack_CallsWeaponFire()
        {
            // WeaponController.Fire calls _currentWeapon?.Trigger(state).
            // Since _currentWeapon is null (Initialize is never called in this test),
            // Fire is a no-op. This test verifies the code path executes without error.
            Assert.DoesNotThrow(() => _executor.Attack(new AttackCommand(0)));
        }

        [Test]
        public void IsAttacking_ReturnsFalse()
        {
            // Attack is fire-and-forget; IsAttacking always returns false.
            Assert.That(_executor.IsAttacking, Is.False);

            _executor.Attack(new AttackCommand(0));

            Assert.That(_executor.IsAttacking, Is.False);
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
        public void JumpPressed_UsesConsumeBufferedJump()
        {
            // ConsumeBufferedJump returns true when Time.time - _lastJumpTime <= jumpBufferedTime.
            // _lastJumpTime is initialized to -100f, so it normally returns false.
            Assert.That(_adapter.JumpPressed, Is.False);

            // Set _lastJumpTime to current time so ConsumeBufferedJump returns true
            var field = typeof(InputHandler).GetField("_lastJumpTime",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "Field _lastJumpTime not found on InputHandler");
            field.SetValue(_inputHandler, Time.time);

            Assert.That(_adapter.JumpPressed, Is.True);
        }

        [Test]
        public void DashPressed_MapsFromInputHandler()
        {
            SetPrivateProperty(_inputHandler, "IsDashPressed", true);

            Assert.That(_adapter.DashPressed, Is.True);

            SetPrivateProperty(_inputHandler, "IsDashPressed", false);

            Assert.That(_adapter.DashPressed, Is.False);
        }

        [Test]
        public void FirePressed_MapsFromInputHandler()
        {
            // IsFirePressed has a public setter (declared as { get; set; })
            _inputHandler.IsFirePressed = true;

            Assert.That(_adapter.FirePressed, Is.True);

            _inputHandler.IsFirePressed = false;

            Assert.That(_adapter.FirePressed, Is.False);
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
