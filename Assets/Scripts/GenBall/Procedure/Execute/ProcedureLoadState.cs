using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GenBall.Enemy;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Procedure.Execute
{
    public class ProcedureLoadState : FsmState<ExecuteComponent>
    {
        protected internal override void OnEnter(Fsm<ExecuteComponent> fsm)
        {
            RegisterEvents();
            GameEntry.UI.OpenForm<StartForm>();
        }

        protected internal override void OnExit(Fsm<ExecuteComponent> fsm, bool isShutdown = false)
        {
            UnRegisterEvents();
        }


        private void RegisterEvents()
        {
            
        }

        private void UnRegisterEvents()
        {
            
        }
        
    }
}