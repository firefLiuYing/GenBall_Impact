using GenBall.BattleSystem.Command;
using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework.AI
{
    public class AIDecisionAttackState : EnemyDecisionStateBase
    {
        private bool _attackIssued;

        protected internal override void OnEnter(Fsm<EnemyDecisionLayer> fsm)
        {
            base.OnEnter(fsm);
            _attackIssued = false;

            if (Detect.HasTarget)
            {
                IssueCommand(new FaceDirectionCommand(Detect.DirectionToTarget));
            }
            IssueCommand(new AttackCommand(StateConfig.attackId));
            _attackIssued = true;
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyDecisionLayer> fsm, float fixedDeltaTime)
        {
            if (!_attackIssued) return;

            if (!(AttackController?.IsAttacking ?? false))
            {
                if (Detect.HasTarget && Detect.InAttackRange)
                {
                    IssueCommand(new FaceDirectionCommand(Detect.DirectionToTarget));
                    IssueCommand(new AttackCommand(StateConfig.attackId));
                }
                else
                {
                    ChangeState<AIDecisionChaseState>();
                }
            }
            else if (Detect.HasTarget)
            {
                // Attack is committed — face target until complete
                IssueCommand(new FaceDirectionCommand(Detect.DirectionToTarget));
            }
        }
    }
}
