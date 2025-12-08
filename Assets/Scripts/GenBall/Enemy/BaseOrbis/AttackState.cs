using System;
using GenBall.BattleSystem;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public class AttackState:OrbisStateBase
    {
        private Variable<bool> _inAttackRadius;

        protected override void GetDatas()
        {
            base.GetDatas();
            _inAttackRadius = Fsm.GetData<Variable<bool>>("InAttackRadius");
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            _inAttackRadius.Observe(InAttackRadiusChange);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            _inAttackRadius.Unobserve(InAttackRadiusChange);
        }

        private void InAttackRadiusChange(bool inAttackRadius)
        {
            if (!inAttackRadius)
            {
                Fsm.ChangeState<ChaseState>();
            }
        }
    }
}