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

### 指令分发（Command Dispatcher）❌ 未实现
- 旧 CharacterState 中通过 HandleCommand(ICommand) switch 语句分发到 IMove/IRotate/IAttack/IFaceDirection
- BattleEntity 中尚未形式化
- 需要的指令类型：MoveCommand、RotateCommand、AttackCommand、FaceDirectionCommand

### 决策层（Decision Layer）❌ 未形式化
- Player 决策：InputController → FSM data → FSM states → 执行物理动作（目前还在旧 Player 分部类中）
- Enemy 决策：EnemyAIController → FSM states → IssueCommand → CharacterState.HandleCommand
- BattleEntity 中尚未抽象为组件

### 事件分发器（Event Dispatcher）❌ 待讨论
- 复杂度高，已约定后续专门讨论
- 涉及 BattleEntity 内部组件间的低耦合通信

## 实体装配示例

```csharp
// Player 实体：全功能
var entity = go.AddComponent<BattleEntity>();
entity.RegisterComponent(new StatComponent());        // 属性
entity.RegisterComponent(new BuffContainerComponent()); // Buff
entity.RegisterComponent(new DamageReceiverComponent(entity)); // 承伤
entity.RegisterComponent(new AttackComponent(entity, new WeaponDamageCalculator())); // 攻击
// TODO: entity.RegisterComponent(new CommandDispatcher());     // 指令分发
// TODO: entity.RegisterComponent(new PlayerDecisionLayer());   // 玩家决策

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
| HandleCommand switch | CommandDispatcher（待建） | ❌ |
| Player FSM + Controllers | PlayerDecisionLayer（待建） | ❌ |
| EnemyAI + FSM | EnemyDecisionLayer（待建） | ❌ |
| 组件间通信 | EventDispatcher（待讨论） | ❌ |

## 核心设计原则

1. **组件纯C#、不继承MonoBehaviour** — 只有 BattleEntity 壳是 MB
2. **可选组合** — 不同实体类型按需装配不同组件
3. **组件间通过 BattleEntity.Get<T>() 互相访问** — 无全局状态，天然可测试
4. **ISystem 做全局服务** — 伤害/死亡/Buff/EntityUpdate 都是 ISystem，BattleEntity 组件通过 SystemRepository 获取
