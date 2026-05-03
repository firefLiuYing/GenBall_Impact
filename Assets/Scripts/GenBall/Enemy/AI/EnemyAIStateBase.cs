using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.Enemy.Controller;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public abstract class EnemyAIStateBase : FsmState<EnemyAIController>
    {
        protected Fsm<EnemyAIController> Fsm;
        protected EnemyAIController AI => Fsm.Owner;
        protected CharacterState Character => AI.CharacterState;
        protected EnemyDetectController Detect => AI.Detect;
        protected EnemyAttackController AttackController => AI.AttackController;
        public AIStateConfig StateConfig { get; private set; }

        public void SetConfig(AIStateConfig config) => StateConfig = config;

        protected internal override void OnEnter(Fsm<EnemyAIController> fsm)
        {
            Fsm = fsm;
        }

        protected void ChangeState<T>() where T : EnemyAIStateBase
            => Fsm.ChangeState<T>();

        protected void IssueCommand(ICommand command) => AI.IssueCommand(command);
    }
}
