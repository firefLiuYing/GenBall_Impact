# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-05-28
> **当前阶段**：Phase B

---

## 目标

从当前的"三轨并行"架构收敛到纯 **BattleEntity + ISystem** 架构，同时实现第一章 DEMO 所需的全部游戏功能。

## 当前架构状态速查

| 层 | 代表 | 状态 |
|----|------|------|
| Layer 1 | IComponent 模块（GameEntry.GetModule） | 运行中，已被 ISystem 替代，等删除 |
| Layer 2 | CharacterState / Player / WeaponBase / EnemyBase | 运行中，等迁移到 BattleEntity |
| Layer 3 | BattleEntity Framework + ISystem 服务 | 框架已有，组件不全，未投产 |

### 已完成的 ISystem 注册（FrameworkDefault.cs）

IConfigProvider, ISaveService, IBuffRegistry, IBuffTickSystem, IEntityUpdateSystem,
IDamageSystem, IDeathSystem, IInteractSystem, ISceneStateSystem, ISceneLoadSystem,
ITeleportSystem, IPauseSystem, IGameManagerSystem, ILaunchSystem, IPlayerSystem,
IBulletSystem, IEvolutionSystem, ISceneExecutorSystem, IGMCommandSystem

### 已完成的 BattleEntity 组件

StatComponent, DamageReceiverComponent, BuffContainerComponent, AttackComponent,
CommandDispatcherComponent, DecisionLayer (Player/Enemy), EventDispatcherComponent,
DeathComponent (IDeathHandler)

### 缺失的 BattleEntity 组件

ShieldRegenComponent（Phase C 实现自动回复）

---

## Phase A：完成 BattleEntity 框架

**目标**：让 BattleEntity 能完整替代 CharacterState
**范围**：`Assets/Scripts/GenBall/BattleSystem/Framework/`
**依赖**：无

### A-1：CommandDispatcher 组件

**状态**：✅ 已完成（2026-05-24）

**已实现**：
- `IArbitratedCommand` 接口：InterruptPriority / AntiInterruptPriority / Bufferable
- `AttackCommand` 升级为 `IArbitratedCommand`（默认 2/2，可选参数向后兼容）
- 新增 `JumpCommand`（3/3, Bufferable）、`DashCommand`（5/5, 不可缓冲）
- 新增 `IJump`、`IDash` 执行器接口
- `CommandDispatcherComponent`（纯 C#）：
  - 指令三级分类：连续型（直通但有动作时阻断/清零）、瞬时（始终执行）、动作型（仲裁+缓冲）
  - 仲裁：InterruptPriority >= AntiInterruptPriority 则打断
  - 缓冲：0.2s 窗口 FIFO 环形队列，完成后 drain 一个
  - 暂停：Issue() 入口检查 IPauseSystem.IsPaused
  - 帧序：IEntityLogicUpdate，注册在 DecisionLayer 之前
- `CommandDispatcherComponentTests`：45 个测试覆盖路由/阻断/仲裁/缓冲/暂停/完成检测/边界

**文件清单**：
- 新建 `Command/IArbitratedCommand.cs`、`Command/IJump.cs`、`Command/IDash.cs`
- 新建 `Command/JumpCommand.cs`、`Command/DashCommand.cs`
- 修改 `Command/CharacterCommand.cs`（AttackCommand 升级）
- 新建 `Framework/CommandDispatcherComponent.cs`
- 新建 `Framework/Editor/CommandDispatcherComponentTests.cs`

### A-2：DecisionLayer 组件

**状态**：✅ 已完成（2026-05-24）

**已实现**：
- `IDecisionLayer` 接口：`CommandDispatcherComponent Dispatcher { get; set; }` + `void MakeDecision(float deltaTime)`
- `IPlayerInputProvider` 接口：MoveDirection / ViewDelta / JumpPressed / DashPressed / FirePressed
- `PlayerDecisionLayer`（纯 C#，`IEntityLogicUpdate`）：
  - 通过 `IPlayerInputProvider` 读取输入
  - Move/Rotate 每帧持续发出
  - Jump（检查地面）、Dash（0.5s 冷却）、Attack（FirePressed 时）按条件发出
  - 地面检测：`BattleEntity.Get<ICharacterGroundDetect>()` 优先，`GetComponentInChildren` 作为 fallback
