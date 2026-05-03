using GenBall.BattleSystem.Command;
using GenBall.Enemy.Controller;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public class AIAttackState : EnemyAIStateBase
    {
        private bool _attackIssued;

        protected internal override void OnEnter(Fsm<EnemyAIController> fsm)
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

        protected internal override void OnFixedUpdate(Fsm<EnemyAIController> fsm, float fixeDeltaTime)
        {
            if (!_attackIssued) return;

            if (!AttackController.IsAttacking)
            {
                if (Detect.HasTarget && Detect.InAttackRange)
                {
                    IssueCommand(new FaceDirectionCommand(Detect.DirectionToTarget));
                    IssueCommand(new AttackCommand(StateConfig.attackId));
                }
                else
                {
                    ChangeState<AIChaseState>();
                }
            }
            else if (Detect.HasTarget)
            {
                IssueCommand(new FaceDirectionCommand(Detect.DirectionToTarget));
            }
        }
    }
}
