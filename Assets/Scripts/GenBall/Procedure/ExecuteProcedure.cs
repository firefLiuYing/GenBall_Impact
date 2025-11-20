using System.Collections.Generic;
using Yueyn.Fsm;

namespace GenBall.Procedure
{
    public sealed class ExecuteProcedure
    {
        private Fsm<ExecuteProcedure> _fsm;
        private readonly List<FsmState<ExecuteProcedure>> _states=new();

        public void Start()
        {
            _fsm.Start<ProcedureLoadState>();
        }

        public void Init()
        {
            RegisterStates();
            _fsm = GameEntry.GetModule<FsmManager>().CreateFsm(this,_states);
        }
        private void RegisterStates()
        {
            _states.Add(new ProcedureLoadState());
        }

    }
}