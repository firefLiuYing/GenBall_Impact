using System;
using GenBall.BattleSystem;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public class ChaseState:OrbisStateBase
    {
        private Variable<bool> _inAttackRadius;
        private Variable<bool> _inWanderRadius;
        private Variable<IInteractable> _target;

        protected override void GetDatas()
        {
            base.GetDatas();
            _inAttackRadius = Fsm.GetData<Variable<bool>>("InAttackRadius");
            _inWanderRadius = Fsm.GetData<Variable<bool>>("InWanderRadius");
            _target = Fsm.GetData<Variable<IInteractable>>("Target");
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            _inAttackRadius.Observe(InAttackRadiusChange);
            _target.Observe(OnTargetChange);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            _inAttackRadius.Unobserve(InAttackRadiusChange);
            _target.Unobserve(OnTargetChange);
        }

        private void InAttackRadiusChange(bool inAttackRadius)
        {
            if (inAttackRadius)
            {
                Fsm.ChangeState<AttackState>();
            }
        }

        private void OnTargetChange(IInteractable target)
        {
            if (target == null&&_inWanderRadius.Value)
            {
                Fsm.ChangeState<WanderState>();
            }
            else if (target == null)
            {
                Fsm.ChangeState<ReturnState>();
            }
        }

    }
}