using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using GenBall.Enemy;
using GenBall.Player;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Procedure.Execute
{
    public class ProcedureLoadState : FsmState<ExecuteProcedure>
    {
        protected internal override void OnEnter(Fsm<ExecuteProcedure> fsm)
        {
            LoadMainHud();
            LoadPlayer();
            // LoadEnemy();
            _ = LoadEnemys(_cancellationTokenSource.Token);
        }

        protected internal override void OnExit(Fsm<ExecuteProcedure> fsm, bool isShutdown = false)
        {
            base.OnExit(fsm, isShutdown);
            
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private void LoadPlayer()
        {
            GameEntry.GetModule<PlayerManager>().CreatePlayer();
        }

        private void LoadMainHud()
        {
            GameEntry.GetModule<UIManager>().OpenForm<MainHud>();
        }

        private void LoadEnemy()
        {
            var enemyCreator= GameEntry.GetModule<EntityCreator<IEnemy>>();
            var orbis= enemyCreator.CreateEntity<NormalOrbis>("NormalOrbis",new Vector3(0,0.5f,15), Quaternion.identity);
            orbis.Initialize();
            
        }

        private async Task LoadEnemys(CancellationToken token)
        {
            try
            {
                while(!token.IsCancellationRequested)
                {
                    LoadEnemy();
                    await Task.Delay(10000, token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("刷新敌人已取消");
            }
        }
    }
}