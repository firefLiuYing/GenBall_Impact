using GenBall.Enemy.Attack;
using GenBall.Enemy.Detect;
using GenBall.Enemy.Move;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class ChaseState : BaseState
    {
        private Fsm<EnemyEntity> _fsm;
        private DetectModule _detectModule;
        private MoveModule _moveModule;
        private AttackModule _attackModule;
        private Variable<Player.Player> _target;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            base.OnEnter(fsm);
            _detectModule = GetModule<DetectModule>();
            _moveModule = GetModule<MoveModule>();
            _attackModule = GetModule<AttackModule>();
            _target=GetData<Variable<Player.Player>>("Target");
            GetData<Variable<int>>("Health").Observe(DefaultOnHeathChanged);
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyEntity> fsm, float fixeDeltaTime)
        {
            if (!_detectModule.InHateRange())
            {
                _target.PostValue(null);
                ChangeState<BackState>();
                return;
            }

            if (_attackModule.CanAttack())
            {
                ChangeState<AttackState>();
                return;
            }
            
            _moveModule.MoveTo(_target.Value.transform.position);
        }

        protected internal override void OnExit(Fsm<EnemyEntity> fsm, bool isShutdown = false)
        {
            _moveModule.StopMove();
            GetData<Variable<int>>("Health").Unobserve(DefaultOnHeathChanged);
        }
    }
}