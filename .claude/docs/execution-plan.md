# 长期执行计划

> **用途**：跨会话持久化计划。每完成一个任务更新状态。新会话启动时先读此文档。
> **最后更新**：2026-06-28
> **当前阶段**：Phase D-3 存档系统 & 死亡复活闭环（核心循环打通）
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

### D-1：场景事件触发器工具 ✅（2026-06-21）

**目标**：让策划在场景里通过下拉框选择全局事件并配置参数，无需代码。

- [x] D-1a：`TriggerVolume` 组件（`IScenePlaceable`）+ `EventAdapter` + `[SerializeReference]` 多态 `EventParameterBase`
- [x] D-1b：三种触发器类型（`TriggerMode`）：`Collision` / `Interact` / `EventListener`
- [x] D-1c：Custom Editor（`TriggerVolumeEditor` + `SearchableEventPopup`，选事件 → 动态切换参数字段）
- [x] D-1d：事件 ID 管理（`GlobalEventId` 枚举 1-5999 + `PlacedEventTable` 投放类 >=6000，CSV 导入 + 冲突校验）
- [x] D-1e：接收器端（`SceneEventOrchestrator`：事件→行为映射，`SpawnEnemy` 已完成；其余 handler 待依赖系统就绪后添加）
- [x] D-1f：烘焙管线（`BakingPipeline`，扫描 `IScenePlaceable` → 序列化到 `SceneConfigCollection.asset`）
- [x] D-1g：运行时生成（`SceneExecutorSystemDefault.SpawnTriggers()` → `RuntimeEventTrigger`）

### D-2：对话系统 ❌

- [ ] D-2a：Excel 导入管线 + 编辑器下拉列表
- [ ] D-2b：对话播放器（底部字幕条：说话者+内容、可配时长、跳过）
- [ ] D-2c：单条/序列/分支选项三种结构
- [ ] D-2d：三种暂停模式（完全暂停/可移动/完全自由）
- [ ] D-2e：闲聊系统（定时随机池 + 事件触发池）
- [ ] D-2f：对话触发器（场景内选对话 ID）→ **依赖 D-1**

### D-3：存档系统 & 死亡复活闭环 🔄

**边界**：打通 存档→死亡→复活→恢复 的完整核心循环。全息界面的子功能（配装/技能树/配件）只做入口占位，等对应系统就绪后再接入。

**设计决策摘要**：
- 用户设置（音量/键位等）独立于存档槽位持久化，所有槽位共享
- 增量保存：`GameManager.UpdateSaveFields(providerKey, fields)`，按系统隔离字段
- 全量保存触发：手动休息、传送前、章节切换
- 死亡：YOU DIED → 自动重载场景 → 复活到存档点
- 复活不读磁盘存档，用内存中的 `PlayerSaveData` + `ISceneStateSystem` 场景状态
- 字段 key 用 const string 维护，放到 `SaveFieldKeys` 类

#### D-3a：用户设置独立存储 ✅（2026-06-28）

**目标**：独立于 6 个存档槽位的用户级配置文件，所有槽位共享。

- [x] D-3a1：`UserSettings` 数据模型（音量/鼠标灵敏度/键位映射/语言等，第一章先用音量+灵敏度）
- [x] D-3a2：`UserSettingsStorage` — 序列化到 `user_settings.json`，读/写接口
- [x] D-3a3：`FrameworkDefault` 注册，接入音频/输入系统

> 依赖：无

#### D-3b：增量保存机制 + 字段常量 ✅（2026-06-28）

**目标**：在现有 `GameManager` 上暴露字段级增量更新接口。

- [x] D-3b1：`GameManager.UpdateSaveFields(string providerKey, Dictionary<string, string> fields)` — 读当前槽位 GameData → 更新对应 key 的 DataBlock → 写回磁盘
- [x] D-3b2：`SaveFieldKeys` 常量类 — 维护所有增量更新的字段名，按 provider 分组，避免拼写错误
- [x] D-3b3：字段隔离校验 — `UpdateSaveFields` 只允许更新 providerKey 匹配的 DataBlock，跨系统更新打 logWarning 忽略
- [x] D-3b4：单元测试

> 依赖：D-1 事件总管，无其他硬依赖

#### D-3c：存档槽选择 UI

**目标**：标题画面选"读取存档"时不硬编码 index=0，而是展示存档槽列表。

- [x] D-3c1：`SaveSlotForm` UI Form — 展示槽位列表（创建时间、总时长、场景名），选中后传 `saveIndex` 进 LoadGame 流程
- [x] D-3c2：`StartFormLogic` — `OnLoadGame()` 改为打开 SaveSlotForm 而非硬编码 index=0
- [x] D-3c3：`CanContinue` 修复 — `RefreshCanContinue()` 检查是否有有效存档槽位，灰显/点亮继续按钮
- [x] D-3c4：`SaveSlotViewData` — 槽位元数据 ViewData