- `EnemyDecisionLayer`（纯 C#，`IEntityLogicUpdate`）：
  - 管理 `Fsm<EnemyDecisionLayer>`，硬编码 4 状态（Idle/Chase/Attack/Wander）
  - 从 BattleEntity 的 GameObject 查找 EnemyDetectController/EnemyAttackController
  - `IssueCommand()` 通过 CommandDispatcherComponent 路由
- `EnemyDecisionStateBase`（`FsmState<EnemyDecisionLayer>`）：Agent/Detect/AttackController/ChangeState/IssueCommand
- AI 决策状态：AIDecisionIdleState、AIDecisionChaseState、AIDecisionAttackState、AIDecisionWanderState
- `DecisionLayerTests`：37 个测试覆盖 Player 和 Enemy 决策层
- TODO Phase B：AI 状态改为从 aiConfig 数据驱动

**文件清单**：
- 新建 `Framework/IDecisionLayer.cs`
- 新建 `Framework/PlayerDecisionLayer.cs`（含 IPlayerInputProvider）
- 新建 `Framework/EnemyDecisionLayer.cs`
- 新建 `Framework/AI/EnemyDecisionStateBase.cs`
- 新建 `Framework/AI/AIDecisionIdleState.cs`
- 新建 `Framework/AI/AIDecisionChaseState.cs`
- 新建 `Framework/AI/AIDecisionAttackState.cs`
- 新建 `Framework/AI/AIDecisionWanderState.cs`
- 新建 `Framework/Editor/DecisionLayerTests.cs`

### A-3：EventDispatcher + 关联重构

**状态**：✅ 已完成（2026-05-27）

**决定**：
- EventDispatcherComponent 作为独立可选组件，包装 `Yueyn.Event.EventDispatcher`
- 所有 BattleEntity 共用单一 `EntityEventId` enum
- 仅用 FireNow（立即触发），首批事件：StatChanged + HealthChanged
- 不做全局 CEventRouter 自动桥接（订阅方手动转发）
- CurrentHealth 迁移到 StatComponent 统一管理
- 新增 DeathComponent（IDeathHandler 策略）处理差异化死亡
- Shield 作为 Stat 纳入数值系统（DamageReceiverComponent.TakeDamage 优先扣盾）

**文件清单**：
- 新建 `Framework/EntityEventId.cs`
- 新建 `Framework/EventDispatcherComponent.cs`
- 新建 `Framework/DeathComponent.cs`（含 `IDeathHandler`、`HealthChangedEventData`）
- 新建 `Framework/Editor/EventDispatcherComponentTests.cs`（17 测试）
- 修改 `Framework/StatComponent.cs`（BattleEntity 引用 + 发射 StatChanged 事件）
- 修改 `Framework/Stat.cs`（SetBaseValue/AddModifier/RemoveModifier 返回 oldValue）
- 修改 `Framework/DamageReceiverComponent.cs`（CurrentHealth → StatComponent，Shield 优先，IHealable，HealthChanged 事件）
- 修改 `Player/PlayerEntityFactory.cs`（装配 EventDispatcher + DeathComponent + Shield stats）
- 修改 `Enemy/EnemyEntityFactory.cs`（装配 EventDispatcher + DeathComponent）
- 修改 `Framework/Editor/BattleEntityIntegrationTests.cs`（适配新构造函数）

---

## Phase B：迁移实体到 BattleEntity

**目标**：Player、Enemy、Weapon 全部跑在 BattleEntity 上。删除 CharacterState。
**依赖**：Phase A 完成

### B-1：Player 迁移

**状态**：✅ 核心代码完成（2026-05-28），仅剩验证和旧代码删除

