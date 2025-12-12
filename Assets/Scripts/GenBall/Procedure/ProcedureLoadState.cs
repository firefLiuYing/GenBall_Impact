using GenBall.Enemy;
using GenBall.Player;
using GenBall.UI;
using GenBall.Utils.EntityCreator;
using UnityEngine;
using Yueyn.Fsm;

namespace GenBall.Procedure
{
    public class ProcedureLoadState : FsmState<ExecuteProcedure>
    {
        protected internal override void OnEnter(Fsm<ExecuteProcedure> fsm)
        {
            LoadMainHud();
            LoadPlayer();
            LoadEnemy();
        }

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
            var orbis= enemyCreator.CreateEntity<EnemyEntity>("NormalOrbis",new Vector3(0,0.5f,15), Quaternion.identity);
            orbis.Initialize();
            
        }
    }
}