# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-05-31
> **当前阶段**：Phase C-1 能力枪械系统

---

## 目标

从"三轨并行"架构收敛到纯 **BattleEntity + ISystem**，实现第一章 DEMO 所需的全部游戏功能。

## 当前架构状态

| 层       | 代表                                      | 状态                 |
|---------|-----------------------------------------|--------------------|
| Layer 1 | IComponent 模块（GameEntry.GetModule）      | 运行中，等 Phase E 删除   |
| Layer 2 | CharacterState / WeaponBase / EnemyBase | 仅剩少数旧引用，Phase E 删除 |
| Layer 3 | BattleEntity Framework + ISystem 服务     | **主力运行**           |

---

## Phase A：BattleEntity 框架 ✅（2026-05-27）

CommandDispatcherComponent、DecisionLayer (Player/Enemy)、EventDispatcherComponent、DeathComponent、HitReactionComponent、StatComponent、DamageReceiverComponent、BuffContainerComponent、AttackComponent。所有后续 ISystem 已在 FrameworkDefault.cs 注册。

---

## Phase B：实体迁移 ✅（2026-05-31）

**B-1 Player**：PlayerEntityFactory + PlayerMove/Gravity/Jump/Dash/Interact Executor + PlayerDecisionLayer + InputAdapter → BattleEntity 运行

**B-2 Enemy**：EnemyEntityFactory + EnemyJumpMove/Gravity/Jump/Face/Dash Executor + EnemyDecisionLayer(数据驱动FSM) + EnemyDetector + SphereGroundDetector → BattleEntity 运行。攻击检测用 OverlapSphere+SphereCast(Tag)，伤害/死亡管线适配纯C#组件。SquashStretchVisual 形变动画。

**B-3 Weapon**：WeaponEntityFactory + WeaponAssembly MB + ITriggerBehavior(半自动/全自动/霰弹/蓄力) + IAmmoSystem(弹匣/热量/无限) + WeaponFireDecision/Executor + WeaponAttackExecutor(Player侧路由)

### Phase B 剩余尾巴（→ Phase E）

- NormalOrbis prefab 更新（移除旧组件，挂新组件）
- 删除 SpawnTestEnemy() 临时测试代码
- 删除旧代码：EnemyBase、Module、Controller、EnemyAIController、CharacterState、Player.cs 分部类、WeaponBase 等
- 碰撞矩阵 Orbis/Player 交互确认
- CPoolManager 对象池接入
- WeaponFireExecutor 接入 IBulletSystem（子弹系统已迁移，待对接）
- SwitchWeapon 完整流程、配件系统（依赖 C-4 进化系统重设计）

---

## Phase C：游戏设计基础系统

**目标**：第一章 DEMO 核心游戏功能
**依赖**：Phase B 完成

### C-0：HUD 迁移到新 UI 框架 ✓

- [x] C-0a：事件桥接（Player.Health.cs + MagazineComponent → CEventRouter）
- [x] C-0b：MainHudFormLogic 事件订阅 + ViewData 数据流
- [x] C-0c：代码生成器修复（生命周期方法移到标记外）
- [x] C-0d：MainHudForm.prefab 更新（添加 TxtKillPoints/TxtLevel/TxtHealth/TxtArmor/TxtAmmo）→ **需 Unity Editor 手动操作**
- [x] C-0e：编译验证 + 场景测试

### C-1：能力枪械系统 🔄
**设计文档**：`.claude/docs/design/weapon-system.md`
**架构**：IAbilityWeaponSystem (ISystem) + IAbilityWeapon 策略接口 + Command 管道切 Executor

- [x] C-1a：Command 层（AbilitySecondaryCommand / WeaponVisibilityCommand / 接口 + CommandDispatcher 扩展）
- [x] C-1b：输入管道（IPlayerInputEvents 扩展 + InputHandler + PlayerInputAdapter + PlayerDecisionLayer）
- [x] C-1c：ICombatStateSystem（伤害计时器判定 + CombatStateChanged 事件）
- [x] C-1d：IAbilityWeapon + IAbilityWeaponSystem + AbilityWeaponExecutor + WeaponVisibilityExecutor
- [x] C-1e：武器轮盘 UI（AbilityWheelFormLogic + View，程序化扇形分割）→ **需 Unity Editor 创建 prefab**
- [ ] C-1f：匣纳之枪（StackGunAbility 已建，待完成核心逻辑：吸收/射出奥比斯）
- [ ] C-1g：连理之枪
- [ ] C-1h：裁径之枪

### C-2：奥比斯种类补齐 ❌

- [ ] C-2a~f：橙黄奥比斯（自爆）、飞行奥比斯、吞噬者奥比斯、相溶奥比斯、酸液奥比斯、重量等级系统

### C-3：经济与技能树 ❌

- [ ] C-3a~d：ICurrencySystem + ISkillTreeSystem

### C-4：枪械进化系统完善 ❌

- [ ] IEevolutionSystem 已有，需与配件系统一起重新设计

---

## Phase D：游戏内容层 ❌

D-1~D-4：机关系统、战斗反馈、剧情对话、关卡搭建

---

## Phase E：清理旧代码 ❌

删除 IComponent、CharacterState、WeaponBase、EnemyBase、Module、Controller 体系、Player.cs 分部类等旧架构残留

---

## 进度跟踪

| Phase | 状态 |
|-------|------|
| A: BattleEntity 框架 | ✅ |
| B: 实体迁移 (Player/Enemy/Weapon) | ✅ |
| C: 基础系统 | 🔄 C-1 能力枪械 |
| D: 内容层 | ❌ |
| E: 清理 | ❌ |

---

## 执行约定

1. **每个任务独立验证**：改完 → 编译 → 测试 → 标记完成
2. **会话结束时更新本文档**
3. **新会话开始时先读本文档**
4. **BattleEntity 组件**：纯 C#，不继承 MonoBehaviour，构造函数注入
5. **ISystem**：不继承 MonoBehaviour，不创建静态单例
6. **测试文件**：`Editor/` 子目录（无 asmdef）
7. **命名规范**：Decision / Dispatcher / Executor
