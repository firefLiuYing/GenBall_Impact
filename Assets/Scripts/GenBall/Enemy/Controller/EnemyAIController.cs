using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.AI;
using Yueyn.Fsm;

namespace GenBall.Enemy.Controller
{
    public class EnemyAIController : CharacterControllerBase
    {
        [UnityEngine.SerializeField] private EnemyAIConfigSo aiConfig;
        public EnemyAIConfigSo AiConfig => aiConfig;

        private CharacterState _characterState;
        private Fsm<EnemyAIController> _fsm;

        public EnemyDetectController Detect { get; private set; }
        public EnemyAttackController AttackController { get; private set; }
        public CharacterState CharacterState => _characterState;

        public override void Initialize(CharacterState characterState)
        {
            // [DEPRECATED] EnemyAIController will be removed in Phase E.
            // BattleEntity-based enemies use EnemyDecisionLayer instead.
        }

        public override void Tick(float deltaTime)
        {
            // [DEPRECATED] EnemyAIController will be removed in Phase E.
        }

        public void IssueCommand(ICommand command)
        {
            // [DEPRECATED] EnemyAIController will be removed in Phase E.
        }

        private void OnDestroy()
        {
            // [DEPRECATED] EnemyAIController will be removed in Phase E.
        }
    }
}
