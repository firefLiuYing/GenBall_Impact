using GenBall.BattleSystem.Command;
using GenBall.Enemy.AI;
using GenBall.Enemy.Controller;
using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework.AI
{
    public abstract class EnemyDecisionStateBase : FsmState<EnemyDecisionLayer>
    {
        protected Fsm<EnemyDecisionLayer> Fsm;
        protected EnemyDecisionLayer Agent => Fsm.Owner;
        protected EnemyDetectController Detect => Agent.Detect;
        protected EnemyAttackController AttackController => Agent.AttackController;
        public AIStateConfig StateConfig { get; private set; }

        public void SetConfig(AIStateConfig config) => StateConfig = config;

        protected internal override void OnEnter(Fsm<EnemyDecisionLayer> fsm)
        {
            Fsm = fsm;
        }

        protected void ChangeState<T>() where T : EnemyDecisionStateBase
            => Fsm.ChangeState<T>();

        protected void IssueCommand(ICommand command) => Agent.IssueCommand(command);
    }
}
