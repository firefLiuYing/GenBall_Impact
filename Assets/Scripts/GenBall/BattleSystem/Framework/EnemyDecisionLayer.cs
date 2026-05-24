using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework.AI;
using GenBall.Enemy.AI;
using GenBall.Enemy.Controller;
using GenBall.Framework.Entity;
using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework
{
    /// <summary>
    /// Enemy decision layer driven by an AI finite state machine.
    /// Creates an FSM with hardcoded states (Idle, Chase, Attack, Wander)
    /// and issues commands through the CommandDispatcherComponent each frame.
    ///
    /// Implements IDecisionLayer + IEntityLogicUpdate so it receives frame
    /// updates from EntityUpdateSystem when registered with a BattleEntity.
    ///
    /// TODO Phase B: Make FSM states data-driven from aiConfig rather than hardcoded.
    /// </summary>
    public class EnemyDecisionLayer : IDecisionLayer, IEntityLogicUpdate
    {
        private readonly BattleEntity _entity;
        private Fsm<EnemyDecisionLayer> _fsm;

        public CommandDispatcherComponent Dispatcher { get; set; }
        public EnemyDetectController Detect { get; private set; }
        public EnemyAttackController AttackController { get; private set; }

        /// <summary>
        /// Create an enemy decision layer.
        /// </summary>
        /// <param name="entity">The owning BattleEntity (may be null for tests).</param>
        /// <param name="aiConfig">AI configuration ScriptableObject (null = no FSM, graceful no-op).</param>
        public EnemyDecisionLayer(BattleEntity entity, EnemyAIConfigSo aiConfig)
        {
            _entity = entity;

            Detect = entity?.GetComponentInChildren<EnemyDetectController>();
            AttackController = entity?.GetComponentInChildren<EnemyAttackController>();

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
                GameEntry.Fsm.DestroyFsm(_fsm);
                _fsm = null;
            }
        }

        /// <summary>
        /// TODO Phase B: Make this data-driven from aiConfig.
        /// Currently hardcodes the 4-state FSM with default config values.
        /// </summary>
        private void InitializeFsm()
        {
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

            _fsm = GameEntry.Fsm.CreateFsm($"EnemyDecision_{GetHashCode()}", this, states);
            _fsm.Start<AIDecisionIdleState>();
        }
    }
}
