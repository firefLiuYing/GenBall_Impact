using GenBall.BattleSystem.AbilityWeapon;
using GenBall.BattleSystem;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.CombatState;
using GenBall.Enemy;
using GenBall.Framework.TimeScale;
using GenBall.GameCamera;
using GenBall.GM;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using GenBall.Interact;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using GenBall.UI;
using Yueyn.Main;
using UnityEngine;
using Yueyn.UI;

namespace GenBall.Framework
{
    /// <summary>
    /// 默认框架，注册和维护所有 ISystem 业务系统
    /// 基建模块（Event/Resource/UI/Pool）在 FrameworkBase 中直接初始化
    /// </summary>
    public class FrameworkDefault : FrameworkBase
    {
        protected override void DoInit()
        {
            // 注册敌人 prefab 路径映射
            EnemyRegister.Register();

            // 配置系统
            SystemRep.RegisterSystem<IConfigProvider>(new AppConfigManager());

            // 存档系统（待重新设计后移入 Yueyn 框架层）
            SystemRep.RegisterSystem<ISaveService>(new SaveSystem());

            // 用户设置存储（独立于存档槽位）
            SystemRep.RegisterSystem<IUserSettingsStorage>(new UserSettingsStorage());

            // Phase 4: 战斗系统（Buff、伤害、死亡）
            SystemRep.RegisterSystem<IBuffRegistry>(new BuffRegistry());
            SystemRep.RegisterSystem<IBuffTickSystem>(new BuffTickSystem());
            SystemRep.RegisterSystem<IEntityUpdateSystem>(new EntityUpdateSystem());
            SystemRep.RegisterSystem<IDamageSystem>(new DamageSystemDefault());
            SystemRep.RegisterSystem<IDeathSystem>(new DeathSystemDefault());

            // Phase 2A: 交互、场景、传送、暂停、游戏管理、相机
            SystemRep.RegisterSystem<IInteractSystem>(new InteractSystem());
            SystemRep.RegisterSystem<ICameraSystem>(new CameraSystemDefault());
            SystemRep.RegisterSystem<ISceneStateSystem>(new SceneSystem());
            SystemRep.RegisterSystem<ISceneLoadSystem>(new SceneLoadSystemDefault());
            SystemRep.RegisterSystem<ITeleportSystem>(new TeleportSystem());
            SystemRep.RegisterSystem<IPauseSystem>(new PauseManager());
            SystemRep.RegisterSystem<IGameManagerSystem>(new GameManager());

            // 注册存档数据提供者到 GameManager
            var gameManager = SystemRep.GetSystem<IGameManagerSystem>();
            gameManager.RegisterSaveDataProvider(new MapSaveDataProvider());
            gameManager.RegisterSaveDataProvider(new PlayerSaveDataProvider());

            // Phase 2B: 启动流程、场景执行、游戏开始
            SystemRep.RegisterSystem<ILaunchSystem>(new LaunchSystemDefault());
            SystemRep.RegisterSystem<IGameStartSystem>(new GameStartSystemDefault());

            // Phase 3: 玩家、子弹、进化
            SystemRep.RegisterSystem<IPlayerSystem>(new PlayerSystemDefault());
            SystemRep.RegisterSystem<IBulletSystem>(new BulletSystem());
            SystemRep.RegisterSystem<IEvolutionSystem>(new EvolutionSystem());

            // Combat state + ability weapon system
            SystemRep.RegisterSystem<ICombatStateSystem>(new CombatStateSystem());
            SystemRep.RegisterSystem<IAbilityWeaponSystem>(new AbilityWeaponSystem());
            SystemRep.RegisterSystem<ITimeScaleSystem>(new TimeScaleSystemDefault());

            SystemRep.RegisterSystem<ISceneExecutorSystem>(new SceneExecutorSystemDefault());

            // GM 命令系统（调试工具）
            SystemRep.RegisterSystem<IGMCommandSystem>(new GMCommandSystemDefault());

            // LoadingBusinessLogic：常驻 UI 业务逻辑，通过事件总线管理启动 Loading 流程
            BusinessLogicManager.Instance.CreateLogic<LoadingBusinessLogic>();

            // InGameUIBusinessLogic 不再在启动时创建，由编排层在 SceneReady 事件后显式创建

            Debug.Log("[FrameworkDefault] Systems registered successfully");
        }

        protected override void DoStart()
        {
            base.DoStart();
            // 启动流程由 LaunchSystemDefault (IFrameUpdate) 驱动：StartupLoading → StartForm → LoadScene
        }
    }
}