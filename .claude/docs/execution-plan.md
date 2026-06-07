# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-06-06
> **当前阶段**：Phase C-1 匣纳之枪收尾（能力武器管线验证）
> **需求清单**：`.claude/docs/requirements-checklist.md`（会议后用策划回答填入）

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

- [x] NormalOrbis prefab 更新（移除旧组件，挂新组件）
- [ ] 删除 SpawnTestEnemy() 临时测试代码
- [ ] 删除旧代码：EnemyBase、Module、Controller、EnemyAIController、CharacterState、Player.cs 分部类、WeaponBase 等
- [ ] 碰撞矩阵 Orbis/Player 交互确认
- [ ] CPoolManager 对象池接入
- [x] WeaponFireExecutor 接入 IBulletSystem（子弹系统已迁移，待对接）
- [ ] SwitchWeapon 完整流程

---

## Phase C：核心系统建设

### C-0：HUD 迁移到新 UI 框架 ✅

- [x] C-0a：事件桥接（Player.Health.cs + MagazineComponent → CEventRouter）
- [x] C-0b：MainHudFormLogic 事件订阅 + ViewData 数据流
- [x] C-0c：代码生成器修复（生命周期方法移到标记外）
- [x] C-0d：MainHudForm.prefab 更新（添加 TxtKillPoints/TxtLevel/TxtHealth/TxtArmor/TxtAmmo）→ **需 Unity Editor 手动操作**
- [x] C-0e：编译验证 + 场景测试

### C-1：能力枪械系统（匣纳之枪）🔄

**架构**：IAbilityWeaponSystem (ISystem) + IAbilityWeapon 策略接口 + Command 管道切 Executor

- [x] C-1a：Command 层（AbilitySecondaryCommand / WeaponVisibilityCommand / 接口 + CommandDispatcher 扩展）
- [x] C-1b：输入管道（IPlayerInputEvents 扩展 + InputHandler + PlayerInputAdapter + PlayerDecisionLayer）
- [x] C-1c：ICombatStateSystem（伤害计时器判定 + CombatStateChanged 事件）
- [x] C-1d：IAbilityWeapon + IAbilityWeaponSystem + AbilityWeaponExecutor + WeaponVisibilityExecutor
- [x] C-1e：武器轮盘 UI（AbilityWheelFormLogic + View，程序化扇形分割）→ **需 Unity Editor 创建 prefab**
- [x] C-1f：匣纳之枪核心逻辑（StackGunAbility：锥形吸收 + OrbProjectile 发射 + 碰撞伤害击飞 + 超时变回敌人）
- [ ] C-1g：匣纳之枪收尾（HUD 弹匣图标+LIFO顺序、吸收阵营过滤、发射碰撞后行为、吸满提示）→ **依赖策划回答 2.2**

> 连理之枪、裁径之枪延后至第一章跑通之后。

### C-2：效果原语层 + Buff 修复 ❌

**设计方向**：抽象一层效果原语（触发条件+效果），Buff 和配件共用。原语暂不做组合。

- [ ] C-2a：修复现有 Buff 系统 bug（OnRemove/Clear 不一致、maxStack 未生效、ActiveBuffs 清理）
- [ ] C-2b：定义 IEffectPrimitive 接口 + 原语注册
- [ ] C-2c：实现第一章需要的原语（属性修改、条件属性修改、命中 proc、击杀 proc、一次性效果）
- [ ] C-2d：Buff 瘦身 —— 移除"上帝类"职责，只保留临时可叠加效果
- [ ] C-2e：Buff Excel 配置 + 导入管线 → **依赖策划回答 2.6**
- [ ] C-2f：BuffSystemDefault 对接 Excel 数据

### C-3：枪械进化系统重写 ❌

**设计变更**：不再是"一把枪变形"，而是"每个阶段一套配装（主武器+配件），进化=切换到下一阶段配装"。

- [ ] C-3a：EvolutionConfig (ScriptableObject)：阶段数、每阶段负载上限、击杀阈值（全部可配置）
- [ ] C-3b：StageLoadout 数据模型：主武器ID + 配件ID列表 + 负载校验
- [ ] C-3c：阶段上限管理系统（全局事件解锁 + 持久化）
- [ ] C-3d：进化触发流程（击杀达标→提示Q→可选进化→切换配装）
- [ ] C-3e：阶段配装 UI（在存档点全息界面内，硬阻止同武器重复/空阶段）
- [ ] C-3f：默认配装兜底（首次使用时自动填第一把可用武器）
- [ ] C-3g：存档集成（击杀清零+回阶段一、配装保留）→ **依赖 D-3 存档点**

### C-4：配件系统 ❌

**设计方向**：独立于 Buff，使用效果原语。配件=永久装备，存档点配置。

- [ ] C-4a：IModuleEffect 接口 + AccessoryConfig（负载费用、武器专有限制）
- [ ] C-4b：配件装备/卸载逻辑 + 负载校验
- [ ] C-4c：配件 Excel 配置 + 导入 → **依赖策划回答 2.9**
- [ ] C-4d：配件获取（全局事件 GrantAccessory，由触发器投放）
- [ ] C-4e：配件配置 UI（在存档点全息界面内）→ **依赖 D-3**

### C-5：经济 + 技能树 ❌

- [ ] C-5a：ICurrencySystem（双货币：圆形数据 + 失落数据，转换率可配）
- [ ] C-5b：ISkillTreeSystem（可配置节点、前置依赖、节点效果用原语）
- [ ] C-5c：击杀计数 → 存档 → 转换货币流程
- [ ] C-5d：技能树 Excel 配置 + 导入 → **依赖策划回答 2.7**
- [ ] C-5e：技能树 UI（在存档点全息界面内）→ **依赖 D-3**

