using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class BackState : BaseState
    {
        protected internal override void OnUpdate(Fsm<EnemyEntity> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeState<WanderState>();
        }
    }
}