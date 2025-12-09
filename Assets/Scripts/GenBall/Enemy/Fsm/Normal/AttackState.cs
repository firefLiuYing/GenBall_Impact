using GenBall.Enemy.Detect;
using Yueyn.Fsm;

namespace GenBall.Enemy.Fsm.Normal
{
    public class AttackState : BaseState
    {
        private DetectModule _detectModule;
        protected internal override void OnEnter(Fsm<EnemyEntity> fsm)
        {
            base.OnEnter(fsm);
            _detectModule = GetModule<DetectModule>();
        }

        protected internal override void OnFixedUpdate(Fsm<EnemyEntity> fsm, float fixeDeltaTime)
        {
            // todo gzp ¹¥»÷Âß¼­£¬ÏÈ²»×ö¹¥»÷£¬ÏÈÐ´³ÉÍË³ö¹¥»÷·¶Î§¾ÍÍÑÀë¹¥»÷Ì¬
            if (!_detectModule.InAttackRange())
            {
                ChangeState<ChaseState>();
            }
        }
    }
}