> 依赖：现有 StartForm + ISaveService.GetSaveSlotDatas()

#### D-3d：存档点状态区分 ✅（2026-06-28）

**目标**：存档点有"未解锁"和"已解锁"两种状态，视觉和行为都不同。

- [x] D-3d1：`SavePoint` 组件扩展 — `IsUnlocked` 字段 + `SavePointIndex` + 视觉占位（`ApplyVisualState()`）
- [x] D-3d2：未解锁交互流程 — 靠近按 F → 标记 `IsUnlocked = true` → 同步 ISceneStateSystem + MapSaveDataProvider → `GameManager.UpdateSaveFields("Map", ...)` 增量保存
- [x] D-3d3：已解锁交互流程 — 靠近按 F → `OpenBonfireUI()` 占位（→ D-3e）
- [x] D-3d4：`SavePoint.Interact()` 内根据 `IsUnlocked` 分支 `Unlock()` / `OpenBonfireUI()`
- [x] D-3d5：`SpawnBonfires()` 启动时读取 ISceneStateSystem 检查已有解锁状态，传入 `SetConfig()`

#### D-3e：存档点全息界面（含 UI 框架 World Space Canvas 支持）

**目标**：世界空间 3D UI，已解锁存档点交互后弹出。需要先改造 UI 框架层支持 World Space Canvas。

##### D-3e-1：UI 框架层改造（Assets/Scripts/Yueyn/UI/）

**关键文件**：
- `UIFormScript.cs` — `InitializeCanvas()` 第 224-272 行硬编码 ScreenSpaceCamera，是唯一最关键改动点
- `UIManager.cs` — 表单实例化/父节点分配/层级设置
- `BusinessFormLogic.cs` — 表单创建流程
- `UIFormType.cs` — 枚举扩展
- `UiBindingCodeGenerator.cs` — 代码生成器模板

**具体改动**：

- [ ] D-3e1a：`UIFormType` 枚举 — 新增 `WorldSpace` 类型
- [ ] D-3e1b：`UIFormScript.InitializeCanvas()` — 检查 FormType，WorldSpace 分支：`RenderMode.WorldSpace`、跳过 `CanvasScaler`、不强制 `overrideSorting`
- [ ] D-3e1c：`UIManager.OpenForm()` — WorldSpace 表单：不挂到 Persistent/Popup/Transition RectTransform 下，放到 WorldUIRoot（场景中独立节点）；跳过 `SetUILayerRecursively`（用场景层而非 UI 层）
- [ ] D-3e1d：`BusinessFormLogic` — 新增 `IsWorldSpace` 虚属性（默认 false），`OnCreateInternal()` 中传入 FormType 到 OpenForm
- [ ] D-3e1e：`UiViewBinding` + 代码生成器 — `FormTypeEnum` 加 `WorldSpace`；`EnsurePrefabComponents()` 对 WorldSpace 表单跳过 CanvasScaler 添加
- [ ] D-3e1f：编译验证 + 现有屏幕空间 UI 回归（确保 StartForm/LoadingForm/MainHud 不受影响）

##### D-3e-2：BonfireForm 实现（Assets/Scripts/GenBall/UI/BonfireForm/）

- [ ] D-3e2a：`BonfireForm.prefab` — World Space Canvas，全息视觉效果（透明/发光材质），定位在存档点前方
- [ ] D-3e2b：`BonfireFormView` + `BonfireFormLogic` — WorldSpace FormType，UI 事件绑定
- [ ] D-3e2d：菜单项 — "休息"按钮（调 `GameManager.SaveGame()` + 战斗状态重置）
- [ ] D-3e2e：子系统入口占位 — 配装/技能树/配件 按钮（灰显，文字提示"开发中"）
- [ ] D-3e2f：关闭交互 — 远离存档点自动关闭 / 按 ESC 关闭
- [ ] D-3e2g：`IInteractSystem` 集成 — 超出交互范围自动关闭菜单

> 依赖：D-3d（存档点状态）、D-3e-1（UI 框架层改造）、现有 UI 框架

#### D-3f：IRespawnSystem 死亡复活

**目标**：处理玩家死亡→复活完整流程，通过事件总线解耦其他系统的响应。

