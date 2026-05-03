using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.AI;
using Yueyn.Fsm;

namespace GenBall.Enemy.Controller
{
    public class EnemyAIController : CharacterControllerBase
    {
        [UnityEngine.SerializeField] private EnemyAIConfigSo aiConfig;

        private CharacterState _characterState;
        private Fsm<EnemyAIController> _fsm;

        public EnemyDetectController Detect { get; private set; }
        public EnemyAttackController AttackController { get; private set; }
        public CharacterState CharacterState => _characterState;

        public override void Initialize(CharacterState characterState)
        {
            _characterState = characterState;
            Detect = characterState.GetComponentInChildren<EnemyDetectController>();
            AttackController = characterState.GetComponentInChildren<EnemyAttackController>();

            var states = aiConfig.CreateStates();
            var startState = aiConfig.StartStateType;
            _fsm = GameEntry.Fsm.CreateFsm($"EnemyAI_{GetHashCode()}", this, states);
            
            _fsm.PrintLog = true;
            _fsm.Start(startState);
        }

        public override void Tick(float deltaTime)
        {
            if (_characterState.IsDead) return;
            _fsm.FixedUpdate(deltaTime);
        }

        public void IssueCommand(ICommand command)
        {
            _characterState.HandleCommand(command);
        }

        private void OnDestroy()
        {
            if (_fsm != null && !_fsm.IsDestroyed)
                GameEntry.Fsm.DestroyFsm(_fsm);
        }
    }
}
