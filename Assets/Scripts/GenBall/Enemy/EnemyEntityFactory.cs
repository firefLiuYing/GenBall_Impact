using GenBall.BattleSystem;
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

            // 3. Create StatComponent and set ALL initial stats (no EventDispatcher yet → no events)
            var stats = new StatComponent(entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("CurrentHealth", 100f);
            stats.GetOrCreate("Attack", 10f);
            stats.GetOrCreate("MoveSpeed", 3f);

            // 4. Create EventDispatcherComponent — events from this point onward
            var eventDispatcher = new EventDispatcherComponent(entity);

            // 5. Create framework components
            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new SimpleDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 6. Register executors on dispatcher
            if (enemyMover != null)
                dispatcher.RegisterExecutor<MoveCommand>(enemyMover);
            if (attackController != null)
                dispatcher.RegisterExecutor<AttackCommand>(attackController);
            if (faceController != null)
                dispatcher.RegisterExecutor<FaceDirectionCommand>(faceController);

            // 7. Create decision layer with AI config
            var decisionLayer = new EnemyDecisionLayer(entity, aiConfig);
            decisionLayer.Dispatcher = dispatcher;

            // 8. Create DeathComponent
            var deathComponent = new DeathComponent(entity, new EnemyDeathHandler());

            // 9. Register everything on BattleEntity
            entity.RegisterComponent(eventDispatcher);
            entity.RegisterComponent(stats);
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(decisionLayer);
            entity.RegisterComponent(deathComponent);
        }
    }

    /// <summary>
    /// Placeholder: enemy-specific death behavior (death animation, loot, despawn).
    /// </summary>
    internal class EnemyDeathHandler : IDeathHandler
    {
        public void OnDeath(BattleSystem.DeathInfo deathInfo)
        {
            Debug.Log($"[EnemyDeath] Enemy died: {deathInfo.Victim.name}");
            // TODO Phase D: death animation → loot drop → despawn
        }
    }
}
