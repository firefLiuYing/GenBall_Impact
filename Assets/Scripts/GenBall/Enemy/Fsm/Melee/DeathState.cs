using GenBall.Enemy.Hurt;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Melee
{
    public class DeathState : BackState
    {
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            base.OnEnter(fsm);
            Fsm.Owner.Death();
        }
    }
}