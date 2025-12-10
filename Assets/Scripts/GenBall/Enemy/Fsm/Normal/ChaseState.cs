using GenBall.Enemy.Detect;
using GenBall.Enemy.Move;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class ChaseState : BaseState
    {
        private Fsm<EnemyEntity> _fsm;
        private DetectModule _detectModule;
        private MoveModule _moveModule;
        private Variable<Player.Player> _target;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            base.OnEnter(fsm);
            _detectModule = GetModule<DetectModule>();
            _moveModule = GetModule<MoveModule>();
            _target=GetData<Variable<Player.Player>>("Target");
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyEntity> fsm, float fixeDeltaTime)
        {
            if (!_detectModule.InReversoRange())
            {
                _target.PostValue(null);
                ChangeState<BackState>();
                return;
            }

            if (_detectModule.InAttackRange())
            {
                ChangeState<AttackState>();
                return;
            }
            
            _moveModule.MoveTo(_target.Value.transform.position);
        }

        protected internal override void OnExit(Fsm<EnemyEntity> fsm, bool isShutdown = false)
        {
            _moveModule.StopMove();
        }
    }
}