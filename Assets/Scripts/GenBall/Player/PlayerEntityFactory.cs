using GenBall.BattleSystem;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Player.Controller;
using GenBall.Player.Executor;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player
{
    public static class PlayerEntityFactory
    {
        public static void AssemblePlayer(GameObject playerInstance, PlayerConfig config)
        {
            // 1. Get or add BattleEntity component
            var entity = playerInstance.GetComponent<BattleEntity>();
            if (entity == null)
                entity = playerInstance.AddComponent<BattleEntity>();

            // 2. Find MonoBehaviours on playerInstance
            var mover = playerInstance.GetComponent<RigidbodyMover>();
            var rigidbody = mover.GetComponent<Rigidbody>();
            var playerMover = playerInstance.GetComponent<PlayerMover>();
            var inputHandler = playerInstance.GetComponentInChildren<InputHandler>();
            var groundDetect = playerInstance.GetComponent<ICharacterGroundDetect>()
                               ?? playerInstance.GetComponentInChildren<ICharacterGroundDetect>();
            var weaponController = playerInstance.GetComponentInChildren<WeaponController>();
            var rotater = playerInstance.GetComponent<IRotate>();

            // 3. Create StatComponent and set ALL initial stats (no EventDispatcher yet → no events)
            var stats = new StatComponent(entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("CurrentHealth", 100f);
            stats.GetOrCreate("Attack", 10f);
            stats.GetOrCreate("MoveSpeed", 5f);
            stats.GetOrCreate("MaxShield", 100f);
            stats.GetOrCreate("Shield", 100f);

            // 4. Create EventDispatcherComponent — events from this point onward
            var eventDispatcher = new EventDispatcherComponent(entity);

            // 5. Create framework components
            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new WeaponDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 6. Create executors
            var jumpExecutor = new PlayerJumpExecutor(rigidbody, playerMover, config, inputHandler, groundDetect);
            var dashExecutor = new PlayerDashExecutor(rigidbody, playerMover, config, entity);
            var attackExecutor = new PlayerAttackExecutor(weaponController);

            // 7. Register executors on dispatcher
            dispatcher.RegisterExecutor<MoveCommand>(playerMover);
            dispatcher.RegisterExecutor<JumpCommand>(jumpExecutor);
            dispatcher.RegisterExecutor<DashCommand>(dashExecutor);
            dispatcher.RegisterExecutor<AttackCommand>(attackExecutor);
            if (rotater != null)
                dispatcher.RegisterExecutor<RotateCommand>(rotater);

            // 8. Create input adapter and decision layer
            var inputAdapter = new PlayerInputAdapter(inputHandler);
            var decisionLayer = new PlayerDecisionLayer(entity, inputAdapter);
            decisionLayer.Dispatcher = dispatcher;

            // 9. Create DeathComponent
            var deathComponent = new DeathComponent(entity, new PlayerDeathHandler());

            // 10. Register everything on BattleEntity
            entity.RegisterComponent(eventDispatcher);
            entity.RegisterComponent(stats);
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(jumpExecutor);
            entity.RegisterComponent(dashExecutor);
            entity.RegisterComponent(attackExecutor);
            entity.RegisterComponent(decisionLayer);
            entity.RegisterComponent(deathComponent);
        }
    }

    /// <summary>
    /// Placeholder: player-specific death behavior (respawn, game-over flow).
    /// </summary>
    internal class PlayerDeathHandler : IDeathHandler
    {
        public void OnDeath(DeathInfo deathInfo)
        {
            Debug.Log($"[PlayerDeath] Player died. Killer: {deathInfo.Killer}");
            // TODO Phase D: death animation → respawn flow → game-over UI
        }
    }
}
