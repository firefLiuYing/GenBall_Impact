# 战斗单元架构（BattleEntity Framework）

## 设计目标

替代旧的 CharacterState（臃肿 MonoBehaviour 单体），建立基于**可选组件**的轻量战斗单元。

## 架构概览

```
BattleEntity (MonoBehaviour, 160行)
├── Dictionary<Type, object> _components   ← 唯一状态
├── RegisterComponent<T>()                 ← 自动接 IEntityFrameUpdate/IEntityLogicUpdate
├── Get<T>() / TryGet<T>() / Has<T>()
└── OnDestroy()                            ← 自动从 EntityUpdateSystem 注销
```

BattleEntity 只是一个**薄壳**——MonoBehaviour 的唯一原因是它需要挂载到 GameObject 上以参与 Unity 生命周期。所有逻辑都在可插拔的纯 C# 组件中。

## 可选基建组件

### StatComponent（属性维护）✅ 已实现
- 键值对存储（string → Stat），无需改代码即可添加新属性
- `Stat = (Base + FlatAdd) × (1 + PercentAdd) × Multiply`
- 自由组合 StatModifier（FlatAdd / PercentAdd / Multiply）

### DamageReceiverComponent（伤害接收）✅ 已实现
- 实现 IHealth + IDamageable
- 动态从 StatComponent 读取 MaxHealth
- 血量 ≤ 0 时自动调用 IDeathSystem.ApplyDeath()
- 死亡后锁死（不再接受伤害）

### BuffContainerComponent（Buff容器）✅ 已实现
- 实现 IBuffContainer
- SortedSet<IBuff> 按优先级排序
- 完全复用现有 Buff 系统（BuffRegistry + BuffTickSystem）

### AttackComponent（攻击）✅ 已实现
- 实现 IAttacker
- 策略模式：IDamageCalculator 接口
- SimpleDamageCalculator → 只用 Attacker 的 Attack 属性
- WeaponDamageCalculator → Attack + WeaponStats["Damage"]
- BulletDamageCalculator → Attack + WeaponStats["Damage"] + BulletStats["Damage"]

### 指令分发（Command Dispatcher）✅ 已实现
- `CommandDispatcherComponent`：连续型指令直通（Move/Rotate 有动作时清零）、动作型指令仲裁（InterruptPriority >= AntiInterruptPriority 则打断）
- 0.2s 输入缓冲窗口（FIFO 环形队列）
- 暂停检查：`Issue()` 入口检查 `IPauseSystem.IsPaused`
- 帧序：IEntityLogicUpdate，须在 DecisionLayer 之前注册
- 新增指令类型：JumpCommand、DashCommand
- 新增执行器接口：IJump、IDash
- `IArbitratedCommand` 接口：InterruptPriority / AntiInterruptPriority / Bufferable

### 决策层（Decision Layer）✅ 已实现
- `IDecisionLayer` 接口：`Dispatcher { get; set; }` + `MakeDecision(float deltaTime)`
- `PlayerDecisionLayer`：纯 C# 类，`IEntityLogicUpdate`。通过 `IPlayerInputProvider` 读取输入 → 产出 Move/Rotate/Jump/Dash/Attack 命令。Dash 有 0.5s 冷却。地面检测通过 `ICharacterGroundDetect`（BattleEntity 组件优先，MonoBehaviour fallback）
- `EnemyDecisionLayer`：纯 C# 类，`IEntityLogicUpdate`。管理 AI FSM（Idle→Chase→Attack→Wander），AI 状态通过 `EnemyDecisionStateBase` 基类。命令通过 `CommandDispatcherComponent.Issue()` 路由
- `EnemyDecisionStateBase`（`FsmState<EnemyDecisionLayer>`）：提供 `Agent`、`Detect`、`AttackController`、`ChangeState<T>()`、`IssueCommand()` 便捷访问器
- AI 决策状态：`AIDecisionIdleState`、`AIDecisionChaseState`、`AIDecisionAttackState`、`AIDecisionWanderState`（位于 `Framework/AI/`）
- TODO Phase B：AI 状态改为从 aiConfig 数据驱动

### 事件分发器（Event Dispatcher）❌ 待讨论
- 复杂度高，已约定后续专门讨论
- 涉及 BattleEntity 内部组件间的低耦合通信

### 执行器组件（Player Executors）✅ 已实现（B-1 进行中）
- `PlayerJumpExecutor`（纯 C#，IJump + IEntityLogicUpdate）：变高跳跃物理，复用 JumpController.InitArgs() 公式。持有/释放判断通过 InputHandler。
- `PlayerDashExecutor`（纯 C#，IDash + IEntityLogicUpdate）：固定时长冲刺（无敌期+结束期），完成后解锁 PlayerMover。
- `PlayerAttackExecutor`（纯 C#，IAttack）：委托 WeaponController.Fire(ButtonState.Down)。
- `PlayerInputAdapter`（纯 C#，IPlayerInputProvider）：包装 InputHandler，提供统一输入接口给 PlayerDecisionLayer。
- `PlayerEntityFactory`（静态类）：AssemblePlayer() 负责装配 BattleEntity + 所有组件 + 执行器 + 决策层。
- `PlayerMover` 新增 LockVertical/LockHorizontal 属性，执行器激活时保留被锁轴的速度分量。

## 实体装配示例

```csharp
// Player 实体：全功能
var entity = go.AddComponent<BattleEntity>();
entity.RegisterComponent(new StatComponent());        // 属性
entity.RegisterComponent(new BuffContainerComponent()); // Buff
entity.RegisterComponent(new DamageReceiverComponent(entity)); // 承伤
entity.RegisterComponent(new AttackComponent(entity, new WeaponDamageCalculator())); // 攻击
var dispatcher = new CommandDispatcherComponent();    // 指令分发
entity.RegisterComponent(dispatcher);
entity.RegisterComponent(new PlayerDecisionLayer(entity, inputProvider)); // 玩家决策

// Trap 实体：仅有攻击
entity.RegisterComponent(new AttackComponent(entity, new SimpleDamageCalculator()));

// Barrel 实体：仅承伤
var stats = new StatComponent();
stats.SetBase("MaxHealth", 30f);
entity.RegisterComponent(stats);
entity.RegisterComponent(new DamageReceiverComponent(entity));
```

## 与旧架构的对应

| 旧 (CharacterState) | 新 (BattleEntity Framework) | 状态 |
|-----|------|------|
| CharacterState 本体 | BattleEntity 薄壳 | ✅ |
| CharacterStats | StatComponent | ✅ |
| IDamageable 内联实现 | DamageReceiverComponent | ✅ |
| IBuffContainer 内联实现 | BuffContainerComponent | ✅ |
| IAttacker (分散各处) | AttackComponent + IDamageCalculator | ✅ |
| HandleCommand switch | CommandDispatcher（仲裁+缓冲） | ✅ |
| Player FSM + Controllers | PlayerDecisionLayer | ✅ |
| EnemyAI + FSM | EnemyDecisionLayer | ✅ |
| 组件间通信 | EventDispatcher（待讨论） | ❌ |

## 核心设计原则

1. **组件纯C#、不继承MonoBehaviour** — 只有 BattleEntity 壳是 MB
2. **可选组合** — 不同实体类型按需装配不同组件
3. **组件间通过 BattleEntity.Get<T>() 互相访问** — 无全局状态，天然可测试
4. **ISystem 做全局服务** — 伤害/死亡/Buff/EntityUpdate 都是 ISystem，BattleEntity 组件通过 SystemRepository 获取
