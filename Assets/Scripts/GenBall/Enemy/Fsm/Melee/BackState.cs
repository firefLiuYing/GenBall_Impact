using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class BackState : BaseState
    {
        protected internal override void OnEnter(Fsm<EnemyBase> fsm)
        {
            base.OnEnter(fsm);
            GetData<Variable<int>>("Health").Observe(DefaultOnHeathChanged);
        }

        protected internal override void OnExit(Fsm<EnemyBase> fsm, bool isShutdown = false)
        {
            base.OnExit(fsm, isShutdown);
            GetData<Variable<int>>("Health").Unobserve(DefaultOnHeathChanged);
        }

        protected internal override void OnUpdate(Fsm<EnemyBase> fsm, float elapsedTime, float realElapseTime)
        {
            ChangeState<WanderState>();
        }
    }
}