**已完成**：
- [x] B-1a：BattleEntity 装配 — PlayerEntityFactory.AssemblePlayer() 注册 StatComponent/DamageReceiver/BuffContainer/AttackComponent/CommandDispatcher/PlayerDecisionLayer + 所有 executor
- [x] B-1b（部分）：Executor 组件 — PlayerJumpExecutor (IJump+IEntityLogicUpdate, 变高跳跃物理), PlayerDashExecutor (IDash+IEntityLogicUpdate, 无敌帧+结束期), PlayerAttackExecutor (IAttack, 委托 WeaponController), PlayerInputAdapter (IPlayerInputProvider 包装 InputHandler)
- [x] PlayerMover 增加 LockVertical/LockHorizontal 属性
- [x] InputHandler 增加 ViewDelta/IsDashPressed/IsFirePressed 属性
- [x] WeaponController 增加 public Fire(ButtonState) 方法
- [x] PlayerSystemDefault.CreatePlayer() 接入工厂调用

**待完成**：
- [x] B-1c：PlayerConfigSo → AppSettingsConfig（移除 Editor-only，#if UNITY_EDITOR 全部替换为 IConfigProvider）
- [x] B-1d：IRotate 适配（PlayerRotater 已实现 IRotate，工厂自动发现）
- [x] B-1g：PlayerConfig 独立化（2026-05-28）— 创建 PlayerConfig ScriptableObject，Player 专属字段从 AppSettingsConfig 拆出。AppConfigManager 加载 PlayerConfig。PlayerEntityFactory/Executor 全链路使用 PlayerConfig。
- [x] B-1h：指令系统升级 + 无敌帧（2026-05-28）— DamageReceiverComponent 加 IsInvincible，PlayerDashExecutor 设置无敌。AttackCommand 加 ButtonState 字段。新增瞬时指令：ReloadCommand/SwitchWeaponCommand + IReload/ISwitchWeapon 接口。CommandDispatcher 瞬时路由通道。PlayerDecisionLayer 跟踪 Fire Down/Held/Up 状态。IPlayerInputProvider 新增 ReloadPressed/SwitchWeaponPressed。
- [x] B-1i：工厂补齐 + HitReactionComponent 修复（2026-05-29）— PlayerReloadExecutor/PlayerSwitchWeaponExecutor 创建并注册到工厂。WeaponController 新增 Reload()/SwitchWeapon() 公开方法。HitReactionComponent 构造函数改为直接注入 CommandDispatcherComponent + EventDispatcherComponent（修复 entity.Get 在注册前为 null 的 bug）。
- [x] B-1j：相机系统（2026-05-29）— 创建 ICameraSystem (IFrameUpdate) 全局相机管理：MainCamera 引用、FOV 平滑、屏幕震动、Override Target。PlayerEntityFactory 注册 FirstPersonCamera 到 ICameraSystem。InputHandler 替换 Camera.main 为 ICameraSystem.MainCamera。
- [ ] B-1e：验证 Launcher 场景（用户编译测试）
- [ ] B-1f：删除旧 Player.cs 及分部类（等 B-2/B-3 完成下游引用迁移）

**新建文件**：
- `Player/Executor/PlayerJumpExecutor.cs`
- `Player/Executor/PlayerDashExecutor.cs`
- `Player/Executor/PlayerAttackExecutor.cs`
- `Player/Executor/PlayerInputAdapter.cs`
- `Player/Executor/PlayerReloadExecutor.cs`（ReloadCommand → WeaponController.Reload()）
- `Player/Executor/PlayerSwitchWeaponExecutor.cs`（SwitchWeaponCommand → WeaponController.SwitchWeapon()）
- `Player/PlayerEntityFactory.cs`
- `Player/PlayerConfig.cs`（独立配置，从 AppSettingsConfig 拆出）
- `BattleSystem/Framework/HitReactionComponent.cs`（受击硬直，IStun + IEntityLogicUpdate）
- `GameCamera/ICameraSystem.cs`（全局相机 ISystem：MainCamera/FOV/Shake/Override）
- `GameCamera/CameraSystemDefault.cs`（IFrameUpdate：FOV 平滑 + 屏幕震动 + Override 跟踪）

