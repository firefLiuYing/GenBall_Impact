# 战斗系统

## 核心设计

**BattleEntity 薄壳 + 可选组件 + ISystem 全局服务**。

BattleEntity 只是一个 95 行的 MonoBehaviour 容器（`Dictionary<Type, object>`），不包含任何业务逻辑。所有逻辑在可插拔的纯 C# 组件中。

```
BattleEntity (MonoBehaviour, 薄壳)
├── RegisterComponent<T>() — 自动检测 IEntityFrameUpdate/IEntityLogicUpdate 并注册到 EntityUpdateSystem
├── RegisterComponentAs<TInterface>() — 额外的接口查询键
├── Get<T>() / TryGet<T>() / Has<T>()
└── OnDestroy() — 自动注销所有更新接口
```

## 组件目录

| 组件 | 层 | 必须？ | 作用 |
|------|----|--------|------|
| StatComponent | 属性 | 按需 | `string→Stat` 键值对，公式 `(Base+Flat)×(1+Percent)×Multiply` |
| EventDispatcherComponent | 属性 | 按需 | 组件间同步事件（封装 Yueyn.Event.EventDispatcher） |
| DamageReceiverComponent | 战斗 | 承伤实体 | 护盾先吸收→扣血。零血不锁死，由 DeathComponent 决定死亡 |
| AttackComponent | 战斗 | 攻击实体 | 策略模式 `IDamageCalculator`（Simple/Weapon/Bullet 三种），产出 DamageContext |
| BuffContainerComponent | 战斗 | 按需 | SortedSet 按优先级排序，复用 BuffRegistry + BuffTickSystem |
| CommandDispatcherComponent | 控制 | 有决策的实体 | 连续指令直通 + 动作指令仲裁（优先级对比），0.2s 输入缓冲 FIFO 队列 |
| HitReactionComponent | 控制 | 可选 | 受击后发 StunCommand(priority=10)，清空命令缓冲（武器就没有） |
| DeathComponent | 生命周期 | 可死亡的实体 | 订阅 HealthChanged，零血触发 `IDeathHandler.OnDeath()` + `IDeathSystem.ApplyDeath()` |

DeathComponent 与 DamageReceiverComponent 分离：不同实体（Player、各类 Orbis）死亡行为不同，通过 `IDeathHandler` 可插拔。

## 组件间通信

1. **`entity.Get<T>()` 查兄弟组件** — 主要方式。DamageReceiver 查 StatComponent 读血量，AttackComponent 查 StatComponent 取攻击力
2. **EventDispatcherComponent 发布/订阅** — 解耦通信。StatComponent 抛 StatChangedEvent，DamageReceiver 抛 HealthChangedEvent
3. **全局 CEventRouter** — 系统级事件。DamageSystemDefault/DeathSystemDefault 通过此发布，BuffTickSystem 订阅后触发 Buff 回调
4. **SystemRepository.Instance.GetSystem<T>()** — 所有组件均可直接调用，获取 ISystem 全局服务

## 决策层

```
IDecisionLayer { Dispatcher, MakeDecision(deltaTime) }
├── PlayerDecisionLayer — IPlayerInputEvents 驱动（按键=事件, 连续输入=轮询）→ 产出命令
└── EnemyDecisionLayer — Yueyn.Fsm.Fsm<T> + EnemyAIConfigSo 数据驱动 AI 状态机
```

AI 状态：Idle → Chase → Attack → Wander，全部在 `Framework/AI/` 下。

## 命令系统

连续型指令（Move/Rotate）直通。动作型指令通过 `IArbitratedCommand` 优先级仲裁：

| 命令 | 优先级 | 反优先级 | 可缓冲 | 阻止移动 | 阻止旋转 | 阻止重力 |
|------|--------|----------|--------|----------|----------|----------|
| Attack | 2 | 2 | 是 | 是 | 否 | 否 |
| Jump | 3 | 3 | 是(Start) | 是 | 否 | 否 |
| Dash | 5 | 5 | 否 | 是 | 是 | 是 |
| Stun | 10 | 10 | 否 | 是 | 否 | 否 |
| AbilitySecondary | 2 | 2 | 是 | 否 | 否 | 否 |

仲裁规则：`IssuedCommand.InterruptPriority >= CurrentCommand.AntiInterruptPriority` 则打断。

## 执行器层

执行器实现特定接口（IMove/IRotate/IJump/IDash/IAttack/IReload/ISwitchWeapon/IStun/IInteract/IWheel 等），由 CommandDispatcher 按命令类型路由。

| 执行器 | 接口 | 位置 |
|--------|------|------|
| PlayerMoveExecutor | IMove | Player/Executor/ |
| PlayerJumpExecutor | IJump, IEntityLogicUpdate | 同上 |
| PlayerDashExecutor | IDash, IEntityLogicUpdate | 同上 |
| PlayerGravityExecutor | IEntityLogicUpdate | 同上 |
| PlayerRotateExecutor | IRotate | 同上 |
| PlayerInteractExecutor | IInteract | 同上 |
| WeaponAttackExecutor | IAttack, IReload, ISwitchWeapon | 同上 |
| WheelExecutor | IWheel | 同上 |
| WeaponVisibilityExecutor | IWeaponVisibility | 同上 |

## 实体装配

`PlayerEntityFactory.AssemblePlayer()` 固定装配顺序：

1. 创建 BattleEntity → 2. 收集 MB 引用（RigidbodyMover、InputHandler、GroundDetect）
2. StatComponent（初始值：MaxHealth=100, Attack=10, MoveSpeed=5 等）
3. EventDispatcherComponent → 战斗组件（DamageReceiver、BuffContainer、Attack、CommandDispatcher）
4. 执行器注册到 CommandDispatcher
5. PlayerInputAdapter + PlayerDecisionLayer
6. DeathComponent + PlayerDeathHandler → HitReactionComponent
7. 全组件 RegisterComponent + RegisterComponentAs
8. 创建默认手枪并装备 → 绑定 ICombatStateSystem 和 IAbilityWeaponSystem

## 全局 ISystem 依赖

组件通过 `SystemRepository.Instance.GetSystem<T>()` 获取：

| 系统 | 接口 | 用途 |
|------|------|------|
| EntityUpdateSystem | IEntityUpdateSystem | BattleEntity 注册/注销帧更新 |
| DamageSystemDefault | IDamageSystem | 应用伤害，通过 CEventRouter 触发 Buff 事件 |
| DeathSystemDefault | IDeathSystem | 处理死亡 |
| BuffRegistry | IBuffRegistry | Buff 添加/移除 |
| BuffTickSystem | IBuffTickSystem | 每帧 Tick 活跃 Buff |

## 子模块

- **武器系统** — `Weapons/`，武器自身是独立的 BattleEntity，有弹药系统（Magazine/Heat/Infinite）和扳机行为（SemiAuto/FullAuto/Shotgun/Charge），通过 WeaponAttackExecutor 与 Player 桥接。见 `Weapons/CLAUDE.md`（待创建）
- **能力武器** — 技能体系，非武器。激活时替换普通武器执行器，有自己的状态机（Idle→Hiding→Active→Showing）。待迁入 BattleSystem 下并创建独立 CLAUDE.md
