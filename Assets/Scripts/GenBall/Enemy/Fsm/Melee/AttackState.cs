using GenBall.Enemy.Attack;
using GenBall.Enemy.Detect;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class AttackState : BaseState
    {
        private DetectModule _detectModule;
        private AttackModule _attackModule;
        protected internal override void OnEnter(Fsm<EnemyBase> fsm)
        {
            base.OnEnter(fsm);
            _detectModule = GetModule<DetectModule>();
            _attackModule = GetModule<AttackModule>();
            
            _attackModule.StartAttack();
            GetData<Variable<int>>("Health").Observe(DefaultOnHeathChanged);
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyBase> fsm, float fixeDeltaTime)
        {
            if (!_attackModule.CanAttack())
            {
                ChangeState<ChaseState>();
            }
        }

        protected internal override void OnExit(Fsm<EnemyBase> fsm, bool isShutdown = false)
        {
            _attackModule.StopAttack();
            GetData<Variable<int>>("Health").Unobserve(DefaultOnHeathChanged);
        }
    }
}