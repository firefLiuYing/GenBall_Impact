using GenBall.BattleSystem.Character;
using GenBall.BattleSystem.Command;
using GenBall.BattleSystem.Framework;
using GenBall.BattleSystem.Mover;
using GenBall.Framework.Config;
using GenBall.Player.Controller;
using GenBall.Player.Executor;
using GenBall.Player.Input;
using UnityEngine;

namespace GenBall.Player
{
    public static class PlayerEntityFactory
    {
        public static void AssemblePlayer(GameObject playerInstance, AppSettingsConfig config)
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

            // 3. Create framework components
            var stats = new StatComponent();
            stats.GetOrCreate("MaxHealth", 100f);

            var damageReceiver = new DamageReceiverComponent(entity);
            var buffContainer = new BuffContainerComponent();
            var attackComp = new AttackComponent(entity, new WeaponDamageCalculator());
            var dispatcher = new CommandDispatcherComponent();

            // 4. Create executors
            var jumpExecutor = new PlayerJumpExecutor(rigidbody, playerMover, config, inputHandler, groundDetect);
            var dashExecutor = new PlayerDashExecutor(rigidbody, playerMover, config);
            var attackExecutor = new PlayerAttackExecutor(weaponController);

            // 5. Register executors on dispatcher
            dispatcher.RegisterExecutor<MoveCommand>(playerMover);
            dispatcher.RegisterExecutor<JumpCommand>(jumpExecutor);
            dispatcher.RegisterExecutor<DashCommand>(dashExecutor);
            dispatcher.RegisterExecutor<AttackCommand>(attackExecutor);
            if (rotater != null)
                dispatcher.RegisterExecutor<RotateCommand>(rotater);

            // 6. Create input adapter and decision layer
            var inputAdapter = new PlayerInputAdapter(inputHandler);
            var decisionLayer = new PlayerDecisionLayer(entity, inputAdapter);
            decisionLayer.Dispatcher = dispatcher;

            // 7. Register everything on BattleEntity
            entity.RegisterComponent(stats);
            entity.RegisterComponent(damageReceiver);
            entity.RegisterComponent(buffContainer);
            entity.RegisterComponent(attackComp);
            entity.RegisterComponent(dispatcher);
            entity.RegisterComponent(jumpExecutor);
            entity.RegisterComponent(dashExecutor);
            entity.RegisterComponent(attackExecutor);
            entity.RegisterComponent(decisionLayer);
        }
    }
}
