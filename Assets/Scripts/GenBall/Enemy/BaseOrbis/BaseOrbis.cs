using System;
using System.Collections.Generic;
using GenBall.BattleSystem;
using UnityEngine;
using Yueyn.Base.ReferencePool;
using Yueyn.Base.Variable;
using Yueyn.Fsm;

namespace GenBall.Enemy.BaseOrbis
{
    public abstract partial class BaseOrbis : MonoBehaviour,IEnemy
    {
        private Fsm<BaseOrbis> _baseAiFsm;
        private readonly List<FsmState<BaseOrbis>> _aiStates = new();
        private Variable<IInteractable> _targetVariable;
        protected Variable<bool> InAttackRadius;
        protected Variable<bool> InWanderRadius;
        protected Variable<IInteractToken[]> ResponseTokens;
        protected Variable<IInteractToken> StimulusToken;

        #region Callbacks

        protected OrbisStateBase.UpdateCallback WanderUpdateCallback;
        protected OrbisStateBase.UpdateCallback ChaseUpdateCallback;
        protected OrbisStateBase.UpdateCallback ReturnUpdateCallback;
        protected OrbisStateBase.UpdateCallback AttackUpdateCallback;
        
        protected OrbisStateBase.UpdateCallback WanderFixedUpdateCallback;
        protected OrbisStateBase.UpdateCallback ChaseFixedUpdateCallback;
        protected OrbisStateBase.UpdateCallback ReturnFixedUpdateCallback;
        protected OrbisStateBase.UpdateCallback AttackFixedUpdateCallback;
        
        protected OrbisStateBase.HandleDelegate WanderHandle;
        protected OrbisStateBase.HandleDelegate ChaseHandle;
        protected OrbisStateBase.HandleDelegate ReturnHandle;
        protected OrbisStateBase.HandleDelegate AttackHandle;

        #endregion

        protected void StartFsm()
        {
            _baseAiFsm.Start<WanderState>();
        }

        protected void ShutdownFsm()
        {
            _baseAiFsm.Shutdown();
        }
        public IInteractable Target { get; protected set; }

        protected internal virtual void SetTarget(IInteractable target)
        {
            Target = target;
            _targetVariable.PostValue(target);
        }
        public virtual void Handle(IInteractToken stimulus, out IInteractToken[] responses)
        {
            StimulusToken.PostValue(stimulus);
            responses = ResponseTokens.Value ?? Array.Empty<IInteractToken>();
        }

        public virtual void EntityUpdate(float deltaTime)
        {
            
        }

        public virtual void EntityFixedUpdate(float fixedDeltaTime)
        {
            
        }

        public virtual void OnRecycle()
        {
            _baseAiFsm.Shutdown();
            _aiStates.Clear();
        }

        protected virtual void SetCallbacks()
        {
            WanderUpdateCallback = DefaultWanderUpdate;
            WanderFixedUpdateCallback = DefaultWanderFixedUpdate;
            WanderHandle = DefaultWanderHandle;

            ChaseUpdateCallback = DefaultChaseUpdate;
            ChaseFixedUpdateCallback = DefaultChaseFixedUpdate;
            ChaseHandle = DefaultChaseHandle;

            AttackUpdateCallback = DefaultAttackUpdate;
            AttackFixedUpdateCallback = DefaultAttackFixedUpdate;
            AttackHandle = DefaultAttackHandle;

            ReturnUpdateCallback = DefaultReturnUpdate;
            ReturnFixedUpdateCallback = DefaultReturnFixedUpdate;
            ReturnHandle = DefaultReturnHandle;
        }
        public virtual void Initialize()
        {
            _rigidbody=GetComponent<Rigidbody>();
            _aiStates.Clear();
            SetCallbacks();
            _aiStates.Add(new WanderState()
            {
                StateUpdateCallback = WanderUpdateCallback,
                StateFixedUpdateCallback = WanderFixedUpdateCallback,
                StateHandle = WanderHandle,
            });
            _aiStates.Add(new ChaseState()
            {
                StateUpdateCallback = ChaseUpdateCallback,
                StateFixedUpdateCallback = ChaseFixedUpdateCallback,
                StateHandle = ChaseHandle,
            });
            _aiStates.Add(new AttackState()
            {
                StateUpdateCallback = AttackUpdateCallback,
                StateFixedUpdateCallback = AttackFixedUpdateCallback,
                StateHandle = AttackHandle,
            });
            _aiStates.Add(new ReturnState()
            {
                StateUpdateCallback = ReturnUpdateCallback,
                StateFixedUpdateCallback = ReturnFixedUpdateCallback,
                StateHandle = ReturnHandle,
            });
            _baseAiFsm = GameEntry.GetModule<FsmManager>().CreateFsm(this, _aiStates);
            RegisterFsmDatas();
        }

        private void RegisterFsmDatas()
        {
            _targetVariable = ReferencePool.Acquire<Variable<IInteractable>>();
            _baseAiFsm.SetData("Target",_targetVariable);
            InAttackRadius=ReferencePool.Acquire<Variable<bool>>();
            _baseAiFsm.SetData("InAttackRadius",InAttackRadius);
            InWanderRadius=ReferencePool.Acquire<Variable<bool>>();
            _baseAiFsm.SetData("InWanderRadius",InWanderRadius);
            ResponseTokens = ReferencePool.Acquire<Variable<IInteractToken[]>>();
            _baseAiFsm.SetData("ResponseTokens",ResponseTokens);
            StimulusToken = ReferencePool.Acquire<Variable<IInteractToken>>();
            _baseAiFsm.SetData("StimulusToken",StimulusToken);
        }
    }
}