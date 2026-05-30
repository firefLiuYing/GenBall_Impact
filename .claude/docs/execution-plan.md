# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-05-30
> **当前阶段**：Phase B 收尾 / Phase C 准备

---

## 目标

从当前的"三轨并行"架构收敛到纯 **BattleEntity + ISystem** 架构，同时实现第一章 DEMO 所需的全部游戏功能。

## 当前架构状态速查

| 层 | 代表 | 状态 |
|----|------|------|
| Layer 1 | IComponent 模块（GameEntry.GetModule） | 运行中，已被 ISystem 替代，等删除 |
| Layer 2 | CharacterState / Player / WeaponBase / EnemyBase | 运行中，Player/Weapon 已迁移到 BattleEntity |
| Layer 3 | BattleEntity Framework + ISystem 服务 | 投产中 |

### 已完成的 ISystem 注册（FrameworkDefault.cs）

IConfigProvider, ISaveService, IBuffRegistry, IBuffTickSystem, IEntityUpdateSystem,
IDamageSystem, IDeathSystem, IInteractSystem, ISceneStateSystem, ISceneLoadSystem,
ITeleportSystem, IPauseSystem, IGameManagerSystem, ILaunchSystem, IPlayerSystem,
IBulletSystem, IEvolutionSystem, ISceneExecutorSystem, IGMCommandSystem, ICameraSystem

### 已完成的 BattleEntity 组件

StatComponent, DamageReceiverComponent, BuffContainerComponent, AttackComponent,
CommandDispatcherComponent, DecisionLayer (Player/Enemy), EventDispatcherComponent,
DeathComponent, HitReactionComponent

---

## Phase A：BattleEntity 框架 ✅

**状态**：100% 完成（2026-05-27）

- A-1：CommandDispatcherComponent — 仲裁+缓冲+瞬时路由
- A-2：DecisionLayer — PlayerDecisionLayer + EnemyDecisionLayer
- A-3：EventDispatcherComponent + DeathComponent + Shield 统合

---

## Phase B：实体迁移到 BattleEntity

**目标**：Player、Enemy、Weapon 全部跑在 BattleEntity 上。删除旧代码。
**依赖**：Phase A 完成

### B-1：Player 迁移 ✅

**状态**：代码完成（2026-05-30），仅剩运行时验证

已完成全部核心：PlayerEntityFactory 装配、Executor 组件（Jump/Dash/Gravity/Interact）、
PlayerDecisionLayer、InputAdapter、PlayerConfig、HitReaction、Death、ICameraSystem。

**剩余尾巴**：
- [ ] B-1e：验证 Launcher 场景全流程
- [ ] B-1f：删除旧 Player.cs 及分部类（等 B-2 完成下游引用迁移）

### B-2：Enemy 迁移

**状态**：暂时跳过（2026-05-30）

核心已完成（EnemyEntityFactory、SceneExecutorSystemDefault 接入），
剩 EnemyDecisionLayer aiConfig 数据驱动化。待 Phase C 重新拾起。

### B-3：Weapon 迁移 ← 本次会话推进

**状态**：✅ 架构代码完成（2026-05-30）

#### 架构：预制体驱动 + 工厂装配

```
武器 prefab
  └── WeaponAssembly (MB)           ← 策划在 Inspector 配置
        ├── Archetype (enum)        ← 决定 ITriggerBehavior
        ├── AmmoType (enum)         ← 决定 IAmmoSystem
        └── 参数 (Damage, FireInterval, MagazineCapacity, ...)

WeaponEntityFactory.Assemble(go)    ← 读 WeaponAssembly → 创建组件 → BattleEntity
```

#### 武器内部决策/执行分层

```
WeaponFireDecision (决策,武器内)   ← 冷却 + ITriggerBehavior.Evaluate()
  → WeaponFireExecutor (执行,武器内)  ← 子弹生成 (TODO: 等子弹迁移)
```

#### Player 侧路由

```
PlayerDecisionLayer → AttackCommand(ButtonState)
  → CommandDispatcherComponent → WeaponAttackExecutor (执行)
    → weapon.Get<IWeaponTrigger>().SetTriggerState()
    → weapon.TryGet<IReloadable>()?.Reload()
```

#### 新增文件

| 文件 | 说明 |
|------|------|
| `Weapons/WeaponAssembly.cs` | MB，挂在 prefab 上定义组件组合+参数 |
| `Weapons/Components/IWeaponTrigger.cs` | 扳机接口 |
| `Weapons/Components/ITriggerBehavior.cs` | 策略接口 + FireRequest |
| `Weapons/Components/SemiAutoTriggerBehavior.cs` | 半自动 |
| `Weapons/Components/FullAutoTriggerBehavior.cs` | 全自动 |
| `Weapons/Components/ShotgunTriggerBehavior.cs` | 霰弹 |
| `Weapons/Components/ChargeTriggerBehavior.cs` | 蓄力 |
| `Weapons/Components/IAmmoSystem.cs` | 弹药抽象 + IConsumableAmmo + IReloadable + AmmoDisplayInfo |
| `Weapons/Components/MagazineComponent.cs` | 弹匣 |
| `Weapons/Components/HeatComponent.cs` | 热量 |
| `Weapons/Components/InfiniteAmmoComponent.cs` | 无限 |
| `Weapons/Components/SpreadComponent.cs` | 散布 |
| `Weapons/Components/WeaponFireDecision.cs` | 武器内决策 |
| `Weapons/Components/WeaponFireExecutor.cs` | 武器内执行（子弹留空） |
| `Weapons/Factory/WeaponEntityFactory.cs` | 工厂 |
| `Player/Executor/WeaponAttackExecutor.cs` | Player 执行层，IAttack+IReload+ISwitchWeapon |

