using System;
using GenBall.BattleSystem;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public class ReturnState : OrbisStateBase
    {
        private Variable<bool> _inWanderRadius;
        protected override void GetDatas()
        {
            base.GetDatas();
            _inWanderRadius=Fsm.GetData<Variable<bool>>("InWanderRadius");
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            _inWanderRadius.Observe(InWanderRadiusChange);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            _inWanderRadius.Unobserve(InWanderRadiusChange);
        }

        private void InWanderRadiusChange(bool inWanderRadius)
        {
            if (inWanderRadius)
            {
                Fsm.ChangeState<WanderState>();
            }
        }

    }
}