**修改文件**：
- `Player/Controller/PlayerMover.cs`（LockVertical/LockHorizontal）
- `Player/Controller/WeaponController.cs`（Fire + Reload + SwitchWeapon 方法）
- `Player/Input/InputHandler.cs`（ViewDelta/IsDashPressed/IsFirePressed/IsReloadPressed/IsSwitchWeaponPressed）
- `Player/PlayerSystemDefault.cs`（工厂接入 + PlayerConfig）
- `Player/PlayerSystemDefault.cs`（工厂接入 + PlayerConfig）
- `Player/Controller/PlayerStateMachine.cs`（PlayerConfig 引用）
- `Player/Controller/PhysicsController.cs`（PlayerConfig 引用）
- `Player/Controller/JumpController.cs`（PlayerConfig 引用）
- `Framework/Config/AppSettingsConfig.cs`（移除 Player 字段）
- `Framework/Config/AppConfigManager.cs`（加载 PlayerConfig）
- `BattleSystem/Command/CharacterCommand.cs`（AttackCommand 加 ButtonState）
- `BattleSystem/Command/ReloadCommand.cs`（新建瞬时指令）
- `BattleSystem/Command/SwitchWeaponCommand.cs`（新建瞬时指令）
- `BattleSystem/Command/IReload.cs`（新建 executor 接口）
- `BattleSystem/Command/ISwitchWeapon.cs`（新建 executor 接口）
- `BattleSystem/Framework/DamageReceiverComponent.cs`（IsInvincible 属性）
- `BattleSystem/Framework/PlayerDecisionLayer.cs`（Fire 状态跟踪 + 新输入接口）
- `BattleSystem/Framework/CommandDispatcherComponent.cs`（瞬时指令路由）
- `Player/Executor/PlayerDashExecutor.cs`（BattleEntity 参数 + 无敌设置）
- `Player/Executor/PlayerAttackExecutor.cs`（cmd.TriggerState 路由）
- `Player/Executor/PlayerInputAdapter.cs`（ReloadPressed/SwitchWeaponPressed）
- `Player/Input/InputHandler.cs`（IsReloadPressed/IsSwitchWeaponPressed + ICameraSystem 替换 Camera.main）
- `Framework/FrameworkDefault.cs`（注册 ICameraSystem）

**测试文件**：
- `Player/Executor/Editor/PlayerExecutorTests.cs`（20 测试）
- `Framework/Editor/DecisionLayerTests.cs`（37 测试，已修复 MultipleCommands 断言）

### B-2：Enemy 迁移

**状态**：🔄 进行中（2026-05-24）

**已完成**：
- [x] B-2a：EnemyEntityFactory.AssembleEnemy() 注册 BattleEntity + StatComponent/DamageReceiver/BuffContainer/AttackComponent/CommandDispatcher/EnemyDecisionLayer
- [x] B-2b：复用现有 EnemyMover (IMove) + EnemyAttackController (IAttack) + EnemyFaceController (IFaceDirection) + EnemyDetectController
- [x] EnemyAIController 暴露 AiConfig 公共属性
- [x] SceneExecutorSystemDefault.LoadEnemyUnit() 接入工厂调用

**待完成**：
- [ ] B-2d：EnemyDecisionLayer 从 aiConfig 数据驱动（当前硬编码 FSM）
- [ ] B-2e：验证敌人生成→巡逻→追击→攻击→死亡流程
- [ ] B-2f：删除旧 EnemyBase、CharacterState

**新建文件**：
- `Enemy/EnemyEntityFactory.cs`

**修改文件**：
- `Enemy/Controller/EnemyAIController.cs`（AiConfig 属性）
- `Procedure/Execute/SceneExecutorSystemDefault.cs`（工厂接入 + using）

### B-3：Weapon 迁移

**状态**：❌ 未开始

**当前旧代码位置**：
- `BattleSystem/Weapons/WeaponBase.cs` — 旧抽象基类
- `BattleSystem/Weapons/WeaponState.cs` — 新武器状态（MonoBehaviour）
- `BattleSystem/Weapons/FireComponent.cs` — 射击
- `BattleSystem/Weapons/MagazineComponent.cs` — 弹匣

**迁移策略**：
- [ ] B-3a：用 BattleEntity 装配 Weapon
  - `BattleEntity` + `StatComponent(Damage, FireInterval, ReloadTime, ...)`
  - `AttackComponent(WeaponDamageCalculator)` 或专用 WeaponAttackComponent
