using GenBall.BattleSystem;
using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.BattleSystem.Weapons.Factory;
using GenBall.Event;
using GenBall.GameCamera;
using GenBall.Interact;
using GenBall.Player.Executor;
using GenBall.Player.Input;
using UnityEngine;
using Yueyn.Event;
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
            var playerMoveExecutor = new PlayerMoveExecutor();
            playerMoveExecutor.Init(mover, config.speed);
            var inputHandler = playerInstance.GetComponentInChildren<InputHandler>();
            var groundDetect = playerInstance.GetComponent<ICharacterGroundDetect>()
                               ?? playerInstance.GetComponentInChildren<ICharacterGroundDetect>();

            // WeaponAttackExecutor (pure C#) replaces WeaponExecutor MB.
            // Read weaponSpawnPoint from the old WeaponExecutor's serialized field
            // (carefully positioned on the prefab, cannot be found by path reliably).
            var oldWeaponExecutor = playerInstance.GetComponentInChildren<WeaponExecutor>();
            var weaponSpawnPoint = oldWeaponExecutor != null
                ? oldWeaponExecutor.WeaponSpawnPoint
                : playerInstance.transform;
            var weaponAttackExecutor = new WeaponAttackExecutor(entity, weaponSpawnPoint);
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
            entity.RegisterComponent(stats);

            // 4. Create EventDispatcherComponent
            var eventDispatcher = new EventDispatcherComponent(entity);
            entity.RegisterComponent(eventDispatcher);

            // 5. Create framework components
            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new WeaponDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 6. Create executors
            // Jump executor — provides extra force beyond gravity during jump
            var jumpExecutor = new PlayerJumpExecutor(mover, config, groundDetect);
            // Dash executor — cancels jump on activation; registered LAST to overwrite all other velocity
            var dashExecutor = new PlayerDashExecutor(mover, config, entity, jumpExecutor);

            // Gravity executor — checks dispatcher for BlocksGravity (e.g., dash)
            var gravityExecutor = new PlayerGravityExecutor(mover, groundDetect, config, dispatcher);

            // Interact executor
            var camera = firstPersonCamera;
            var interactSystem = SystemRepository.Instance.GetSystem<IInteractSystem>();
            var interactExecutor = new PlayerInteractExecutor(interactSystem, camera,
                config.sightDetectRadius, config.sightDetectDistance, config.interactableLayer);

            // 7. Register executors on dispatcher
            dispatcher.RegisterExecutor<MoveCommand>(playerMoveExecutor);
            dispatcher.RegisterExecutor<JumpCommand>(jumpExecutor);
            dispatcher.RegisterExecutor<DashCommand>(dashExecutor);
            dispatcher.RegisterExecutor<AttackCommand>(weaponAttackExecutor);
            dispatcher.RegisterExecutor<ReloadCommand>(weaponAttackExecutor);
            dispatcher.RegisterExecutor<SwitchWeaponCommand>(weaponAttackExecutor);
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

            // 10. Register everything on BattleEntity.
            // Order no longer matters for velocity correctness — blocking is declarative
            // (IArbitratedCommand.BlocksMove/BlocksRotate/BlocksGravity).
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponentAs<IDamageable>(damageReceiver);
            entity.RegisterComponentAs<IHealth>(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(gravityExecutor);
            entity.RegisterComponent(jumpExecutor);
            entity.RegisterComponent(dashExecutor);
            entity.RegisterComponent(interactExecutor);
            entity.RegisterComponent(decisionLayer);
            entity.RegisterComponent(deathComponent);
            entity.RegisterComponent(hitReaction);
            entity.RegisterComponent(weaponAttackExecutor);

            // 11. Register player camera with ICameraSystem
            var cameraSystem = SystemRepository.Instance.GetSystem<ICameraSystem>();
            if (cameraSystem != null && firstPersonCamera != null)
            {
                cameraSystem.RegisterPlayerCamera(firstPersonCamera.transform);
            }

            // 12. Bridge entity-local health events to global CEventRouter for HUD
            eventDispatcher.Subscribe<HealthChangedEventData>((int)EntityEventId.HealthChanged,
                data =>
                {
                    CEventRouter.Instance.FireNow((int)GlobalEventId.HealthChanged, (int)data.NewHealth);
                    CEventRouter.Instance.FireNow((int)GlobalEventId.MaxHealthChanged, (int)data.MaxHealth);
                });

            eventDispatcher.Subscribe<StatChangedEventData>((int)EntityEventId.StatChanged,
                data =>
                {
                    if (data.StatName == "Shield")
                        CEventRouter.Instance.FireNow((int)GlobalEventId.ArmorChanged, (int)data.NewValue);
                });

            // Fire initial values so HUD shows correct state immediately
            CEventRouter.Instance.FireNow((int)GlobalEventId.HealthChanged, (int)stats.GetValue("CurrentHealth"));
            CEventRouter.Instance.FireNow((int)GlobalEventId.MaxHealthChanged, (int)stats.GetValue("MaxHealth"));
            CEventRouter.Instance.FireNow((int)GlobalEventId.ArmorChanged, (int)stats.GetValue("Shield"));

            // 13. [临时] 装备默认手枪，方便场景测试验证 Player 组装+武器流程。
            // 正式流程：首次进游戏装默认手枪、复活/进化时根据进化阶段装对应武器，
            // 统一由武器生命周期管理方调用 WeaponAttackExecutor.EquipWeapon()。
            // 详见 .claude/docs/execution-plan.md B-3 武器生命周期管理。
            var defaultWeapon = WeaponEntityFactory.CreateDefault(weaponSpawnPoint);
            if (defaultWeapon != null)
            {
                weaponAttackExecutor.EquipWeapon(defaultWeapon);
            }
            else
            {
                Debug.LogWarning("[PlayerEntityFactory] 默认手枪创建失败，" +
                    "请确认手枪 prefab 已挂载 WeaponAssembly 组件且预制体路径正确。");
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
