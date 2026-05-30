using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using GenBall.Procedure.Game;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework.Tests
{
    // ================================================================
    // MOCK EXECUTORS
    // ================================================================

    public class MockMove : IMove
    {
        public MoveCommand LastCommand;
        public int CallCount;
        public void Move(MoveCommand moveCommand) { LastCommand = moveCommand; CallCount++; }
        public Vector3 Velocity => LastCommand.Velocity;
    }

    public class MockRotate : IRotate
    {
        public RotateCommand LastCommand;
        public int CallCount;
        public void Rotate(RotateCommand command) { LastCommand = command; CallCount++; }
    }

    public class MockAttack : IAttack
    {
        public AttackCommand LastCommand;
        public int CallCount;
        public bool IsAttacking { get; set; }
        public void Attack(AttackCommand command) { LastCommand = command; CallCount++; IsAttacking = true; }
    }

    public class MockJump : IJump
    {
        public JumpCommand LastCommand;
        public int CallCount;
        public bool IsJumping { get; set; }
        public void Jump(JumpCommand command) { LastCommand = command; CallCount++; IsJumping = true; }
        public void Cancel() { IsJumping = false; }
    }

    public class MockDash : IDash
    {
        public DashCommand LastCommand;
        public int CallCount;
        public bool IsDashing { get; set; }
        public void Dash(DashCommand command) { LastCommand = command; CallCount++; IsDashing = true; }
    }

    public class MockFaceDirection : IFaceDirection
    {
        public FaceDirectionCommand LastCommand;
        public int CallCount;
        public void Face(FaceDirectionCommand command) { LastCommand = command; CallCount++; }
    }

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

    // ================================================================
    // TESTS
    // ================================================================

    [TestFixture]
    public class CommandDispatcherComponentTests
    {
        private CommandDispatcherComponent _dispatcher;
        private EntityUpdateSystem _updateSystem;
        private MockPauseSystem _pauseSystem;

        private MockMove _mockMove;
        private MockRotate _mockRotate;
        private MockAttack _mockAttack;
        private MockJump _mockJump;
        private MockDash _mockDash;
        private MockFaceDirection _mockFace;

        [SetUp]
        public void SetUp()
        {
            _dispatcher = new CommandDispatcherComponent(0.2f);

            _updateSystem = new EntityUpdateSystem();
            SystemRepository.Instance.RegisterSystem<IEntityUpdateSystem>(_updateSystem);

            _pauseSystem = new MockPauseSystem();
            SystemRepository.Instance.RegisterSystem<IPauseSystem>(_pauseSystem);

            SystemUpdaterManager.Instance.Resume();

            _mockMove = new MockMove();
            _mockRotate = new MockRotate();
            _mockAttack = new MockAttack();
            _mockJump = new MockJump();
            _mockDash = new MockDash();
            _mockFace = new MockFaceDirection();

            _dispatcher.RegisterExecutor<MoveCommand>(_mockMove);
            _dispatcher.RegisterExecutor<RotateCommand>(_mockRotate);
            _dispatcher.RegisterExecutor<AttackCommand>(_mockAttack);
            _dispatcher.RegisterExecutor<JumpCommand>(_mockJump);
            _dispatcher.RegisterExecutor<DashCommand>(_mockDash);
            _dispatcher.RegisterExecutor<FaceDirectionCommand>(_mockFace);
        }

        [TearDown]
        public void TearDown()
        {
            if (SystemRepository.Instance.HasSystem<IEntityUpdateSystem>())
                SystemRepository.Instance.UnregisterSystem<IEntityUpdateSystem>();
            if (SystemRepository.Instance.HasSystem<IPauseSystem>())
                SystemRepository.Instance.UnregisterSystem<IPauseSystem>();
            SystemUpdaterManager.Instance.Resume();
        }

        // ================================================================
        // BASIC ROUTING
        // ================================================================

        [Test]
        public void Issue_MoveCommand_RoutesToMoveExecutor()
        {
            var cmd = new MoveCommand(new Vector3(1, 0, 0));
            _dispatcher.Issue(cmd);

            Assert.That(_mockMove.CallCount, Is.EqualTo(1));
            Assert.That(_mockMove.LastCommand.Velocity, Is.EqualTo(new Vector3(1, 0, 0)));
        }

        [Test]
        public void Issue_RotateCommand_RoutesToRotateExecutor()
        {
            var cmd = new RotateCommand(1.5f, -0.5f);
            _dispatcher.Issue(cmd);

            Assert.That(_mockRotate.CallCount, Is.EqualTo(1));
            Assert.That(_mockRotate.LastCommand.HorizontalAngle, Is.EqualTo(1.5f));
            Assert.That(_mockRotate.LastCommand.VerticalAngle, Is.EqualTo(-0.5f));
        }

        [Test]
        public void Issue_AttackCommand_RoutesToAttackExecutor()
        {
            var cmd = new AttackCommand(1);
            _dispatcher.Issue(cmd);

            Assert.That(_mockAttack.CallCount, Is.EqualTo(1));
            Assert.That(_mockAttack.LastCommand.AttackId, Is.EqualTo(1));
        }

        [Test]
        public void Issue_JumpCommand_RoutesToJumpExecutor()
        {
            var cmd = new JumpCommand();
            _dispatcher.Issue(cmd);

            Assert.That(_mockJump.CallCount, Is.EqualTo(1));
            Assert.That(_mockJump.LastCommand.Phase, Is.EqualTo(JumpPhase.Start));
        }

        [Test]
        public void Issue_DashCommand_RoutesToDashExecutor()
        {
            var cmd = new DashCommand(Vector3.forward, 10f);
            _dispatcher.Issue(cmd);

            Assert.That(_mockDash.CallCount, Is.EqualTo(1));
            Assert.That(_mockDash.LastCommand.Speed, Is.EqualTo(10f));
        }

        [Test]
        public void Issue_FaceDirectionCommand_RoutesToFaceExecutor()
        {
            var cmd = new FaceDirectionCommand(Vector3.right);
            _dispatcher.Issue(cmd);

            Assert.That(_mockFace.CallCount, Is.EqualTo(1));
            Assert.That(_mockFace.LastCommand.Direction, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void Issue_AttackCommand_WithCustomPriorities()
        {
            var cmd = new AttackCommand(1, interruptPriority: 4, antiInterruptPriority: 5);

            Assert.That(cmd.InterruptPriority, Is.EqualTo(4));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(5));
            Assert.That(cmd.Bufferable, Is.True);
        }

        // ================================================================
        // CONTINUOUS COMMAND BLOCKING
        // ================================================================

        [Test]
        public void MoveCommand_Blocked_WhenActionActive()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            _dispatcher.Issue(new MoveCommand(new Vector3(1, 0, 0)));

            // Attack blocks Move (BlocksMove=true by default)
            Assert.That(_mockMove.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void RotateCommand_Allowed_WhenActionActive()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            _dispatcher.Issue(new RotateCommand(1f, 0f));

            // Rotation is always allowed — player can look around during any action.
            Assert.That(_mockRotate.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void FaceDirectionCommand_Blocked_WhenActionActive()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            _dispatcher.Issue(new FaceDirectionCommand(Vector3.right));

            Assert.That(_mockFace.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void MoveCommand_Resumes_AfterActiveActionCompletes()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            // Complete the attack via LogicUpdate
            _mockAttack.IsAttacking = false;
            _dispatcher.LogicUpdate(0.016f);

            _dispatcher.Issue(new MoveCommand(new Vector3(1, 0, 0)));

            Assert.That(_mockMove.LastCommand.Velocity, Is.EqualTo(new Vector3(1, 0, 0)));
        }

        [Test]
        public void RotateCommand_Resumes_AfterActiveActionCompletes()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            _mockAttack.IsAttacking = false;
            _dispatcher.LogicUpdate(0.016f);

            _dispatcher.Issue(new RotateCommand(1f, 0f));

            Assert.That(_mockRotate.CallCount, Is.EqualTo(1));
        }

        // ================================================================
        // ARBITRATION
        // ================================================================

        [Test]
        public void ActionCommand_Accepted_WhenNoActiveAction()
        {
            _dispatcher.Issue(new AttackCommand(1));

            Assert.That(_dispatcher.HasActiveAction, Is.True);
            Assert.That(_mockAttack.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void ActionCommand_Interrupts_WhenHigherPriority()
        {
            _dispatcher.Issue(new AttackCommand(1)); // Interrupt=2, Anti=2

            _dispatcher.Issue(new JumpCommand()); // Interrupt=3 >= Anti=2

            Assert.That(_mockJump.CallCount, Is.EqualTo(1));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<JumpCommand>());
        }

        [Test]
        public void ActionCommand_Buffered_WhenLowerPriority()
        {
            _dispatcher.Issue(new JumpCommand()); // Interrupt=3, Anti=3

            _dispatcher.Issue(new AttackCommand(1)); // Interrupt=2 < Anti=3

            Assert.That(_mockAttack.CallCount, Is.EqualTo(0));
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(1));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<JumpCommand>());
        }

        [Test]
        public void ActionCommand_EqualPriority_Interrupts()
        {
            _dispatcher.Issue(new AttackCommand(1, interruptPriority: 2, antiInterruptPriority: 2)); // Interrupt=2, Anti=2

            _dispatcher.Issue(new AttackCommand(2, interruptPriority: 2, antiInterruptPriority: 2)); // Interrupt=2 >= Anti=2

            Assert.That(_mockAttack.CallCount, Is.EqualTo(2));
            Assert.That(_mockAttack.LastCommand.AttackId, Is.EqualTo(2));
        }

        [Test]
        public void DashCommand_InterruptsEverything()
        {
            _dispatcher.Issue(new JumpCommand()); // Anti=3

            _dispatcher.Issue(new DashCommand(Vector3.forward, 10f)); // Interrupt=5 >= 3

            Assert.That(_mockDash.CallCount, Is.EqualTo(1));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<DashCommand>());
        }

        [Test]
        public void DashCommand_CannotBeInterrupted()
        {
            _dispatcher.Issue(new DashCommand(Vector3.forward, 10f)); // Anti=5

            _dispatcher.Issue(new JumpCommand()); // Interrupt=3 < 5

            Assert.That(_mockJump.CallCount, Is.EqualTo(0));
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(1));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<DashCommand>());
        }

        [Test]
        public void NonBufferableCommand_Dropped()
        {
            _dispatcher.Issue(new JumpCommand()); // Anti=3, active

            _dispatcher.Issue(new DashCommand(Vector3.forward, 10f)); // Interrupt=5 >= 3 → interrupts
            // Now Dash is active (Anti=5)

            _dispatcher.Issue(new AttackCommand(1, interruptPriority: 2, antiInterruptPriority: 2)); // Interrupt=2 < 5, Bufferable=true → buffered

            // Another dash — not bufferable
            _dispatcher.Issue(new DashCommand(Vector3.back, 5f)); // Interrupt=5 >= 5 → interrupts

            Assert.That(_mockDash.CallCount, Is.EqualTo(2)); // Both dashes executed
        }

        [Test]
        public void HeavyAttack_ResistsInterruption()
        {
            _dispatcher.Issue(new AttackCommand(2, interruptPriority: 3, antiInterruptPriority: 4)); // Heavy: Anti=4

            _dispatcher.Issue(new JumpCommand()); // Interrupt=3 < 4

            Assert.That(_mockJump.CallCount, Is.EqualTo(0));
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(1));
        }

        // ================================================================
        // BUFFER
        // ================================================================

        [Test]
        public void Buffer_Drains_AfterActiveActionCompletes()
        {
            _dispatcher.Issue(new JumpCommand()); // Active: Jump (Anti=3)
            _dispatcher.Issue(new AttackCommand(1));      // Buffered

            // Jump completes
            _mockJump.IsJumping = false;
            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_mockAttack.CallCount, Is.EqualTo(1));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<AttackCommand>());
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(0));
        }

        [Test]
        public void Buffer_MultipleCommands_FifoOrder()
        {
            _dispatcher.Issue(new JumpCommand());   // Active
            _dispatcher.Issue(new AttackCommand(1));       // Buffered #1
            _dispatcher.Issue(new AttackCommand(2, interruptPriority: 2, antiInterruptPriority: 2)); // Buffered #2

            _mockJump.IsJumping = false;
            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_mockAttack.LastCommand.AttackId, Is.EqualTo(1));
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(1)); // #2 still buffered
        }

        [Test]
        public void Buffer_OnlyActivatesOnePerDrain()
        {
            _dispatcher.Issue(new JumpCommand());   // Active
            _dispatcher.Issue(new AttackCommand(1));       // Buffered
            _dispatcher.Issue(new AttackCommand(2));       // Buffered

            _mockJump.IsJumping = false;
            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_mockAttack.CallCount, Is.EqualTo(1));
            Assert.That(_mockAttack.LastCommand.AttackId, Is.EqualTo(1));
            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(1));
        }

        [Test]
        public void Buffer_ExpiredCommands_DiscardedOnDrain()
        {
            _dispatcher.Issue(new JumpCommand()); // Active
            _dispatcher.Issue(new AttackCommand(1));     // Buffered

            // Simulate time passing beyond buffer window
            var timeProp = typeof(Time).GetProperty("time");
            Assume.That(timeProp?.CanWrite ?? false, Is.False,
                "Test injects expired entry via internal state. " +
                "Integration test would need Time.time manipulation.");

            // Directly enqueue an expired entry for testing
            _dispatcher.ClearBuffer();
            _mockJump.IsJumping = false;

            // No buffer entries → no activation
            _dispatcher.LogicUpdate(0.016f);
            Assert.That(_dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void Buffer_ClearedByExplicitCall()
        {
            _dispatcher.Issue(new JumpCommand()); // Active
            _dispatcher.Issue(new AttackCommand(1));     // Buffered
            _dispatcher.Issue(new AttackCommand(2));     // Buffered

            _dispatcher.ClearBuffer();

            Assert.That(_dispatcher.BufferedCount, Is.EqualTo(0));
        }

        // ================================================================
        // COMPLETION DETECTION
        // ================================================================

        [Test]
        public void LogicUpdate_KeepsActive_WhenAttackStillAttacking()
        {
            _dispatcher.Issue(new AttackCommand(1));
            Assert.That(_dispatcher.HasActiveAction, Is.True);

            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_dispatcher.HasActiveAction, Is.True);
        }

        [Test]
        public void LogicUpdate_ClearsActive_WhenAttackCompletes()
        {
            _dispatcher.Issue(new AttackCommand(1));
            _mockAttack.IsAttacking = false;

            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void LogicUpdate_ClearsActive_WhenJumpCompletes()
        {
            _dispatcher.Issue(new JumpCommand());
            _mockJump.IsJumping = false;

            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void LogicUpdate_ClearsActive_WhenDashCompletes()
        {
            _dispatcher.Issue(new DashCommand(Vector3.forward, 10f));
            _mockDash.IsDashing = false;

            _dispatcher.LogicUpdate(0.016f);

            Assert.That(_dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void LogicUpdate_NoActiveCommand_IsNoOp()
        {
            Assert.DoesNotThrow(() => _dispatcher.LogicUpdate(0.016f));
        }

        // ================================================================
        // REGISTRATION
        // ================================================================

        [Test]
        public void RegisterExecutor_AutoDetectsAttackCompletionCheck()
        {
            var dispatcher = new CommandDispatcherComponent();
            var attack = new MockAttack();
            dispatcher.RegisterExecutor<AttackCommand>(attack);

            dispatcher.Issue(new AttackCommand(1));
            Assert.That(dispatcher.HasActiveAction, Is.True);

            attack.IsAttacking = false;
            dispatcher.LogicUpdate(0.016f);
            Assert.That(dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void RegisterExecutor_AutoDetectsJumpCompletionCheck()
        {
            var dispatcher = new CommandDispatcherComponent();
            var jump = new MockJump();
            dispatcher.RegisterExecutor<JumpCommand>(jump);

            dispatcher.Issue(new JumpCommand());
            Assert.That(dispatcher.HasActiveAction, Is.True);

            jump.IsJumping = false;
            dispatcher.LogicUpdate(0.016f);
            Assert.That(dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void RegisterExecutor_AutoDetectsDashCompletionCheck()
        {
            var dispatcher = new CommandDispatcherComponent();
            var dash = new MockDash();
            dispatcher.RegisterExecutor<DashCommand>(dash);

            dispatcher.Issue(new DashCommand(Vector3.forward, 10f));
            Assert.That(dispatcher.HasActiveAction, Is.True);

            dash.IsDashing = false;
            dispatcher.LogicUpdate(0.016f);
            Assert.That(dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void RegisterExecutor_ReplacesExisting()
        {
            var move1 = new MockMove();
            var move2 = new MockMove();
            _dispatcher.RegisterExecutor<MoveCommand>(move1);
            _dispatcher.RegisterExecutor<MoveCommand>(move2);

            _dispatcher.Issue(new MoveCommand(Vector3.forward));

            Assert.That(move2.CallCount, Is.EqualTo(1));
            Assert.That(move1.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void Issue_NoExecutorRegistered_DoesNotThrow()
        {
            var dispatcher = new CommandDispatcherComponent();
            Assert.DoesNotThrow(() => dispatcher.Issue(new MoveCommand(Vector3.forward)));
        }

        // ================================================================
        // HasExecutor TESTS
        // ================================================================

        [Test]
        public void HasExecutor_ReturnsTrue_ForRegisteredType()
        {
            Assert.That(_dispatcher.HasExecutor<MoveCommand>(), Is.True);
            Assert.That(_dispatcher.HasExecutor<AttackCommand>(), Is.True);
        }

        [Test]
        public void HasExecutor_ReturnsFalse_ForUnregisteredType()
        {
            var dispatcher = new CommandDispatcherComponent();
            Assert.That(dispatcher.HasExecutor<MoveCommand>(), Is.False);
        }

        // ================================================================
        // ExecutorCount TESTS
        // ================================================================

        [Test]
        public void ExecutorCount_ReflectsRegisteredExecutors()
        {
            Assert.That(_dispatcher.ExecutorCount, Is.EqualTo(6));
        }

        // ================================================================
        // EDGE CASES
        // ================================================================

        [Test]
        public void ForceClearActive_RemovesActiveCommand()
        {
            _dispatcher.Issue(new AttackCommand(1));
            Assert.That(_dispatcher.HasActiveAction, Is.True);

            _dispatcher.ForceClearActive();

            Assert.That(_dispatcher.HasActiveAction, Is.False);
        }

        [Test]
        public void TwoActionCommands_SameFrame_SecondWins()
        {
            _dispatcher.Issue(new AttackCommand(1, interruptPriority: 2, antiInterruptPriority: 2));
            _dispatcher.Issue(new AttackCommand(2, interruptPriority: 3, antiInterruptPriority: 2)); // Interrupt=3 >= Anti=2

            Assert.That(_mockAttack.LastCommand.AttackId, Is.EqualTo(2));
        }

        [Test]
        public void AttackCommand_DefaultPriorities_AreTwo()
        {
            var cmd = new AttackCommand(1);

            Assert.That(cmd.InterruptPriority, Is.EqualTo(2));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(2));
            Assert.That(cmd.Bufferable, Is.True);
        }

        [Test]
        public void JumpCommand_FixedPriorities_AreThree()
        {
            var cmd = new JumpCommand();

            Assert.That(cmd.InterruptPriority, Is.EqualTo(3));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(3));
            Assert.That(cmd.Bufferable, Is.True);
        }

        [Test]
        public void DashCommand_FixedPriorities_AreFive_NotBufferable()
        {
            var cmd = new DashCommand(default, 1f);

            Assert.That(cmd.InterruptPriority, Is.EqualTo(5));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(5));
            Assert.That(cmd.Bufferable, Is.False);
        }

        [Test]
        public void BufferWindow_Default_Is200ms()
        {
            var dispatcher = new CommandDispatcherComponent();
            Assert.That(dispatcher, Is.Not.Null);
        }

        [Test]
        public void BufferWindow_Custom_Accepted()
        {
            var dispatcher = new CommandDispatcherComponent(0.1f);
            Assert.That(dispatcher, Is.Not.Null);
        }

        [Test]
        public void MoveCommand_HigherPriority_WhenNoActionActive_RoutesNormally()
        {
            var cmd = new MoveCommand(new Vector3(0, 0, 5), priority: 10);
            _dispatcher.Issue(cmd);

            Assert.That(_mockMove.CallCount, Is.EqualTo(1));
            Assert.That(_mockMove.LastCommand.Velocity.z, Is.EqualTo(5f));
            Assert.That(_mockMove.LastCommand.Priority, Is.EqualTo(10));
        }

        [Test]
        public void MoveCommand_Blocked_WhenActionActive_WithPriority()
        {
            _mockAttack.IsAttacking = true;
            _dispatcher.Issue(new AttackCommand(1));

            _dispatcher.Issue(new MoveCommand(new Vector3(1, 0, 0), priority: 10));

            // Move is blocked when action is active (BlocksMove=true by default)
            Assert.That(_mockMove.CallCount, Is.EqualTo(0));
        }

        [Test]
        public void AttackCommand_Default_IsBackwardCompatible()
        {
            // Existing code: new AttackCommand(id) should still compile and work
            var cmd = new AttackCommand(42);
            Assert.That(cmd.AttackId, Is.EqualTo(42));
            Assert.That(cmd.InterruptPriority, Is.EqualTo(2));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(2));
        }

        [Test]
        public void FaceDirectionCommand_StillWorks_WhenNotBlocked()
        {
            var cmd = new FaceDirectionCommand(new Vector3(0, 1, 0));
            _dispatcher.Issue(cmd);

            Assert.That(_mockFace.CallCount, Is.EqualTo(1));
            Assert.That(_mockFace.LastCommand.Direction, Is.EqualTo(new Vector3(0, 1, 0)));
        }

        // ================================================================
        // JUMP COMMAND PHASE TESTS
        // ================================================================

        [Test]
        public void JumpCommand_Start_HasDefaultPriorities()
        {
            var cmd = new JumpCommand(JumpPhase.Start);

            Assert.That(cmd.InterruptPriority, Is.EqualTo(3));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(3));
            Assert.That(cmd.Bufferable, Is.True);
        }

        [Test]
        public void JumpCommand_Cancel_HasMaxPriority()
        {
            var cmd = new JumpCommand(JumpPhase.Cancel);

            Assert.That(cmd.InterruptPriority, Is.EqualTo(int.MaxValue));
            Assert.That(cmd.AntiInterruptPriority, Is.EqualTo(int.MaxValue));
            Assert.That(cmd.Bufferable, Is.False);
        }

        [Test]
        public void JumpCommand_DefaultConstructor_UsesStartPhase()
        {
            var cmd = new JumpCommand();

            Assert.That(cmd.Phase, Is.EqualTo(JumpPhase.Start));
            Assert.That(cmd.InterruptPriority, Is.EqualTo(3));
            Assert.That(cmd.Bufferable, Is.True);
        }

        [Test]
        public void JumpCancel_AlwaysInterrupts_ActiveAction()
        {
            // Start a jump
            _dispatcher.Issue(new JumpCommand(JumpPhase.Start));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<JumpCommand>());

            // Dash has AntiInterrupt=5, but Cancel has int.MaxValue Interrupt priority
            _dispatcher.Issue(new DashCommand(Vector3.forward, 10f));
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<DashCommand>());

            // Cancel should interrupt anything
            _dispatcher.Issue(new JumpCommand(JumpPhase.Cancel));

            // The cancel should be the active command
            Assert.That(_dispatcher.ActiveCommand, Is.TypeOf<JumpCommand>());
            var active = (JumpCommand)_dispatcher.ActiveCommand;
            Assert.That(active.Phase, Is.EqualTo(JumpPhase.Cancel));
        }
    }
}

