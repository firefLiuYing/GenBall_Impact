using GenBall.BattleSystem;
using GenBall.BattleSystem.Buff;
using GenBall.BattleSystem.Bullets;
using GenBall.BattleSystem.Weapons.Accessory;
using GenBall.Framework.Config;
using GenBall.Framework.Entity;
using GenBall.Interact;
using GenBall.Map;
using GenBall.Player;
using GenBall.Procedure;
using GenBall.Procedure.Execute;
using GenBall.Procedure.Game;
using Yueyn.Main;
using UnityEngine;

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
            // 配置系统
            SystemRep.RegisterSystem<IConfigProvider>(new AppConfigManager());

            // 存档系统（待重新设计后移入 Yueyn 框架层）
            SystemRep.RegisterSystem<ISaveService>(new SaveSystem());

            // Phase 4: 战斗系统（Buff、伤害、死亡）
            SystemRep.RegisterSystem<IBuffRegistry>(new BuffRegistry());
            SystemRep.RegisterSystem<IBuffTickSystem>(new BuffTickSystem());
            SystemRep.RegisterSystem<IEntityUpdateSystem>(new EntityUpdateSystem());
            SystemRep.RegisterSystem<IDamageSystem>(new DamageSystemDefault());
            SystemRep.RegisterSystem<IDeathSystem>(new DeathSystemDefault());

            // Phase 2A: 交互、场景、传送、暂停、游戏管理
            SystemRep.RegisterSystem<IInteractSystem>(new InteractSystem());
            SystemRep.RegisterSystem<ISceneStateSystem>(new SceneSystem());
            SystemRep.RegisterSystem<ITeleportSystem>(new TeleportSystem());
            SystemRep.RegisterSystem<IPauseSystem>(new PauseManager());
            SystemRep.RegisterSystem<IGameManagerSystem>(new GameManager());

            // Phase 2B: 启动流程、场景执行
            SystemRep.RegisterSystem<ILaunchSystem>(new LaunchSystemDefault());

            // Phase 3: 玩家、子弹、进化
            SystemRep.RegisterSystem<IPlayerSystem>(new PlayerSystemDefault());
            SystemRep.RegisterSystem<IBulletSystem>(new BulletSystem());
            SystemRep.RegisterSystem<IEvolutionSystem>(new EvolutionSystem());

            SystemRep.RegisterSystem<ISceneExecutorSystem>(new SceneExecutorSystemDefault());

            Debug.Log("[FrameworkDefault] Systems registered successfully");
        }
    }
}