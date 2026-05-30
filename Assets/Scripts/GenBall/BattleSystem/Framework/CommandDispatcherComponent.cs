using System;
using System.Collections.Generic;
using GenBall.BattleSystem.Command;
using GenBall.Framework.Entity;
using GenBall.Procedure.Game;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Receives commands from the Decision Layer, arbitrates action commands by
    /// InterruptPriority vs AntiInterruptPriority, buffers overflow commands,
    /// and routes accepted commands to registered executors.
    ///
    /// Lifecycle: BattleEntity component implementing IEntityLogicUpdate.
    /// Register BEFORE DecisionLayer so completion/buffer-drain runs first each frame.
    ///
    /// DecisionLayer (doesn't know): arbitration, buffering, executor routing, priority
    /// CommandDispatcher (doesn't know): input, AI, what "jump" or "attack" means
    /// </summary>
    public class CommandDispatcherComponent : IEntityLogicUpdate
    {
        private readonly float _bufferWindowSeconds;

        // Executor registry: command type → executor instance
        private readonly Dictionary<Type, object> _executors = new();

        // Completion check registry: command type → delegate that returns true while executing
        private readonly Dictionary<Type, Func<bool>> _completionChecks = new();

        // Currently active action command
        private IArbitratedCommand _activeCommand;
        private Type _activeCommandType;
        private Func<bool> _activeCompletionCheck;

        // Input buffer: FIFO queue with timestamps
        private readonly Queue<BufferedEntry> _buffer = new();

        public CommandDispatcherComponent(float bufferWindowSeconds = 0.2f)
        {
            _bufferWindowSeconds = bufferWindowSeconds;
        }

        // ================================================================
        // PUBLIC API
        // ================================================================

        /// <summary>
        /// Register an executor to handle a specific command type.
        /// Auto-detects completion delegates for IAttack (IsAttacking),
        /// IJump (IsJumping), IDash (IsDashing),
        /// IReload (IsReloading), ISwitchWeapon (IsSwitching).
        /// </summary>
        public void RegisterExecutor<TCommand>(object executor) where TCommand : ICommand
        {
            var cmdType = typeof(TCommand);
            _executors[cmdType] = executor;

            // Auto-detect completion check based on executor interface
            if (executor is IAttack attack)
                _completionChecks[cmdType] = () => attack.IsAttacking;
            else if (executor is IJump jump)
                _completionChecks[cmdType] = () => jump.IsJumping;
            else if (executor is IDash dash)
                _completionChecks[cmdType] = () => dash.IsDashing;
            else if (executor is IReload reload)
                _completionChecks[cmdType] = () => reload.IsReloading;
            else if (executor is ISwitchWeapon switchWeapon)
                _completionChecks[cmdType] = () => switchWeapon.IsSwitching;
            else if (executor is IStun stun)
                _completionChecks[cmdType] = () => stun.IsStunned;
        }

        /// <summary>
        /// Called by DecisionLayer each frame. Routes continuous commands directly
        /// (zeroing Move when blocked), action commands through the arbiter.
        /// Does nothing when paused.
        /// </summary>
        public void Issue(ICommand command)
        {
            switch (command)
            {
                case MoveCommand moveCmd:
                    RouteMove(moveCmd);
                    break;

                case RotateCommand rotCmd:
                    RouteRotate(rotCmd);
                    break;

                case FaceDirectionCommand faceCmd:
                    RouteFaceDirection(faceCmd);
                    break;

                case IArbitratedCommand arbCmd:
                    Arbiter(arbCmd);
                    break;
            }
        }

        // ================================================================
        // IEntityLogicUpdate
        // ================================================================

        public void LogicUpdate(float deltaTime)
        {
            CleanExpiredBufferEntries();

            if (_activeCommand == null)
                return;

            // Poll the active executor's completion flag
            if (_activeCompletionCheck != null && !_activeCompletionCheck.Invoke())
            {
                _activeCommand = null;
                _activeCommandType = null;
                _activeCompletionCheck = null;

                DrainBuffer();
            }
        }

        // ================================================================
        // CONTINUOUS COMMAND ROUTING
        // ================================================================

        private void RouteMove(MoveCommand cmd)
        {
            if (!_executors.TryGetValue(typeof(MoveCommand), out var executor) || executor is not IMove move)
                return;

            // Action declares whether it blocks movement
            if (_activeCommand is { BlocksMove: true })
                return;

            move.Move(cmd);
        }

        private void RouteRotate(RotateCommand cmd)
        {
            // Action declares whether it blocks rotation
            if (_activeCommand is { BlocksRotate: true })
                return;

            if (_executors.TryGetValue(typeof(RotateCommand), out var executor) && executor is IRotate rotate)
                rotate.Rotate(cmd);
        }

        private void RouteFaceDirection(FaceDirectionCommand cmd)
        {
            if (_activeCommand != null)
                return;

            if (_executors.TryGetValue(typeof(FaceDirectionCommand), out var executor) && executor is IFaceDirection face)
                face.Face(cmd);
        }

        // ================================================================
        // ARBITRATION
        // ================================================================

        private void Arbiter(IArbitratedCommand newCmd)
        {
            // No active command — accept immediately
            if (_activeCommand == null)
            {
                Activate(newCmd);
                return;
            }

            // Can the new command interrupt the active one?
            if (newCmd.InterruptPriority >= _activeCommand.AntiInterruptPriority)
            {
                Activate(newCmd);
                return;
            }

            // Cannot interrupt — buffer or drop
            if (newCmd.Bufferable)
            {
                _buffer.Enqueue(new BufferedEntry(newCmd, Time.time));
            }
        }

        private void Activate(IArbitratedCommand cmd)
        {
            CancelActive();

            _activeCommand = cmd;
            _activeCommandType = GetCommandType(cmd);
            _completionChecks.TryGetValue(_activeCommandType, out _activeCompletionCheck);

            // Route to executor
            RouteExecutor(_activeCommandType, cmd);
        }

        private void CancelActive()
        {
            if (_activeCommandType == null) return;
            if (_executors.TryGetValue(_activeCommandType, out var executor))
            {
                if (executor is IAttack attack && attack.IsAttacking)
                    attack.Cancel();
                else if (executor is IDash dash && dash.IsDashing)
                    dash.Dash(new DashCommand(Vector3.zero, 0f)); // zero dash = immediate end
            }
        }

        // ================================================================
        // BUFFER
        // ================================================================

        private void DrainBuffer()
        {
            while (_buffer.Count > 0)
            {
                var entry = _buffer.Dequeue();

                if (Time.time - entry.Timestamp > _bufferWindowSeconds)
                    continue; // Expired — discard

                Activate(entry.Command);
                return; // Only activate ONE per drain
            }
        }

        private void CleanExpiredBufferEntries()
        {
            while (_buffer.Count > 0 &&
                   Time.time - _buffer.Peek().Timestamp > _bufferWindowSeconds)
            {
                _buffer.Dequeue();
            }
        }

        // ================================================================
        // EXECUTOR ROUTING
        // ================================================================

        private void RouteExecutor(Type cmdType, ICommand cmd)
        {
            if (!_executors.TryGetValue(cmdType, out var executor))
                return;

            switch (executor)
            {
                case IAttack attack when cmd is AttackCommand attackCmd:
                    attack.Attack(attackCmd);
                    break;

                case IJump jump when cmd is JumpCommand jumpCmd:
                    jump.Jump(jumpCmd);
                    break;

                case IDash dash when cmd is DashCommand dashCmd:
                    dash.Dash(dashCmd);
                    break;

                case IReload reload when cmd is ReloadCommand reloadCmd:
                    reload.Reload(reloadCmd);
                    break;

                case ISwitchWeapon switchWeapon when cmd is SwitchWeaponCommand switchCmd:
                    switchWeapon.SwitchWeapon(switchCmd);
                    break;

                case IStun stun when cmd is StunCommand stunCmd:
                    stun.Stun(stunCmd);
                    break;

                case IInteract interact when cmd is InteractCommand interactCmd:
                    interact.Interact(interactCmd);
                    break;
            }
        }

        // ================================================================
        // HELPERS
        // ================================================================

        private static Type GetCommandType(IArbitratedCommand cmd)
        {
            return cmd switch
            {
                AttackCommand => typeof(AttackCommand),
                JumpCommand => typeof(JumpCommand),
                DashCommand => typeof(DashCommand),
                ReloadCommand => typeof(ReloadCommand),
                SwitchWeaponCommand => typeof(SwitchWeaponCommand),
                StunCommand => typeof(StunCommand),
                InteractCommand => typeof(InteractCommand),
                _ => cmd.GetType()
            };
        }

        // ================================================================
        // PUBLIC TEST ACCESS (different assemblies)
        // ================================================================

        public bool HasActiveAction => _activeCommand != null;
        public IArbitratedCommand ActiveCommand => _activeCommand;
        public Type ActiveCommandType => _activeCommandType;
        public int BufferedCount => _buffer.Count;
        public int ExecutorCount => _executors.Count;

        public bool HasExecutor<T>() where T : ICommand => _executors.ContainsKey(typeof(T));

        public void ClearBuffer() => _buffer.Clear();

        public void ForceClearActive()
        {
            _activeCommand = null;
            _activeCommandType = null;
            _activeCompletionCheck = null;
        }

        private struct BufferedEntry
        {
            public readonly IArbitratedCommand Command;
            public readonly float Timestamp;

            public BufferedEntry(IArbitratedCommand command, float timestamp)
            {
                Command = command;
                Timestamp = timestamp;
            }
        }
    }
}
// force recompile
