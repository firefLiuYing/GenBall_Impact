using GenBall.BattleSystem;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.GameCamera;
using GenBall.Interact;
using GenBall.Player.Executor;
using GenBall.Player.Input;
using UnityEngine;
using Yueyn.Main;

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
            var playerMoveExecutor = new PlayerMoveExecutor();
            playerMoveExecutor.Init(mover, config.speed);
            var inputHandler = playerInstance.GetComponentInChildren<InputHandler>();
            var groundDetect = playerInstance.GetComponent<ICharacterGroundDetect>()
                               ?? playerInstance.GetComponentInChildren<ICharacterGroundDetect>();
            var weaponExecutor = playerInstance.GetComponentInChildren<WeaponExecutor>();
            weaponExecutor.Init(playerInstance);
            var rotater = playerInstance.GetComponent<PlayerRotateExecutor>();
            rotater.Init(config.horizontalSensitivity, config.verticalSensitivity);
            // MainCameraTransform/FirstPersonCamera is a structural guarantee on the prefab.
            var mainCameraArm = playerInstance.transform.Find("MainCameraTransform");
            var firstPersonCamera = mainCameraArm != null ? mainCameraArm.GetComponentInChildren<Camera>() : null;

            // 3. Create StatComponent and set ALL initial stats
            var stats = new StatComponent(entity);
            stats.GetOrCreate("MaxHealth", 100f);
            stats.GetOrCreate("CurrentHealth", 100f);
            stats.GetOrCreate("Attack", 10f);
            stats.GetOrCreate("MoveSpeed", 5f);
            stats.GetOrCreate("MaxShield", 100f);
            stats.GetOrCreate("Shield", 100f);

            // 4. Create EventDispatcherComponent
            var eventDispatcher = new EventDispatcherComponent(entity);

            // 5. Create framework components
            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new WeaponDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 6. Create executors
            // Jump executor — no longer depends on InputHandler
            var jumpExecutor = new PlayerJumpExecutor(rigidbody, mover, config, groundDetect);
            var dashExecutor = new PlayerDashExecutor(rigidbody, mover, config, entity);

            // Gravity executor — uses RigidbodyMover for pause-safe velocity writes
            var gravityExecutor = new PlayerGravityExecutor(rigidbody, mover, groundDetect, config);

            // Interact executor
            var camera = firstPersonCamera;
            var interactSystem = SystemRepository.Instance.GetSystem<IInteractSystem>();
            var interactExecutor = new PlayerInteractExecutor(interactSystem, camera,
                config.sightDetectRadius, config.sightDetectDistance, config.interactableLayer);

            // 7. Register executors on dispatcher
            dispatcher.RegisterExecutor<MoveCommand>(playerMoveExecutor);
            dispatcher.RegisterExecutor<JumpCommand>(jumpExecutor);
            dispatcher.RegisterExecutor<DashCommand>(dashExecutor);
            dispatcher.RegisterExecutor<AttackCommand>(weaponExecutor);
            dispatcher.RegisterExecutor<ReloadCommand>(weaponExecutor);
            dispatcher.RegisterExecutor<SwitchWeaponCommand>(weaponExecutor);
            dispatcher.RegisterExecutor<InteractCommand>(interactExecutor);
            if (rotater != null)
                dispatcher.RegisterExecutor<RotateCommand>(rotater);

            // 8. Create input adapter and decision layer (event-driven)
            var inputAdapter = new PlayerInputAdapter(inputHandler);
            var decisionLayer = new PlayerDecisionLayer(entity, inputAdapter);
            decisionLayer.Dispatcher = dispatcher;

            // 9. Create DeathComponent
            var deathComponent = new DeathComponent(entity, new PlayerDeathHandler());

            // 9.5. Create HitReactionComponent
            var hitReaction = new HitReactionComponent(dispatcher, eventDispatcher, stunDuration: 0.3f);

            // 10. Register everything on BattleEntity
            entity.RegisterComponent(eventDispatcher);
            entity.RegisterComponent(stats);
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(jumpExecutor);
            entity.RegisterComponent(dashExecutor);
            entity.RegisterComponent(gravityExecutor);
            entity.RegisterComponent(interactExecutor);
            entity.RegisterComponent(decisionLayer);
            entity.RegisterComponent(deathComponent);
            entity.RegisterComponent(hitReaction);

            // 11. Register player camera with ICameraSystem
            var cameraSystem = SystemRepository.Instance.GetSystem<ICameraSystem>();
            if (cameraSystem != null && firstPersonCamera != null)
            {
                cameraSystem.RegisterPlayerCamera(firstPersonCamera.transform);
            }


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