- [ ] B-3b：创建 `WeaponFireComponent` → 射击逻辑
- [ ] B-3c：创建 `WeaponMagazineComponent` → 弹匣/换弹逻辑
- [ ] B-3d：对接 BulletSystem（IBulletSystem.FireBullet）
- [ ] B-3e：创建 Weapon 装配工厂
- [ ] B-3f：删除旧 `WeaponBase`、`FireComponent`、`MagazineComponent`
- [ ] B-3g：合并新旧 WeaponState（只保留 BattleEntity 版本）

---

## Phase C：游戏设计基础系统

**目标**：实现第一章 DEMO 的核心游戏功能
**依赖**：Phase B 完成（实体层就绪）

### C-1：能力枪械系统

**状态**：❌ 未开始
**设计文档**：`.claude/docs/design/weapon-system.md`

- [ ] C-1a：创建 `IAbilityWeaponSystem : ISystem`
  - 管理三把能力枪的冷却
  - 管理当前激活状态
  - 脱战自动重置冷却
  - 自动切回物理枪械
- [ ] C-1b：实现 `AbilityWeaponSystemDefault`
- [ ] C-1c：创建武器轮盘 UI + 切换交互
- [ ] C-1d：实现 匣纳之枪（StackGun：吸收/射出奥比斯，LIFO）
- [ ] C-1e：实现 连理之枪（GrappleGun：抓钩 + 重量判定）
- [ ] C-1f：实现 裁径之枪（PathGun：定向通道）

### C-2：奥比斯种类补齐

**状态**：仅 NormalOrbis 存在
**设计文档**：`.claude/docs/design/enemy-design.md`

- [ ] C-2a：橙黄奥比斯（自爆型 — 接近检测 + 范围爆炸）
- [ ] C-2b：飞行奥比斯（飞行移动 + 撞击）
- [ ] C-2c：吞噬者奥比斯（抓取玩家 + 扣血 + 抛出）
- [ ] C-2d：相溶奥比斯（死亡治疗周围所有单位）
- [ ] C-2e：酸液奥比斯（地面酸液 trail + 喷吐酸液）
- [ ] C-2f：重量等级系统（轻/中/重，影响连理之枪交互）

### C-3：经济与技能树

**状态**：❌ 未开始
**设计文档**：`.claude/docs/design/economy-save.md`

- [ ] C-3a：创建 `ICurrencySystem : ISystem`
  - 圆形数据（击杀计数 → 存档转换 1:10）
  - 失落数据（探索获取）
  - 货币变更事件
- [ ] C-3b：创建 `ISkillTreeSystem : ISystem`
  - 节点解锁（消耗货币）
  - 节点解锁事件 → 触发阶段解锁
- [ ] C-3c：实现存档时的货币转换 + 进化重置
- [ ] C-3d：创建技能树 ScriptableObject 配置 + 基础 UI

### C-4：枪械进化系统完善

**状态**：IEvolutionSystem 已有，但不完整
**设计文档**：`.claude/docs/design/weapon-system.md`

- [ ] C-4a：进化阶段配置（阶段一~四的负载限制 + 模块槽数）
- [ ] C-4b：模块系统（负载费用 + 槽位 + 蓝图获取）
- [ ] C-4c：武器基础形态切换（突击步枪/霰弹枪/激光枪/光剑等）
- [ ] C-4d：存档时进化重置 + 击杀计数清零
- [ ] C-4e：Q 键手动进化触发 + 进化演出
- [ ] C-4f：阶段解锁与技能树节点挂钩

---

## Phase D：游戏内容层

**目标**：第一章可玩
**依赖**：Phase C 完成

### D-1：机关系统

**状态**：仅 IInteractable 检测框架存在
**设计文档**：`.claude/docs/design/level-chapter1.md`

- [ ] D-1a：酸蚀材质系统（酸液伤害类型 → 特定材质销毁）
- [ ] D-1b：重量/冲击破坏（地板/墙壁可破坏阈值）
- [ ] D-1c：可推物体 + 重量交互
- [ ] D-1d：能量吸收装置（计数奥比斯能量 → 触发）
- [ ] D-1e：密码锁 UI

### D-2：战斗反馈

**状态**：❌ 未开始
**设计文档**：`.claude/docs/design/ui-feedback.md`

- [ ] D-2a：准星系统（散布指示 + 命中变色 + 击杀形变）
- [ ] D-2b：屏幕震动（CameraController 集成）
- [ ] D-2c：枪口特效 + 弹道拖尾
- [ ] D-2d：受击特效 + 死亡特效
- [ ] D-2e：音效管线（开火/命中/击杀）

