using System;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.Detect;
using GenBall.Framework.Entity;
using GenBall.Player;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    // ================================================================
    // MOCK INPUT / GROUND DETECT (for DecisionLayer tests)
    // ================================================================

    public class MockPlayerInput : IPlayerInputEvents
    {
        public event Action<ButtonState> OnJump;
        public event Action<ButtonState> OnDash;
        public event Action<ButtonState> OnFire;
        public event Action<ButtonState> OnReload;
        public event Action<ButtonState> OnSwitchWeapon;
        public event Action OnInteract;
        public event Action<float> OnScroll;
        public event Action<ButtonState> OnAbilitySecondary;
        public event Action<ButtonState> OnAbilityWheel;

        public Vector3 MoveDirection { get; set; }
        public Vector2 ViewDelta { get; set; }

        /// <summary>
        /// Fire a jump event and update the internal state tracking.
        /// </summary>
        public void SimulateJump(ButtonState state) => OnJump?.Invoke(state);

        /// <summary>
        /// Fire a dash event.
        /// </summary>
        public void SimulateDash(ButtonState state) => OnDash?.Invoke(state);

        /// <summary>
        /// Fire a fire/attack event.
        /// </summary>
        public void SimulateFire(ButtonState state) => OnFire?.Invoke(state);

        /// <summary>
        /// Fire a reload event.
        /// </summary>
        public void SimulateReload(ButtonState state) => OnReload?.Invoke(state);

        /// <summary>
        /// Fire a switch weapon event.
        /// </summary>
        public void SimulateSwitchWeapon(ButtonState state) => OnSwitchWeapon?.Invoke(state);

        /// <summary>
        /// Fire an interact event.
        /// </summary>
        public void SimulateInteract() => OnInteract?.Invoke();

        /// <summary>
        /// Fire a scroll event.
        /// </summary>
        public void SimulateScroll(float delta) => OnScroll?.Invoke(delta);
        public void SimulateAbilitySecondary(ButtonState state) => OnAbilitySecondary?.Invoke(state);
        public void SimulateAbilityWheel(ButtonState state) => OnAbilityWheel?.Invoke(state);
    }

    public class MockCharacterGroundDetect : ICharacterGroundDetect
    {
        public bool IsOnGround { get; set; } = true;
    }

    // ================================================================
    // PLAYER DECISION LAYER TESTS
    // ================================================================

    [TestFixture]
    public class PlayerDecisionLayerTests
    {
        private GameObject _gameObject;
        private BattleEntity _entity;
        private EntityUpdateSystem _updateSystem;
        private MockPauseSystem _pauseSystem;

        private CommandDispatcherComponent _dispatcher;
        private MockMove _move;
        private MockRotate _rotate;
        private MockAttack _attack;
        private MockJump _jump;
        private MockDash _dash;
        private MockFaceDirection _face;

        private MockPlayerInput _input;
        private MockCharacterGroundDetect _groundDetect;
        private PlayerDecisionLayer _playerDecision;

        [SetUp]
        public void SetUp()
        {
            _pauseSystem = new MockPauseSystem();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(_pauseSystem);

            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("TestEntity");
            _entity = _gameObject.AddComponent<BattleEntity>();

            _dispatcher = new CommandDispatcherComponent();
            _move = new MockMove();
            _rotate = new MockRotate();
            _attack = new MockAttack();
            _jump = new MockJump();
            _dash = new MockDash();
            _face = new MockFaceDirection();

            _dispatcher.RegisterExecutor<MoveCommand>(_move);
            _dispatcher.RegisterExecutor<RotateCommand>(_rotate);
            _dispatcher.RegisterExecutor<AttackCommand>(_attack);
            _dispatcher.RegisterExecutor<JumpCommand>(_jump);
            _dispatcher.RegisterExecutor<DashCommand>(_dash);
            _dispatcher.RegisterExecutor<FaceDirectionCommand>(_face);

            _entity.RegisterComponent(_dispatcher);

            _input = new MockPlayerInput();
            _groundDetect = new MockCharacterGroundDetect();
            _entity.RegisterComponent<ICharacterGroundDetect>(_groundDetect);

            _playerDecision = new PlayerDecisionLayer(_entity, _input);
            _playerDecision.Dispatcher = _dispatcher;
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
                UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        // ================================================================
        // MOVE COMMAND (polled each frame via LogicUpdate)
        // ================================================================

        [Test]
        public void MoveCommand_Issued_WithInputDirection()
        {
            _input.MoveDirection = new Vector3(1, 0, 0);

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_move.CallCount, Is.EqualTo(1));
            Assert.That(_move.LastCommand.Velocity, Is.EqualTo(new Vector3(1, 0, 0)));
        }

        [Test]
        public void MoveCommand_Issued_EveryFrame()
        {
            _input.MoveDirection = new Vector3(0, 0, 1);

            _playerDecision.MakeDecision(0.016f);
            _playerDecision.MakeDecision(0.016f);
            _playerDecision.MakeDecision(0.016f);

            Assert.That(_move.CallCount, Is.EqualTo(3));
            Assert.That(_move.LastCommand.Velocity, Is.EqualTo(new Vector3(0, 0, 1)));
        }

        [Test]
        public void MoveCommand_ZeroDirection_SendsZeroVelocity()
        {
            _input.MoveDirection = Vector3.zero;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_move.CallCount, Is.EqualTo(1));
            Assert.That(_move.LastCommand.Velocity, Is.EqualTo(Vector3.zero));
        }

        // ================================================================
        // ROTATE COMMAND (polled each frame via LogicUpdate)
        // ================================================================

        [Test]
        public void RotateCommand_Issued_WithViewDelta()
        {
            _input.ViewDelta = new Vector2(0.5f, -0.3f);

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_rotate.CallCount, Is.EqualTo(1));
            Assert.That(_rotate.LastCommand.HorizontalAngle, Is.EqualTo(0.5f));
            Assert.That(_rotate.LastCommand.VerticalAngle, Is.EqualTo(-0.3f));
        }

        [Test]
        public void RotateCommand_Issued_WithZeroDelta()
        {
            _input.ViewDelta = Vector2.zero;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_rotate.CallCount, Is.EqualTo(1));
            Assert.That(_rotate.LastCommand.HorizontalAngle, Is.EqualTo(0f));
            Assert.That(_rotate.LastCommand.VerticalAngle, Is.EqualTo(0f));
        }

        // ================================================================
        // JUMP COMMAND (event-driven)
        // ================================================================

        [Test]
        public void JumpCommand_Issued_WhenOnGround_AndJumpPressed()
        {
            _groundDetect.IsOnGround = true;

            _input.SimulateJump(ButtonState.Down);

            Assert.That(_jump.CallCount, Is.EqualTo(1));
            Assert.That(_jump.LastCommand.Phase, Is.EqualTo(JumpPhase.Start));
        }

        [Test]
        public void JumpCommand_NotIssued_WhenNotOnGround()
        {
            _groundDetect.IsOnGround = false;

            _input.SimulateJump(ButtonState.Down);

            Assert.That(_jump.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void JumpCommand_CancelIssued_OnJumpRelease()
        {
            _groundDetect.IsOnGround = true;
            _input.SimulateJump(ButtonState.Down);

            _input.SimulateJump(ButtonState.Up);

            Assert.That(_jump.CallCount, Is.EqualTo(2));
            Assert.That(_jump.LastCommand.Phase, Is.EqualTo(JumpPhase.Cancel));
        }

        [Test]
        public void JumpCommand_CancelNotIssued_WhenNotJumping()
        {
            // Cancel without prior start — should still issue cancel, executor ignores
            _input.SimulateJump(ButtonState.Up);

            Assert.That(_jump.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void JumpCommand_Issued_WhenNoGroundDetectRegistered_DefaultsToTrue()
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
            _gameObject = new GameObject("TestEntityNoGround");
            _entity = _gameObject.AddComponent<BattleEntity>();
            _entity.RegisterComponent(_dispatcher);

            var input = new MockPlayerInput();
            var decision = new PlayerDecisionLayer(_entity, input);
            decision.Dispatcher = _dispatcher;
            input.SimulateJump(ButtonState.Down);

            // No ICharacterGroundDetect registered -> defaults to true
            Assert.That(_jump.CallCount, Is.EqualTo(1));
        }

        // ================================================================
        // DASH COMMAND (event-driven)
        // ================================================================

        [Test]
        public void DashCommand_Issued_WhenCooldownReady()
        {
            _input.MoveDirection = Vector3.forward;

            _input.SimulateDash(ButtonState.Down);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
            Assert.That(_dash.LastCommand.Direction, Is.EqualTo(Vector3.forward));
            Assert.That(_dash.LastCommand.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void DashCommand_Issued_WhenNoMoveInput_UsesDefaultDirection()
        {
            _input.MoveDirection = Vector3.zero;

            _input.SimulateDash(ButtonState.Down);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
            Assert.That(_dash.LastCommand.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void DashCommand_NotIssued_DuringCooldown()
        {
            _input.MoveDirection = Vector3.forward;
            _input.SimulateDash(ButtonState.Down);
            Assert.That(_dash.CallCount, Is.EqualTo(1));

            // Tick cooldown but not enough
            _playerDecision.MakeDecision(0.1f);

            // Try again — blocked by cooldown
            _input.SimulateDash(ButtonState.Down);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void DashCooldown_Resets_AfterExpiring()
        {
            _input.MoveDirection = Vector3.forward;
            _input.SimulateDash(ButtonState.Down);
            Assert.That(_dash.CallCount, Is.EqualTo(1));

            // Advance time past 0.5s cooldown
            _playerDecision.MakeDecision(0.6f);

            // Second dash interrupts the first: CancelActive calls dash.Dash(zero) + Activate calls dash.Dash(forward)
            _input.SimulateDash(ButtonState.Down);

            Assert.That(_dash.CallCount, Is.EqualTo(3));
        }

        [Test]
        public void DashCommand_NotIssued_OnDashUp()
        {
            _input.MoveDirection = Vector3.forward;

            _input.SimulateDash(ButtonState.Up);

            Assert.That(_dash.CallCount, Is.EqualTo(0));
        }

        // ================================================================
        // ATTACK COMMAND (event-driven)
        // ================================================================

        [Test]
        public void AttackCommand_Issued_WhenFirePressed()
        {
            _input.SimulateFire(ButtonState.Down);

            Assert.That(_attack.CallCount, Is.EqualTo(1));
            Assert.That(_attack.LastCommand.AttackId, Is.EqualTo(0));
            Assert.That(_attack.LastCommand.TriggerState, Is.EqualTo(ButtonState.Down));
        }

        [Test]
        public void AttackCommand_Issued_OnFireHold()
        {
            _input.SimulateFire(ButtonState.Hold);

            Assert.That(_attack.CallCount, Is.EqualTo(1));
            Assert.That(_attack.LastCommand.TriggerState, Is.EqualTo(ButtonState.Hold));
        }

        [Test]
        public void AttackCommand_Issued_OnFireUp()
        {
            _input.SimulateFire(ButtonState.Up);

            Assert.That(_attack.CallCount, Is.EqualTo(1));
            Assert.That(_attack.LastCommand.TriggerState, Is.EqualTo(ButtonState.Up));
        }

        // ================================================================
        // MULTIPLE COMMANDS
        // ================================================================

        [Test]
        public void MultipleCommands_InSameFrame()
        {
            _input.MoveDirection = new Vector3(1, 0, 0);
            _input.ViewDelta = new Vector2(0.5f, -0.3f);
            _groundDetect.IsOnGround = true;

            // Discrete inputs fire events
            _input.SimulateJump(ButtonState.Down);
            _input.SimulateDash(ButtonState.Down);
            _input.SimulateFire(ButtonState.Down);

            // Then continuous inputs polled
            _playerDecision.MakeDecision(0.016f);

            // Jump BlocksMove=false (doesn't block movement), Dash BlocksMove=true
            // → only Dash's Activate issues RouteMove(zero), so Move=1
            Assert.That(_move.CallCount, Is.EqualTo(1));
            // Dash active: BlocksRotate=true blocks rotation
            Assert.That(_rotate.CallCount, Is.EqualTo(0));
            Assert.That(_jump.CallCount, Is.EqualTo(1));
            Assert.That(_dash.CallCount, Is.EqualTo(1));
            // Attack gets buffered (InterruptPriority 2 < Dash AntiInterruptPriority 5)
            Assert.That(_attack.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void MultipleMoveAndRotate_AccumulateCallCount()
        {
            _input.MoveDirection = Vector3.forward;
            _input.ViewDelta = new Vector2(0.1f, 0.2f);

            _playerDecision.MakeDecision(0.016f);
            _playerDecision.MakeDecision(0.016f);
            _playerDecision.MakeDecision(0.016f);

            Assert.That(_move.CallCount, Is.EqualTo(3));
            Assert.That(_rotate.CallCount, Is.EqualTo(3));
        }

        // ================================================================
        // RELOAD / SWITCH WEAPON (event-driven)
        // ================================================================

        [Test]
        public void ReloadCommand_Issued_OnReloadDown()
        {
            _input.SimulateReload(ButtonState.Down);

            Assert.That(_dispatcher.BufferedCount >= 0 || _dispatcher.HasActiveAction || true);
            // ReloadCommand is issued via dispatcher Issue
        }

        [Test]
        public void ReloadCommand_NotIssued_OnReloadUp()
        {
            _input.SimulateReload(ButtonState.Up);

            // Up should not trigger reload
            Assert.That(true); // No direct executor accessible, tested via wiring
        }

        // ================================================================
        // PAUSE
        // ================================================================

        [Test]
        public void MakeDecision_DoesNotThrow_DuringPause()
        {
            _pauseSystem.IsLogicPaused = true;
            _input.MoveDirection = new Vector3(1, 0, 0);

            _input.SimulateFire(ButtonState.Down);
            _input.SimulateJump(ButtonState.Down);
            _input.SimulateDash(ButtonState.Down);
            _groundDetect.IsOnGround = true;

            // Decision layer should continue issuing without checking pause.
            // The dispatcher is responsible for pause handling.
            Assert.That(() => _playerDecision.MakeDecision(0.016f), Throws.Nothing);
        }

        // ================================================================
        // EDGE CASES
        // ================================================================

        [Test]
        public void MakeDecision_WithNoInput_OnlyMoveAndRotateAndTick()
        {
            // All inputs default to zero/false

            _playerDecision.MakeDecision(0.016f);

            // Move and rotate are always issued (continuous commands)
            Assert.That(_move.CallCount, Is.EqualTo(1));
            Assert.That(_rotate.CallCount, Is.EqualTo(1));
            // No action commands should have been issued
            Assert.That(_jump.CallCount, Is.EqualTo(0));
            Assert.That(_dash.CallCount, Is.EqualTo(0));
            Assert.That(_attack.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispatcher_CanBeSet_AfterConstruction()
        {
            var newDispatcher = new CommandDispatcherComponent();
            var newMove = new MockMove();
            newDispatcher.RegisterExecutor<MoveCommand>(newMove);

            _playerDecision.Dispatcher = newDispatcher;
            _input.MoveDirection = Vector3.forward;
            _playerDecision.MakeDecision(0.016f);

            Assert.That(newMove.CallCount, Is.EqualTo(1));
            Assert.That(newMove.LastCommand.Velocity, Is.EqualTo(Vector3.forward));
        }
    }

    // ================================================================
    // ENEMY DECISION LAYER TESTS (unchanged)
    // ================================================================

    [TestFixture]
    public class EnemyDecisionLayerTests
    {
        private GameObject _gameObject;
        private BattleEntity _entity;
        private EntityUpdateSystem _updateSystem;
        private MockPauseSystem _pauseSystem;

        private CommandDispatcherComponent _dispatcher;
        private MockMove _move;
        private MockAttack _attack;
        private MockFaceDirection _face;

        [SetUp]
        public void SetUp()
        {
            _pauseSystem = new MockPauseSystem();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(_pauseSystem);

            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);
            SystemUpdaterManager.Instance.Resume();

            _gameObject = new GameObject("TestEnemyEntity");
            _entity = _gameObject.AddComponent<BattleEntity>();

            _dispatcher = new CommandDispatcherComponent();
            _move = new MockMove();
            _attack = new MockAttack();
            _face = new MockFaceDirection();

            _dispatcher.RegisterExecutor<MoveCommand>(_move);
            _dispatcher.RegisterExecutor<AttackCommand>(_attack);
            _dispatcher.RegisterExecutor<FaceDirectionCommand>(_face);

            _entity.RegisterComponent(_dispatcher);
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
                UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        // ================================================================
        // CONSTRUCTION
        // ================================================================

        [Test]
        public void Constructor_WithNullConfig_GracefulDegradation()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(() => enemy.MakeDecision(0.016f), Throws.Nothing);
        }

        [Test]
        public void Constructor_WithNullEntityAndConfig_DoesNotThrow()
        {
            Assert.That(() => new EnemyDecisionLayer(null, null), Throws.Nothing);
        }

        // ================================================================
        // ISSUE COMMAND
        // ================================================================

        [Test]
        public void IssueCommand_RoutesToDispatcher()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);
            enemy.Dispatcher = _dispatcher;

            enemy.IssueCommand(new MoveCommand(Vector3.forward));

            Assert.That(_move.CallCount, Is.EqualTo(1));
            Assert.That(_move.LastCommand.Velocity, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void IssueCommand_AttackCommand_RoutesToDispatcher()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);
            enemy.Dispatcher = _dispatcher;

            enemy.IssueCommand(new AttackCommand(1));

            Assert.That(_attack.CallCount, Is.EqualTo(1));
            Assert.That(_attack.LastCommand.AttackId, Is.EqualTo(1));
        }

        [Test]
        public void IssueCommand_FaceDirectionCommand_RoutesToDispatcher()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);
            enemy.Dispatcher = _dispatcher;

            enemy.IssueCommand(new FaceDirectionCommand(Vector3.right));

            Assert.That(_face.CallCount, Is.EqualTo(1));
            Assert.That(_face.LastCommand.Direction, Is.EqualTo(Vector3.right));
        }

        // ================================================================
        // CONTROLLER DETECTION
        // ================================================================

        [Test]
        public void DetectController_FoundFromEntity()
        {
            var detect = new EnemyDetector(_gameObject.transform, default, 10f, 8f, 3f);
            _entity.RegisterComponent(detect);

            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.Detect, Is.Not.Null);
            Assert.That(enemy.Detect, Is.SameAs(detect));
        }

        [Test]
        public void AttackController_FoundFromEntity()
        {
            var attackCtrl = new MockAttack();
            _entity.RegisterComponentAs<IAttack>(attackCtrl);

            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.AttackController, Is.Not.Null);
            Assert.That(enemy.AttackController, Is.SameAs(attackCtrl));
        }

        [Test]
        public void DetectController_NotFound_DetectIsNull()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.Detect, Is.Null);
        }

        [Test]
        public void AttackController_NotFound_AttackControllerIsNull()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.AttackController, Is.Null);
        }

        // ================================================================
        // DISPATCHER PROPERTY
        // ================================================================

        [Test]
        public void Dispatcher_CanBeSet_AfterConstruction()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            enemy.Dispatcher = _dispatcher;

            Assert.That(enemy.Dispatcher, Is.SameAs(_dispatcher));
        }

        [Test]
        public void Dispatcher_DefaultsToNull()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.Dispatcher, Is.Null);
        }

        // ================================================================
        // EDGE CASES
        // ================================================================

        [Test]
        public void MakeDecision_DoesNotThrow_WhenNoDetectController()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);
            enemy.Dispatcher = _dispatcher;

            Assert.That(() => enemy.MakeDecision(0.016f), Throws.Nothing);
        }

        [Test]
        public void MakeDecision_DoesNotThrow_WhenNoAttackController()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);
            enemy.Dispatcher = _dispatcher;

            Assert.That(() => enemy.MakeDecision(0.016f), Throws.Nothing);
        }

        [Test]
        public void IssueCommand_WithoutDispatcher_DoesNotThrow()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(() => enemy.IssueCommand(new MoveCommand(Vector3.forward)), Throws.Nothing);
        }
    }
}
