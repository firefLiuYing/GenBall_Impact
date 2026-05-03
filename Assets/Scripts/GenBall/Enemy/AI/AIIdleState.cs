using GenBall.Enemy.Controller;
using Yueyn.Fsm;

namespace GenBall.Enemy.AI
{
    public class AIIdleState : EnemyAIStateBase
    {
        protected internal override void OnEnter(Fsm<EnemyAIController> fsm)
        {
            base.OnEnter(fsm);
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyAIController> fsm, float fixeDeltaTime)
        {
            if (Detect.HasTarget && Detect.InDetectRange)
            {
                ChangeState<AIChaseState>();
            }
        }
    }
}
