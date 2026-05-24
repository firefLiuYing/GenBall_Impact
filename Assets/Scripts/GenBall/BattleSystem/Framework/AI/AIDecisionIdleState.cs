using Yueyn.Fsm;

namespace GenBall.BattleSystem.Framework.AI
{
    public class AIDecisionIdleState : EnemyDecisionStateBase
    {
        protected internal override void OnEnter(Fsm<EnemyDecisionLayer> fsm)
        {
            base.OnEnter(fsm);
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyDecisionLayer> fsm, float fixedDeltaTime)
        {
            if (Detect.HasTarget && Detect.InDetectRange)
            {
                ChangeState<AIDecisionChaseState>();
            }
        }
    }
}
