using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework.AI;
using UnityEngine;
using GenBall.Enemy.AI;
using GenBall.Enemy.Detect;
using GenBall.BattleSystem.Executors;
using GenBall.Framework.Entity;
using Yueyn.Fsm;

// Note: EnemyDecisionLayer uses Yueyn.Fsm.Fsm<T> directly (not GameEntry.Fsm).
// Fsm<T> is a generic state machine library with no dependency on the old IComponent framework.
// Create via Fsm<T>.Create(), destroy via _fsm.Shutdown().

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Enemy decision layer driven by an AI finite state machine.
    /// Creates an FSM with states loaded from EnemyAIConfigSo (data-driven).
    /// Falls back to hardcoded default states if no config is provided.
    ///
    /// Implements IDecisionLayer + IEntityLogicUpdate so it receives frame
    /// updates from EntityUpdateSystem when registered with a BattleEntity.
    /// </summary>
    public class EnemyDecisionLayer : IDecisionLayer, IEntityLogicUpdate
    {
        private readonly BattleEntity _entity;
        private readonly EnemyAIConfigSo _aiConfig;
        private Fsm<EnemyDecisionLayer> _fsm;

        public CommandDispatcherComponent Dispatcher { get; set; }
        public EnemyDetector Detect { get; private set; }
        public IAttack AttackController { get; private set; }

        /// <summary>
        /// Create an enemy decision layer.
        /// </summary>
        /// <param name="entity">The owning BattleEntity (may be null for tests).</param>
        /// <param name="aiConfig">AI configuration ScriptableObject (null = no FSM, graceful no-op).</param>
        public EnemyDecisionLayer(BattleEntity entity, EnemyAIConfigSo aiConfig)
        {
            _entity = entity;
            _aiConfig = aiConfig;

            Detect = entity?.Get<EnemyDetector>();
            AttackController = (entity?.Get<EnemyDashExecutor>() as IAttack) ?? entity?.Get<IAttack>();

            if (aiConfig != null)
            {
                InitializeFsm();
            }
        }

        /// <summary>
        /// Issue a command to this entity. Routes through the CommandDispatcherComponent
        /// if available, otherwise silently drops the command.
        /// </summary>
        public void IssueCommand(ICommand command)
        {
            Dispatcher?.Issue(command);
        }

        /// <summary>
        /// Run the AI decision-making for this frame.
        /// Ticks the FSM if it is active.
        /// </summary>
        public void MakeDecision(float deltaTime)
        {
            if (_fsm != null && !_fsm.IsDestroyed)
            {
                _fsm.FixedUpdate(deltaTime);
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            MakeDecision(deltaTime);
        }

        /// <summary>
        /// Clean up the FSM. Must be called by the owner when the entity is destroyed,
        /// since EnemyDecisionLayer is not a MonoBehaviour.
        /// </summary>
        public void Cleanup()
        {
            if (_fsm != null && !_fsm.IsDestroyed)
            {
                _fsm.Shutdown();
                _fsm = null;
            }
        }

        /// <summary>
        /// Initialize the FSM from the data-driven AI config.
        /// Uses CreateStatesForLayer() to build state list from config entries,
        /// then starts the FSM with the config's start state type.
        /// Falls back to hardcoded default states if aiConfig is null (test/stub scenario).
        /// </summary>
        private void InitializeFsm()
        {
            if (_aiConfig != null)
            {
                var states = _aiConfig.CreateStatesForLayer();
                _fsm = Fsm<EnemyDecisionLayer>.Create($"EnemyDecision_{GetHashCode()}", this, states);
                _fsm.Start(_aiConfig.StartStateType);
            }
            else
            {
                // Fallback: hardcoded default states for tests / no-config scenarios
                var idleState = new AIDecisionIdleState();
                idleState.SetConfig(new AIStateConfig { stateName = "Idle" });

                var chaseState = new AIDecisionChaseState();
                chaseState.SetConfig(new AIStateConfig { stateName = "Chase", moveSpeed = 5f });

                var attackState = new AIDecisionAttackState();
                attackState.SetConfig(new AIStateConfig { stateName = "Attack", attackId = 0 });

                var wanderState = new AIDecisionWanderState();
                wanderState.SetConfig(new AIStateConfig { stateName = "Wander", moveSpeed = 2f, duration = 3f });

                var states = new FsmState<EnemyDecisionLayer>[]
                {
                    idleState,
                    chaseState,
                    attackState,
                    wanderState
                };

                _fsm = Fsm<EnemyDecisionLayer>.Create($"EnemyDecision_{GetHashCode()}", this, states);
                _fsm.Start<AIDecisionIdleState>();
            }
        }
    }
}
