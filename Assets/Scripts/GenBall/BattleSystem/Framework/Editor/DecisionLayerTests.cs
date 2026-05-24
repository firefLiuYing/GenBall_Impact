using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.Controller;
using GenBall.Framework.Entity;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    // ================================================================
    // MOCK INPUT / GROUND DETECT (for DecisionLayer tests)
    // ================================================================

    public class MockPlayerInput : IPlayerInputProvider
    {
        public Vector3 MoveDirection { get; set; }
        public Vector2 ViewDelta { get; set; }
        public bool JumpPressed { get; set; }
        public bool DashPressed { get; set; }
        public bool FirePressed { get; set; }
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
                Object.DestroyImmediate(_gameObject);
        }

        // ================================================================
        // MOVE COMMAND
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
        // ROTATE COMMAND
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
        // JUMP COMMAND
        // ================================================================

        [Test]
        public void JumpCommand_Issued_WhenOnGround_AndJumpPressed()
        {
            _groundDetect.IsOnGround = true;
            _input.JumpPressed = true;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_jump.CallCount, Is.EqualTo(1));
            Assert.That(_jump.LastCommand.Velocity.y, Is.EqualTo(8f));
        }

        [Test]
        public void JumpCommand_NotIssued_WhenNotOnGround()
        {
            _groundDetect.IsOnGround = false;
            _input.JumpPressed = true;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_jump.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void JumpCommand_NotIssued_WhenJumpNotPressed()
        {
            _groundDetect.IsOnGround = true;
            _input.JumpPressed = false;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_jump.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void JumpCommand_Issued_WhenNoGroundDetectRegistered_DefaultsToTrue()
        {
            Object.DestroyImmediate(_gameObject);
            _gameObject = new GameObject("TestEntityNoGround");
            _entity = _gameObject.AddComponent<BattleEntity>();
            _entity.RegisterComponent(_dispatcher);

            var decision = new PlayerDecisionLayer(_entity, _input);
            decision.Dispatcher = _dispatcher;
            _input.JumpPressed = true;

            decision.MakeDecision(0.016f);

            // No ICharacterGroundDetect registered → defaults to true (safe default)
            Assert.That(_jump.CallCount, Is.EqualTo(1));
        }

        // ================================================================
        // DASH COMMAND
        // ================================================================

        [Test]
        public void DashCommand_Issued_WhenCooldownReady()
        {
            _input.DashPressed = true;
            _input.MoveDirection = Vector3.forward;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
            Assert.That(_dash.LastCommand.Direction, Is.EqualTo(Vector3.forward));
            Assert.That(_dash.LastCommand.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void DashCommand_Issued_WhenNoMoveInput_UsesDefaultDirection()
        {
            _input.DashPressed = true;
            _input.MoveDirection = Vector3.zero;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
            Assert.That(_dash.LastCommand.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void DashCommand_NotIssued_DuringCooldown()
        {
            // First dash
            _input.DashPressed = true;
            _input.MoveDirection = Vector3.forward;
            _playerDecision.MakeDecision(0.016f);
            Assert.That(_dash.CallCount, Is.EqualTo(1));

            // Next frame — cooldown still active
            _input.DashPressed = false;
            _playerDecision.MakeDecision(0.016f); // tick cooldown

            // Press dash again — blocked by cooldown
            _input.DashPressed = true;
            _playerDecision.MakeDecision(0.016f);

            Assert.That(_dash.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void DashCooldown_Resets_AfterExpiring()
        {
            // First dash
            _input.DashPressed = true;
            _input.MoveDirection = Vector3.forward;
            _playerDecision.MakeDecision(0.016f);
            Assert.That(_dash.CallCount, Is.EqualTo(1));

            // Advance time past 0.5s cooldown
            _input.DashPressed = false;
            _playerDecision.MakeDecision(0.6f); // 0.6s > 0.5s cooldown

            // Should be able to dash again
            _input.DashPressed = true;
            _playerDecision.MakeDecision(0.016f);

            Assert.That(_dash.CallCount, Is.EqualTo(2));
        }

        [Test]
        public void DashCommand_NotIssued_WhenDashNotPressed()
        {
            _input.DashPressed = false;
            _input.MoveDirection = Vector3.forward;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_dash.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void DashCooldown_TicksDown_EachFrame()
        {
            // Issue dash to start cooldown
            _input.DashPressed = true;
            _input.MoveDirection = Vector3.forward;
            _playerDecision.MakeDecision(0.016f);

            // Tick the cooldown down to a known state
            _input.DashPressed = false;
            _playerDecision.MakeDecision(0.1f);
            _playerDecision.MakeDecision(0.1f);
            _playerDecision.MakeDecision(0.1f);
            _playerDecision.MakeDecision(0.1f);
            // Total ticked: 0.1 * 4 + the 0.016 from dash frame = 0.416
            // Remaining: ~0.1, still > 0

            _input.DashPressed = true;
            _playerDecision.MakeDecision(0.016f);
            // Should still be on cooldown (0.1s remaining)
            Assert.That(_dash.CallCount, Is.EqualTo(1));

            // Tick one more time past cooldown
            _input.DashPressed = false;
            _playerDecision.MakeDecision(0.2f);

            _input.DashPressed = true;
            _playerDecision.MakeDecision(0.016f);
            Assert.That(_dash.CallCount, Is.EqualTo(2));
        }

        // ================================================================
        // ATTACK COMMAND
        // ================================================================

        [Test]
        public void AttackCommand_Issued_WhenFirePressed()
        {
            _input.FirePressed = true;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_attack.CallCount, Is.EqualTo(1));
            Assert.That(_attack.LastCommand.AttackId, Is.EqualTo(0));
        }

        [Test]
        public void AttackCommand_NotIssued_WhenFireNotPressed()
        {
            _input.FirePressed = false;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_attack.CallCount, Is.EqualTo(0));
        }

        // ================================================================
        // MULTIPLE COMMANDS
        // ================================================================

        [Test]
        public void MultipleCommands_InSameFrame()
        {
            _input.MoveDirection = new Vector3(1, 0, 0);
            _input.ViewDelta = new Vector2(0.5f, -0.3f);
            _input.JumpPressed = true;
            _input.DashPressed = true;
            _input.FirePressed = true;
            _groundDetect.IsOnGround = true;

            _playerDecision.MakeDecision(0.016f);

            Assert.That(_move.CallCount, Is.EqualTo(1));
            Assert.That(_rotate.CallCount, Is.EqualTo(1));
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
        // PAUSE
        // ================================================================

        [Test]
        public void MakeDecision_DoesNotThrow_DuringPause()
        {
            _pauseSystem.IsPaused = true;
            _input.MoveDirection = new Vector3(1, 0, 0);
            _input.FirePressed = true;
            _input.JumpPressed = true;
            _input.DashPressed = true;
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
    // ENEMY DECISION LAYER TESTS
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
                Object.DestroyImmediate(_gameObject);
        }

        // ================================================================
        // CONSTRUCTION
        // ================================================================

        [Test]
        public void Constructor_WithNullConfig_GracefulDegradation()
        {
            var enemy = new EnemyDecisionLayer(_entity, null);

            // Must not throw when making decision with null config.
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
            var child = new GameObject("DetectController");
            child.transform.SetParent(_gameObject.transform);
            var detect = child.AddComponent<EnemyDetectController>();

            var enemy = new EnemyDecisionLayer(_entity, null);

            Assert.That(enemy.Detect, Is.Not.Null);
            Assert.That(enemy.Detect, Is.SameAs(detect));
        }

        [Test]
        public void AttackController_FoundFromEntity()
        {
            var child = new GameObject("AttackController");
            child.transform.SetParent(_gameObject.transform);
            var attackCtrl = child.AddComponent<EnemyAttackController>();

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
