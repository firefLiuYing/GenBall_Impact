using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.Enemy.AI;
using GenBall.Enemy.Controller;
using UnityEngine;

namespace GenBall.Enemy
{
    public static class EnemyEntityFactory
    {
        public static void AssembleEnemy(GameObject enemyInstance, EnemyAIConfigSo aiConfig)
        {
            // 1. Get or add BattleEntity
            var entity = enemyInstance.GetComponent<BattleEntity>();
            if (entity == null)
                entity = enemyInstance.AddComponent<BattleEntity>();

            // 2. Find existing MonoBehaviours
            var enemyMover = enemyInstance.GetComponent<EnemyMover>();
            var attackController = enemyInstance.GetComponentInChildren<EnemyAttackController>();
            var faceController = enemyInstance.GetComponentInChildren<EnemyFaceController>();
            var detectController = enemyInstance.GetComponentInChildren<EnemyDetectController>();

            // 3. Create framework components
            var stats = new StatComponent();
            stats.GetOrCreate("MaxHealth", 100f);

            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new SimpleDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 4. Register executors on dispatcher
            if (enemyMover != null)
                dispatcher.RegisterExecutor<MoveCommand>(enemyMover);
            if (attackController != null)
                dispatcher.RegisterExecutor<AttackCommand>(attackController);
            if (faceController != null)
                dispatcher.RegisterExecutor<FaceDirectionCommand>(faceController);

            // 5. Create decision layer with AI config
            var decisionLayer = new EnemyDecisionLayer(entity, aiConfig);
            decisionLayer.Dispatcher = dispatcher;

            // 6. Register everything on BattleEntity
            entity.RegisterComponent(stats);
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(decisionLayer);
        }
    }
}
