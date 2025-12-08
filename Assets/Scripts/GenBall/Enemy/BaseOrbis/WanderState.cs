using System;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public class WanderState : OrbisStateBase
    {
        private Variable<IInteractable> _target;
        protected override void GetDatas()
        {
            base.GetDatas();
            _target = Fsm.GetData<Variable<IInteractable>>("Target");
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            _target.Observe(OnTargetChange);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            _target.Unobserve(OnTargetChange);
        }

        private void OnTargetChange(IInteractable target)
        {
            if (target != null)
            {
                Fsm.ChangeState<ChaseState>();
            }
        }
    }
}