### D-3：剧情对话

**状态**：❌ 未开始
**设计文档**：`.claude/docs/design/ui-feedback.md`

- [ ] D-3a：底部对话框 UI
- [ ] D-3b：事件触发（位置/拾取/击杀/演出）
- [ ] D-3c：第一章对话数据

### D-4：关卡搭建

**状态**：场景存在，机关/敌人待布置
**设计文档**：`.claude/docs/design/level-chapter1.md`

- [ ] D-4a：第一段（苏醒→追寻光芒）场景搭建
- [ ] D-4b：第二段（实验室探索）场景搭建
- [ ] D-4c：第三段（登顶追逐+水下段落）场景搭建
- [ ] D-4d：存档点布置 + 怪物配置

---

## Phase E：清理旧代码

**目标**：只留 BattleEntity + ISystem。删除所有旧架构残留。
**依赖**：Phase D 完成，且所有旧调用方已迁移

- [ ] E-1：删除 `GameEntry.cs` 及所有 partial 类（`.Bullet`, `.Weapon`, `.Register` 等）
- [ ] E-2：删除 `Entry.cs`（Yueyn 框架入口）
- [ ] E-3：删除 `IComponent.cs` + 所有 IComponent 实现
  - `ExecuteComponent`, `GameSceneExecuteModule`, `PlayerManager`, `MapModule`,
    `SceneModule`, `SaveComponent`, `UIManager`(旧), `TimelineSystem`
- [ ] E-4：删除 `ISingleton.cs` + `SingletonManager.cs`
- [ ] E-5：删除 `EntityRegister/` 目录
- [ ] E-6：删除 `CharacterState.cs` + `CharacterStats` + 所有 Controller/Initializer
- [ ] E-7：删除 `WeaponBase.cs` + `FireComponent` + `MagazineComponent`（旧武器）
- [ ] E-8：删除 `EnemyBase.cs` + 旧模块系统（`AttackModule`, `DetectModule` 等）
- [ ] E-9：删除 `Player.cs` 及所有分部类
- [ ] E-10：删除所有 `static class ConfigProvider`
- [ ] E-11：删除旧 `EventManager` 事件系统代码（如果完全被 CEventRouter 替代）
- [ ] E-12：验证：编译通过 + 所有测试通过 + Launcher 场景全流程

---

## 进度跟踪

| Phase | 任务数 | 已完成 | 进度 |
|-------|--------|--------|------|
| A: BattleEntity 框架 | 3 | 3 | 100% |
| B: 实体迁移 | 3 (含 20 子任务) | B-1 95%, B-2 60%, B-3 0% | 48% |
| C: 基础系统 | 4 (含 20 子任务) | 0 | 0% |
| D: 内容层 | 4 (含 16 子任务) | 0 | 0% |
| E: 清理 | 1 (含 12 子任务) | 0 | 0% |
| F: 事件系统重构 | 2 | 0 | 0% |

### Phase F：事件系统重构

**目标**：将全局事件和 UI 事件的 ID 定义统一为 enum，替代分散的 const int / 字符串常量。

**依赖**：A-3 完成

- [ ] F-1：全局事件 enum — 将 `GlobalEventSystem.Generated.cs` 中的字符串常量改为 `GlobalEventId` enum
- [ ] F-2：UI 事件 enum — 将 UI 相关的事件 ID 统一为 `UIEventId` enum

---

## 执行约定

1. **每个任务独立验证**：改完一个任务 → 编译 → 跑测试 → 标记完成
2. **会话结束时更新本文档**：勾选已完成任务，更新进度表
3. **新会话开始时先读本文档**：了解当前进度和下一个任务
4. **Phase A 优先**：在 BattleEntity 框架完整之前，不开始迁移
5. **BattleEntity 组件原则**：纯 C# 类，不继承 MonoBehaviour，通过构造函数注入 BattleEntity 引用
6. **ISystem 原则**：业务系统不继承 MonoBehaviour，不创建静态单例
7. **测试文件位置**：`Editor/` 子目录（无 asmdef），编译到 Assembly-CSharp-Editor
