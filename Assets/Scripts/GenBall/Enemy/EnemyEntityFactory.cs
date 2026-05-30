using GenBall.BattleSystem;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Navigation;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Enemy.AI;
using GenBall.Enemy.Attack;
using GenBall.Enemy.Detect;
using GenBall.Enemy.Executor;
using GenBall.Enemy.Visual;
using UnityEngine;
using Yueyn.Main;

namespace GenBall.Enemy
{
    public static class EnemyEntityFactory
    {
        public static void AssembleEnemy(GameObject enemyInstance, EnemyConfigSo config, EnemyAIConfigSo aiConfig)
        {
            // 1. Get or add BattleEntity
            var entity = enemyInstance.GetComponent<BattleEntity>();
            if (entity == null)
                entity = enemyInstance.AddComponent<BattleEntity>();

            // 2. Find existing MB components on the GameObject
            var mover = enemyInstance.GetComponent<RigidbodyMover>();
            var groundDetect = enemyInstance.GetComponent<ICharacterGroundDetect>();

            // Freeze all rotation — sphere doesn't roll, facing handled by RigidbodyMover.SetRotation
            var rb = enemyInstance.GetComponent<Rigidbody>();
            if (rb != null)
                rb.constraints = RigidbodyConstraints.FreezeRotation;

            var attackTrigger = enemyInstance.GetComponentInChildren<AttackTrigger>();
            attackTrigger?.Init(enemyInstance);
            var barrier = enemyInstance.GetComponentInChildren<Barrier>();

            // Get or add DirectNavigator (as INavigator)
            var navigator = enemyInstance.GetComponent<DirectNavigator>();
            if (navigator == null)
                navigator = enemyInstance.AddComponent<DirectNavigator>();

            // 3. Create EnemyDetector with target layer and ranges from config
            var detector = new EnemyDetector(
                enemyInstance.transform,
                LayerMask.GetMask("Player"),
                config.detectRange,
                config.hateRange,
                config.attackRange);

            // Register detector early — EnemyDecisionLayer constructor reads it from entity
            entity.RegisterComponent(detector);

            // 4. Create StatComponent and set ALL initial stats from config
            var stats = new StatComponent(entity);
            stats.GetOrCreate("MaxHealth", config.maxHealth);
            stats.GetOrCreate("CurrentHealth", config.maxHealth);
            stats.GetOrCreate("Attack", config.attackDamage);
            stats.GetOrCreate("MoveSpeed", config.moveSpeed);
            entity.RegisterComponent(stats);

            // 5. Create EventDispatcherComponent — events from this point onward
            var eventDispatcher = new EventDispatcherComponent(entity);
            entity.RegisterComponent(eventDispatcher);

            // 6. Create framework components
            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new SimpleDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 7. Create executors (all from config)
            var jumpMoveExecutor = new EnemyJumpMoveExecutor(mover, config, groundDetect, navigator);
            var gravityExecutor = new EnemyGravityExecutor(mover, config, dispatcher, groundDetect);
            var jumpExecutor = new EnemyJumpExecutor(mover, config.jumpForce);
            var faceExecutor = new EnemyFaceExecutor(enemyInstance.transform, 720f);

            // Dash executor — only if both barrier and attackTrigger exist
            EnemyDashExecutor dashExecutor = null;
            if (barrier != null && attackTrigger != null)
            {
                dashExecutor = new EnemyDashExecutor(mover, entity, config, detector, attackTrigger, barrier);
            }

            // Create visual squash & stretch (operates on "Visual" child transform)
            var visualTransform = enemyInstance.transform.Find("Visual");
            if (visualTransform != null)
            {
                var squashStretch = new SquashStretchVisual(visualTransform, mover, groundDetect, config, entity);
                entity.RegisterComponent(squashStretch);
            }

            // 8. Register executors on CommandDispatcher
            dispatcher.RegisterExecutor<MoveCommand>(jumpMoveExecutor);
            dispatcher.RegisterExecutor<JumpCommand>(jumpExecutor);
            dispatcher.RegisterExecutor<FaceDirectionCommand>(faceExecutor);
            if (dashExecutor != null)
                dispatcher.RegisterExecutor<AttackCommand>(dashExecutor);

            // 9. Register executors on entity BEFORE decision layer (it reads them)
            entity.RegisterComponent(jumpMoveExecutor);
            entity.RegisterComponent(gravityExecutor);
            entity.RegisterComponent(jumpExecutor);
            entity.RegisterComponent(faceExecutor);
            if (dashExecutor != null)
            {
                entity.RegisterComponent(dashExecutor);
                entity.RegisterComponentAs<IAttack>(dashExecutor);
            }

            // 10. Create decision layer with AI config (now can find dashExecutor via entity.Get)
            var decisionLayer = new EnemyDecisionLayer(entity, aiConfig);
            decisionLayer.Dispatcher = dispatcher;
            entity.RegisterComponent(decisionLayer);

            // 11. Create HitReactionComponent (following player pattern)
            var hitReaction = new HitReactionComponent(dispatcher, eventDispatcher, stunDuration: 0.3f);
            entity.RegisterComponent(hitReaction);

            // 12. Create DeathComponent with enemy-specific death handler
            var deathComponent = new DeathComponent(entity, new EnemyDeathHandler(config));
            entity.RegisterComponent(deathComponent);

            // 13. Register remaining components
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponentAs<IDamageable>(damageReceiver);
            entity.RegisterComponentAs<IHealth>(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(detector);
        }
    }

    /// <summary>
    /// Enemy-specific death behavior: logs death info and destroys the GameObject.
    /// </summary>
    internal class EnemyDeathHandler : IDeathHandler
    {
        private readonly EnemyConfigSo _config;

        public EnemyDeathHandler(EnemyConfigSo config)
        {
            _config = config;
        }

        public void OnDeath(BattleSystem.DeathInfo deathInfo)
        {
            Debug.Log($"[Enemy] {deathInfo.Victim.name} died, killPoints={_config.killPoints}");

            // Award kill points to the player via the evolution system
            var evoSystem = SystemRepository.Instance.GetSystem<IEvolutionSystem>();
            evoSystem?.AddKillPoints(_config.killPoints);

            // TODO Phase E: integrate with CPoolManager for proper recycle
            deathInfo.Victim.SetActive(false);
        }
    }
}