#### 修改文件

`Player/PlayerEntityFactory.cs` — WeaponExecutor MB → WeaponAttackExecutor 纯 C#，装配默认手枪

#### 武器生命周期管理（已讨论，待 Phase C 实现）

| 场景 | 负责方 | 操作 |
|------|--------|------|
| 首次进游戏 | PlayerEntityFactory | 装默认手枪 |
| 复活 | 死亡/重生系统 | 装当前进化阶段武器 |
| 进化触发 | SwitchWeaponCommand → WeaponAttackExecutor | 装下一阶段武器 |
| 进化配置变更 | 进化/配件系统 | 重新装当前阶段武器 |

所有路径最终调用 `WeaponAttackExecutor.EquipWeapon(BattleEntity)`。
武器变更时通过 BattleEntity 的 `EventDispatcherComponent` 发射 `WeaponChanged` 事件→UI 刷新。
武器 HUD 专用事件（弹匣/热量/蓄力）使用 UI 系统自有事件机制，Phase C 实现。

#### B-3 剩余尾巴（待其他 Phase）

| 待实现 | 依赖 |
|--------|------|
| WeaponFireExecutor 接入 IBulletSystem.FireBullet() | 子弹系统迁移 |
| SwitchWeapon 完整流程（进化阶段→武器配置→切换） | 进化系统重新设计 |
| 配件挂载到 BuffContainerComponent | 配件系统重新设计 |
| 武器 HUD 切换（弹匣/热量/蓄力） | UI 系统事件 |

---

## Phase C：游戏设计基础系统

**目标**：实现第一章 DEMO 的核心游戏功能
**依赖**：Phase B 完成

### C-1：能力枪械系统

**状态**：❌ 未开始
**设计文档**：`.claude/docs/design/weapon-system.md`

- [ ] C-1a：创建 `IAbilityWeaponSystem : ISystem`
- [ ] C-1b：实现 `AbilityWeaponSystemDefault`
- [ ] C-1c：创建武器轮盘 UI + 切换交互
- [ ] C-1d：实现 匣纳之枪
- [ ] C-1e：实现 连理之枪
- [ ] C-1f：实现 裁径之枪

### C-2：奥比斯种类补齐

**状态**：仅 NormalOrbis 存在

- [ ] C-2a：橙黄奥比斯（自爆）
- [ ] C-2b：飞行奥比斯
- [ ] C-2c：吞噬者奥比斯
- [ ] C-2d：相溶奥比斯
- [ ] C-2e：酸液奥比斯
- [ ] C-2f：重量等级系统

### C-3：经济与技能树

**状态**：❌ 未开始

- [ ] C-3a~d：ICurrencySystem + ISkillTreeSystem

### C-4：枪械进化系统完善

**状态**：IEvolutionSystem 已有，不完整。需与配件系统一起重新设计。

- [ ] C-4a~f：进化阶段+模块+形态切换

---

## Phase D：游戏内容层

**状态**：❌ 未开始

D-1~D-4：机关系统、战斗反馈、剧情对话、关卡搭建

---

## Phase E：清理旧代码

**状态**：❌ 未开始

E-1~E-12：删除 IComponent、CharacterState、WeaponBase、EnemyBase、Player.cs 等旧架构残留

---

## 进度跟踪

| Phase | 状态 |
|-------|------|
| A: BattleEntity 框架 | ✅ 100% |
| B-1: Player 迁移 | ✅ 代码完成，仅剩验证+旧代码删除 |
| B-2: Enemy 迁移 | 暂时跳过（核心完成，剩 aiConfig 数据驱动） |
| B-3: Weapon 迁移 | ✅ 架构代码完成，剩子弹/进化/配件对接 |
| C: 基础系统 | ❌ 0% |
| D: 内容层 | ❌ 0% |
| E: 清理 | ❌ 0% |

---

## 执行约定

1. **每个任务独立验证**：改完一个任务 → 编译 → 跑测试 → 标记完成
2. **会话结束时更新本文档**：勾选已完成任务，更新进度表
3. **新会话开始时先读本文档**：了解当前进度和下一个任务
4. **BattleEntity 组件原则**：纯 C# 类，不继承 MonoBehaviour，通过构造函数注入 BattleEntity 引用
5. **ISystem 原则**：业务系统不继承 MonoBehaviour，不创建静态单例
6. **测试文件位置**：`Editor/` 子目录（无 asmdef），编译到 Assembly-CSharp-Editor
7. **武器组装**：预制体 + WeaponAssembly MB → WeaponEntityFactory.Assemble()
8. **命名规范**：Decision（决策）/ Dispatcher（分发）/ Executor（执行）
