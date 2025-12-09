using GenBall.Enemy.Detect;
using GenBall.Enemy.Move;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class WanderState : BaseState
    {
        private Fsm<EnemyEntity> _fsm;
        private DetectModule _detectModule;
        private Variable<Player.Player> _target;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            _fsm = fsm;
            _detectModule = _fsm.Owner.GetModule<DetectModule>();
            _target = _fsm.GetData<Variable<Player.Player>>("Target");
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyEntity> fsm, float fixeDeltaTime)
        {
            _detectModule.Search(OnFindTarget);
        }

        private void OnFindTarget(Player.Player target)
        {
            _target.PostValue(target);
            // todo gzp 切换到追击状态
            Debug.Log($"发现目标{target.name}");
        }
    }
}