using GenBall.BattleSystem.Command;
using GenBall.Enemy.Controller;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public class AIChaseState : EnemyAIStateBase
    {
        protected internal override void OnFixedUpdate(Fsm<EnemyAIController> fsm, float fixeDeltaTime)
        {
            if (!Detect.HasTarget || !Detect.InHateRange)
            {
                ChangeState<AIIdleState>();
                return;
            }

            if (Detect.InAttackRange && !AttackController.IsAttacking)
            {
                ChangeState<AIAttackState>();
                return;
            }

            var direction = Detect.DirectionToTarget;
            IssueCommand(new MoveCommand(direction * StateConfig.moveSpeed));
            IssueCommand(new FaceDirectionCommand(direction));
        }
    }
}
