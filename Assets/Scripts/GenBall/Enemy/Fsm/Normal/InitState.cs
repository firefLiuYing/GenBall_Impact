using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class InitState : BaseState
    {
        private Fsm<EnemyEntity> _fsm;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            _fsm = fsm;
        }

        protected internal override void OnUpdate(Fsm<EnemyEntity> fsm, float elapsedTime, float realElapseTime)
        {
            _fsm.ChangeState<WanderState>();
        }
    }
}