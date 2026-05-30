using GenBall.BattleSystem.Command;
using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework.AI
{
    public class AIDecisionChaseState : EnemyDecisionStateBase
    {
        protected internal override void OnFixedUpdate(Fsm<EnemyDecisionLayer> fsm, float fixedDeltaTime)
        {
            if (!Detect.HasTarget || !Detect.InHateRange)
            {
                ChangeState<AIDecisionIdleState>();
                return;
            }

            if (Detect.InAttackRange && !(AttackController?.IsAttacking ?? false))
            {
                ChangeState<AIDecisionAttackState>();
                return;
            }

            var direction = Detect.DirectionToTarget;
            IssueCommand(new MoveCommand(direction * StateConfig.moveSpeed));
            IssueCommand(new FaceDirectionCommand(direction));
        }
    }
}
