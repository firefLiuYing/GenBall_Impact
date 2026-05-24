# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-05-24
> **当前阶段**：Phase A

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

StatComponent, DamageReceiverComponent, BuffContainerComponent, AttackComponent

### 缺失的 BattleEntity 组件

CommandDispatcher, DecisionLayer（PlayerDecision + EnemyDecision）, EventDispatcher（待讨论）

---

## Phase A：完成 BattleEntity 框架

**目标**：让 BattleEntity 能完整替代 CharacterState
**范围**：`Assets/Scripts/GenBall/BattleSystem/Framework/`
**依赖**：无

### A-1：CommandDispatcher 组件

**状态**：❌ 未开始

**输入**：MoveCommand, RotateCommand, AttackCommand, FaceDirectionCommand 已定义（`BattleSystem/Command/CharacterCommand.cs`）
**执行器接口**：IMove, IRotate, IAttack, IFaceDirection 已定义

**需要做的**：
- [ ] 创建 `CommandDispatcherComponent`（纯 C#，不继承 MB）
  - 持有 `Dictionary<Type, object>` 映射命令类型 → 执行器
  - `RegisterExecutor<TCommand>(IExecutor executor)` 注册
  - `Dispatch(ICommand command)` 查找执行器并执行
  - 可选的命令队列支持（批量执行）
- [ ] 创建 `CommandDispatcherComponentTests` 测试
- [ ] 更新 `battle-entity-architecture.md` 文档

**文件清单**：
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/CommandDispatcherComponent.cs`
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/Editor/CommandDispatcherComponentTests.cs`

### A-2：DecisionLayer 组件

**状态**：❌ 未开始

**设计**：
- `IDecisionLayer` 接口：`void MakeDecision()` — 每帧由 BattleEntity 或 FrameUpdate 调用
- `PlayerDecisionLayer`：读取 Unity InputSystem → 生成命令 → Dispatch 到 CommandDispatcher
- `EnemyDecisionLayer`：运行 AI FSM → 生成命令 → Dispatch 到 CommandDispatcher

**需要做的**：
- [ ] 定义 `IDecisionLayer` 接口
  - `void MakeDecision(float deltaTime)` — 读取输入/AI状态，产出一组 ICommand
- [ ] 创建 `PlayerDecisionLayer`（从旧 `Player.Control.cs` + `Player.Fsm.cs` 提取逻辑）
  - 持有 CommandDispatcher 引用
  - 读取 InputController 状态
  - 将输入翻译为 MoveCommand / JumpCommand / DashCommand / FireCommand
- [ ] 创建 `EnemyDecisionLayer`（从旧 `EnemyAIController` + `AI/*State.cs` 提取逻辑）
  - 持有 CommandDispatcher 引用
  - 运行 AI FSM（Wander → Chase → Attack → Back → Death）
  - 每帧产出对应的命令
- [ ] 创建测试
- [ ] 更新架构文档