### C-6：奥比斯种类补齐（第一章）❌

第一章只需要 4 种：蓝色（已有）、橙黄自爆、吞噬者、酸液。飞行和相溶留到后续章节。

- [ ] C-6a：橙黄奥比斯（自爆前预警、自爆范围伤害、是否伤其他怪、自爆后死亡）
- [ ] C-6b：吞噬者奥比斯（捕食锁定/弹道判定、被吞期间玩家能做什么、喷出方向）
- [ ] C-6c：酸液奥比斯（酸液地面生成、溶解机关、喷吐酸液）
- [ ] C-6d：重量等级系统（int 分档，影响后续连理之枪交互，先做基础定义）

---

## Phase D：内容层 & 工具

### D-1：场景事件触发器工具 ❌

**目标**：让策划在场景里通过下拉框选择全局事件并配置参数，无需代码。

- [ ] D-1a：`SceneEventTrigger` 组件 + `[SerializeReference]` 多态 EventParams
- [ ] D-1b：三种触发器类型：碰撞触发 / 手动交互 / 事件监听
- [ ] D-1c：Custom Editor（选事件 → 动态切换参数字段）
- [ ] D-1d：事件 ID 管理（框架事件枚举 + 投放类事件配置表）→ **依赖策划回答 2.1**
- [ ] D-1e：接收器端（事件→行为）：开门、刷怪、播对话、发道具等

### D-2：对话系统 ❌

- [ ] D-2a：Excel 导入管线 + 编辑器下拉列表
- [ ] D-2b：对话播放器（底部字幕条：说话者+内容、可配时长、跳过）
- [ ] D-2c：单条/序列/分支选项三种结构
- [ ] D-2d：三种暂停模式（完全暂停/可移动/完全自由）
- [ ] D-2e：闲聊系统（定时随机池 + 事件触发池）
- [ ] D-2f：对话触发器（场景内选对话 ID）→ **依赖 D-1**

### D-3：存档点全息界面 ❌

**目标**：World Space 3D UI（死亡搁浅风格），靠近按 F 弹出，整合所有管理功能。

- [ ] D-3a：World Space Canvas + 全息视觉效果
- [ ] D-3b：存档交互（手动存档、休息后回阶段一+货币转换）
- [ ] D-3c：集成：技能树 / 阶段配装 / 配件配置入口
- [ ] D-3d：自动存档节点（Boss 击杀等全局事件触发）
- [ ] D-3e：存档点激活/交互流程 → **依赖策划回答 2.4**

### D-4：第一章关卡搭建 ❌

- [ ] D-4a：机关 Prefab（酸蚀材质、可破坏墙壁、能量吸收装置、可推物体、拉杆/按钮）
- [ ] D-4b：敌人生成点 + 巡逻路径配置
- [ ] D-4c：关卡流程编排（依赖 2.1 策划决定：各自挂载 vs 关卡脚本）
- [ ] D-4d：完整第一章白盒 + 游戏测试

### D-5：战斗反馈与演出 ❌

- [ ] D-5a：准星反馈（命中变色、击杀形变、散布大小）
- [ ] D-5b：敌人受击表现（动画/闪白/缩放）
- [ ] D-5c：枪械进化演出（短时缓+冲击波+外观切换）
- [ ] D-5d：能力枪 VFX/SFX（临时占位或正式）

---

## Phase E：清理旧代码 ❌

- [ ] 删除 IComponent、CharacterState、WeaponBase、EnemyBase、Module、Controller 体系
- [ ] 删除 Player.cs 分部类等旧架构残留
- [ ] 删除 SpawnTestEnemy() 临时测试代码
- [ ] 删除 Map/ 废弃实现
- [ ] CPoolManager 对象池接入
- [ ] 碰撞矩阵 Orbis/Player 交互确认

---

## 进度跟踪

| Phase | 状态 |
|-------|------|
| A: BattleEntity 框架 | ✅ |
| B: 实体迁移 (Player/Enemy/Weapon) | ✅ |
| C-0: HUD 迁移 | ✅ |
| C-1: 能力枪械（匣纳之枪） | 🔄 收尾 |
| C-2: 效果原语 + Buff 修复 | ❌ |
| C-3: 枪械进化系统重写 | ❌ |
| C-4: 配件系统 | ❌ |
| C-5: 经济 + 技能树 | ❌ |
| C-6: 敌人补齐（第一章） | ❌ |
| D-1: 触发器工具 | ❌ |
| D-2: 对话系统 | ❌ |
| D-3: 存档点全息界面 | ❌ |
| D-4: 关卡搭建 | ❌ |
| D-5: 战斗反馈 | ❌ |
| E: 清理旧代码 | ❌ |

### 依赖关系

```
C-2 (原语层) ──→ C-3 (进化) ──→ D-3 (存档UI)
              ├─→ C-4 (配件) ──→ D-3
              └─→ C-5 (技能树) ──→ D-3
C-1 (匣纳) ──→ D-2 (对话需要能力枪管线稳定)
D-1 (触发器工具) ──→ D-2 (对话需要触发器)
                 └─→ D-4 (关卡搭建需要触发器)
C-6 (敌人) ──→ D-4 (关卡需要敌人)
```

---

## 执行约定

1. **每个任务独立验证**：改完 → 编译 → 测试 → 标记完成
2. **会话结束时更新本文档**
3. **新会话开始时先读本文档**
4. **BattleEntity 组件**：纯 C#，不继承 MonoBehaviour，构造函数注入
5. **ISystem**：不继承 MonoBehaviour，不创建静态单例
6. **测试文件**：`Editor/` 子目录（无 asmdef）
7. **命名规范**：Decision / Dispatcher / Executor
