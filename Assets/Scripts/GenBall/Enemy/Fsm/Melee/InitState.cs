using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class InitState : BaseState
    {
        protected internal override void OnUpdate(Fsm<EnemyEntity> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeState<WanderState>();
        }
    }
}