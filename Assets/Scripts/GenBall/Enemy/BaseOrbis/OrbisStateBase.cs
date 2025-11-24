using System;
using GenBall.BattleSystem;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public abstract class OrbisStateBase:FsmState<BaseOrbis>
    {
        protected Fsm<BaseOrbis> Fsm;
        public delegate void UpdateCallback(float deltaTime);
        public delegate void HandleDelegate(IInteractToken stimulus, out IInteractToken[] responses);
        public UpdateCallback StateUpdateCallback;
        public UpdateCallback StateFixedUpdateCallback;
        public HandleDelegate StateHandle;
        protected Variable<IInteractToken> StimulusToken;
        protected Variable<IInteractToken[]> ResponseTokens;

        protected internal override void OnEnter(Fsm<BaseOrbis> fsm)
        {
            Fsm = fsm;
            GetDatas();
            RegisterEvents();
        }

        protected internal override void OnExit(Fsm<BaseOrbis> fsm, bool isShutdown = false)
        {
            UnregisterEvents();
        }

        protected internal override void OnUpdate(Fsm<BaseOrbis> fsm, float elapsedTime, float realElapseTime)
        {
            StateUpdateCallback?.Invoke(elapsedTime);
        }
        protected internal override void OnFixedUpdate(Fsm<BaseOrbis> fsm, float fixeDeltaTime)
        {
            StateFixedUpdateCallback?.Invoke(fixeDeltaTime);
        }

        protected virtual void GetDatas()
        {
            StimulusToken=Fsm.GetData<Variable<IInteractToken>>("StimulusToken");
            ResponseTokens=Fsm.GetData<Variable<IInteractToken[]>>("ResponseTokens");
        }

        protected virtual void RegisterEvents()
        {
            StimulusToken.Observe(OnStimulusTokenChange);
        }

        protected virtual void UnregisterEvents()
        {
            StimulusToken.Unobserve(OnStimulusTokenChange);
        }

        protected virtual void OnStimulusTokenChange(IInteractToken stimulus)
        {
            if (StateHandle == null)
            {
                ResponseTokens.SetValue(Array.Empty<IInteractToken>());
                return;
            }
            StateHandle.Invoke(stimulus, out IInteractToken[] responses);
            ResponseTokens.SetValue(responses);
        }
    }
}