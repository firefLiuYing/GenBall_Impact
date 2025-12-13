using GenBall.Enemy.Detect;
using GenBall.Enemy.Move;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class WanderState : BaseState
    {
        private DetectModule _detectModule;
        private Variable<Player.Player> _target;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            base.OnEnter(fsm);
            _detectModule =GetModule<DetectModule>();
            _target =GetData<Variable<Player.Player>>("Target");
            GetData<Variable<int>>("Health").Observe(DefaultOnHeathChanged);
        }

        protected internal override void OnExit(Fsm<EnemyEntity> fsm, bool isShutdown = false)
        {
            base.OnExit(fsm, isShutdown);
            GetData<Variable<int>>("Health").Unobserve(DefaultOnHeathChanged);
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyEntity> fsm, float fixeDeltaTime)
        {
            _detectModule.Search(OnFindTarget);
        }

        private void OnFindTarget(Player.Player target)
        {
            _target.PostValue(target);
            ChangeState<ChaseState>();
        }
    }
}