- [ ] D-3f1：`IRespawnSystem` 接口定义（`void StartDeathFlow()`）
- [ ] D-3f2：`RespawnSystemDefault` 实现：
  - 发送 `PlayerDied` 事件（通过 `CEventRouter`）
  - 等待死亡表现完成（延迟/动画）
  - 重载当前场景（`ISceneLoadSystem`）
  - 场景加载完成后，用内存中的 `PlayerSaveData` 确定生成位置
  - 发送 `PlayerRespawned` 事件
- [ ] D-3f3：`PlayerDied` / `PlayerRespawned` 事件注册到 `GlobalEventId` 枚举 + 代码生成
- [ ] D-3f4：死亡触发点 — `PlayerDeathHandler.OnDeath()` 中调 `IRespawnSystem.StartDeathFlow()`
- [ ] D-3f5：`DeathInfo` 扩展 — 区分玩家死亡和敌人死亡，避免敌人死亡触发复活流程

> 依赖：DeathComponent + DeathSystemDefault、ISceneLoadSystem、PlayerSaveData（内存）

#### D-3g：战斗状态重置

**目标**：复活/休息时，各系统通过 `PlayerRespawned` 事件重置自己的运行时数据。

- [ ] D-3g1：`PlayerRespawned` 事件 — 各系统订阅并重置：
  - `IHealth` → HP 回满
  - 护甲系统 → 护甲回满
  - `IMagazineComponent` → 弹匣回满
  - 击杀计数 → 转换为圆形数据，清零（→ **依赖 C-5 货币系统，目前发事件占位**）
  - 进化阶段 → 回阶段一
  - 能力枪 → 清空
  - `IBuffContainer` → 清除所有临时 Buff
- [ ] D-3g2：`ICombatStateSystem` 扩展 — 提供 `ResetCombatState()` 方法，统一处理战斗状态重置
- [ ] D-3g3：休息时也触发同一重置流程（全息界面点"休息" = `SaveGame()` + `ResetCombatState()`）

> 依赖：D-3f（PlayerRespawned 事件）、各战斗系统现有接口

#### D-3h：全量存档触发集成

**目标**：在所有需要全量存档的时机调用 `GameManager.SaveGame()`。

- [ ] D-3h1：手动休息 — 全息界面"休息"按钮 → `GameManager.SaveGame()`
- [ ] D-3h2：传送 — `ITeleportSystem.DoTeleport()` 传送前调 `SaveGame()`
- [ ] D-3h3：章节切换 — 章节过渡触发器调用 `SaveGame()`
- [ ] D-3h4：`GameManager.SaveGame()` 内更新 `PlayerSaveData.lastSceneName` + `lastSavePointIndex` 后写入

> 依赖：D-3b（增量保存已就绪）、ITeleportSystem、关卡章节触发器

#### D-3i：死亡 UI（YOU DIED）

**目标**：死亡后黑屏+文字提示，自动过渡到复活。

- [ ] D-3i1：`DeathScreenFormLogic` + `DeathScreenFormView` — 监听 `PlayerDied` 事件，显示 YOU DIED 文字
- [ ] D-3i2：文字淡入动画（约 1 秒）+ 保持（约 2 秒）+ 淡出（约 1 秒）→ 自动关闭
- [ ] D-3i3：淡出完成后发 UI 事件，RespawnSystem 监听到后执行场景重载

> 依赖：D-3f（PlayerDied 事件）、现有 UI 框架

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
| D-1: 触发器工具 | ✅ |
| D-2: 对话系统 | ❌ |
| D-3: 存档系统 & 死亡复活闭环 | 🔄 |
| D-4: 关卡搭建 | ❌ |
| D-5: 战斗反馈 | ❌ |
| E: 清理旧代码 | ❌ |

### 依赖关系

```
D-3 内部依赖：
  D-3b (增量保存) ←─ D-3d (存档点解锁, 增量)
  D-3d (存档点状态) ←─ D-3e (全息界面)
  D-3b (增量保存) ←─ D-3h (全量存档集成)
  D-3f (复活系统) ←─ D-3g (战斗状态重置)
  D-3f (复活系统) ←─ D-3i (死亡 UI)

D-3 外部依赖：
  D-1 (触发器工具) ✅ ──→ D-3h (章节切换触发器)
  C-5 (货币系统) ──→ D-3g (击杀→货币转换)
  C-3/C-4/C-5 ──→ D-3e (全息界面子系统入口，仅占位)

跨 Phase：
  C-2 (原语层) ──→ C-3 (进化) ──→ D-3e (配装入库)
                ├─→ C-4 (配件) ──→ D-3e (配件配置入口)
                └─→ C-5 (技能树) ──→ D-3e (技能树入口)
  C-1 (匣纳) ──→ D-2 (对话需要能力枪管线稳定)
  D-1 (触发器工具) ✅ ──→ D-2 (对话需要触发器)
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