**文件清单**：
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/IDecisionLayer.cs`
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/PlayerDecisionLayer.cs`
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/EnemyDecisionLayer.cs`
- 新建 `Assets/Scripts/GenBall/BattleSystem/Framework/Editor/DecisionLayerTests.cs`

### A-3：EventDispatcher（讨论 + 方案设计）

**状态**：❌ 待讨论

**背景**：组件间通信（如 StatComponent 属性变化通知 DamageReceiverComponent 重新计算 MaxHealth）需要一个低耦合方案。复杂度高，已约定单独讨论。

**需要讨论的问题**：
1. BattleEntity 内部组件间通信用 EventDispatcher，还是直接通过 `entity.Get<T>()` 访问？
2. 如果需要 EventDispatcher，放在 BattleEntity 上还是作为独立组件？
3. 和全局 CEventRouter 的关系？（现有 BuffTickSystem 已经订阅全局伤害/死亡事件）

**输出**：一份 `event-dispatcher-design.md` 文档（含方案对比 + 决定），如决定实现则加入 A-3 任务

---

## Phase B：迁移实体到 BattleEntity

**目标**：Player、Enemy、Weapon 全部跑在 BattleEntity 上。删除 CharacterState。
**依赖**：Phase A 完成

### B-1：Player 迁移

**状态**：❌ 未开始

**当前旧代码位置**：
- `GenBall/Player/Player.cs` — 主入口，MonoBehaviour
- `GenBall/Player/Player.Control.cs` — 输入路由
- `GenBall/Player/Player.Fsm.cs` — 状态机（Move/Jump/Dash）
- `GenBall/Player/Player.Health.cs` — 血量/护甲/IDamageable
- `GenBall/Player/Player.Physics.cs` — 移动/地面检测
- `GenBall/Player/Player.Weapon.cs` — 武器装备
- `GenBall/Player/Player.Countdown.cs` — CD计时
- `GenBall/Player/States/PlayerMoveState.cs` — 移动+视角
- `GenBall/Player/States/PlayerJumpState.cs` — 变高跳跃
- `GenBall/Player/States/PlayerDashState.cs` — 冲刺

**迁移策略**：
- [ ] B-1a：用 BattleEntity 装配 Player
  - `BattleEntity` + `StatComponent(MaxHealth, Attack)` + `DamageReceiverComponent`
  - `BuffContainerComponent` + `AttackComponent(WeaponDamageCalculator)`
  - `CommandDispatcherComponent` + `PlayerDecisionLayer`
- [ ] B-1b：将移动/跳跃/冲刺逻辑从 Player 分部类提取为 BattleEntity 组件
  - `PlayerMoverComponent` → 实现 IMove
  - `PlayerJumperComponent` → 变高跳跃逻辑（从 PlayerJumpState 提取）
  - `PlayerDasherComponent` → 冲刺逻辑（从 PlayerDashState 提取）
  - `PlayerGroundDetectComponent` → 地面检测
  - `PlayerWeaponHolderComponent` → 武器管理
- [ ] B-1c：PlayerDataConfig 对接（PlayerConfigSo → AppSettingsConfig）
- [ ] B-1d：创建 Player 装配工厂（`PlayerEntityFactory` 或类似）
- [ ] B-1e：验证：Launcher 场景启动 → 玩家正常移动/跳跃/冲刺/射击
- [ ] B-1f：删除旧 `Player.cs` 及所有分部类

### B-2：Enemy 迁移

**状态**：❌ 未开始

**当前旧代码位置**：
- `GenBall/Enemy/EnemyBase.cs` — 旧模块式敌人
- `GenBall/Enemy/Controller/EnemyAIController.cs` — AI FSM
- `GenBall/Enemy/AI/` — 各种 AI 状态
- `GenBall/BattleSystem/Character/CharacterState.cs` — 旧角色基类

**迁移策略**：
- [ ] B-2a：用 BattleEntity 装配 Enemy
  - `BattleEntity` + `StatComponent(MaxHealth)` + `DamageReceiverComponent`
  - `BuffContainerComponent` + `CommandDispatcherComponent` + `EnemyDecisionLayer`
- [ ] B-2b：创建 `EnemyMoverComponent` → 实现 IMove
- [ ] B-2c：创建 `EnemyAttackComponent` → 实现 IAttack
- [ ] B-2d：将 AI FSM 逻辑迁移到 EnemyDecisionLayer 内部
- [ ] B-2e：创建 Enemy 装配工厂
- [ ] B-2f：验证：敌人生成→巡逻→发现玩家→追击→攻击→死亡
- [ ] B-2g：删除旧 `EnemyBase`、旧模块系统、`CharacterState`

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
| A: BattleEntity 框架 | 3 | 0 | 0% |
| B: 实体迁移 | 3 (含 17 子任务) | 0 | 0% |
| C: 基础系统 | 4 (含 20 子任务) | 0 | 0% |
| D: 内容层 | 4 (含 16 子任务) | 0 | 0% |
| E: 清理 | 1 (含 12 子任务) | 0 | 0% |

---

## 执行约定

1. **每个任务独立验证**：改完一个任务 → 编译 → 跑测试 → 标记完成
2. **会话结束时更新本文档**：勾选已完成任务，更新进度表
3. **新会话开始时先读本文档**：了解当前进度和下一个任务
4. **Phase A 优先**：在 BattleEntity 框架完整之前，不开始迁移
5. **BattleEntity 组件原则**：纯 C# 类，不继承 MonoBehaviour，通过构造函数注入 BattleEntity 引用
6. **ISystem 原则**：业务系统不继承 MonoBehaviour，不创建静态单例
7. **测试文件位置**：`Editor/` 子目录（无 asmdef），编译到 Assembly-CSharp-Editor
