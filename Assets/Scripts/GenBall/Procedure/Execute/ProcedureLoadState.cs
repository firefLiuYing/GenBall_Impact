using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GenBall.Enemy;
using GenBall.Player;
using GenBall.Procedure.Game;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Procedure.Execute
{
    public class ProcedureLoadState : FsmState<ExecuteProcedure>
    {
        protected internal override async void OnEnter(Fsm<ExecuteProcedure> fsm)
        {
            try
            {
                await Task.Delay(1);
                // todo gzp 模拟一下可能的异步方法
                GameEntry.UI.OpenForm<StartForm>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}