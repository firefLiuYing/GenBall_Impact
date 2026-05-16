# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GenBall_Impact is a Unity 2022.3.42f1c1 game project featuring a custom game framework built on top of Unity. The project uses a component-based architecture with two main code namespaces:

- **Yueyn**: Core framework providing event systems, FSM, object pooling, reference pooling, timers, and resource management
- **GenBall**: Game-specific implementation including battle systems, enemies, UI, maps, player mechanics, and interaction systems

## Architecture

### Entry Point and Module System

The game uses a custom component/module system managed through `Entry.cs` (Yueyn framework):

- `GameEntry.cs` is the main MonoBehaviour that initializes the Entry system via `Awake()`, drives `Update()` and `FixedUpdate()`
- Modules implement `IComponent` interface (Priority, Init, ComponentUpdate, ComponentFixedUpdate, Shutdown) and are registered via `Entry.Register()`
- MonoBehaviour-based modules are discovered via `GetComponentsInChildren<IComponent>()` on GameEntry
- Non-MonoBehaviour modules (EntityCreators) are registered manually
- Access modules via `GameEntry.GetModule<T>()`

Registered modules (from `GameEntry.Register.cs`):

| Module | Type | Description |
|--------|------|-------------|
| `EventManager` | Yueyn | Global event system |
| `UIManager` | GenBall | Stack-based UI form management |
| `FsmManager` | Yueyn | Finite state machine management |
| `ObjectPoolManager` | Yueyn | Object pooling for GameObjects |
| `ResourceManager` | Yueyn | Asset loading |
| `TimerManager` | Yueyn | Timer system |
| `SaveComponent` | GenBall | Save/load system |
| `PlayerManager` | GenBall | Player creation and management |
| `MapModule` | GenBall | Map block loading and transitions |
| `SceneModule` | GenBall | Scene loading (async) |
| `ExecuteComponent` | GenBall | Game startup FSM (priority 10000) |
| `BuffSystem` | GenBall | Global Buff lifecycle management |
| `BulletSystem` | GenBall | Centralized bullet firing pipeline |
| `TimelineSystem` | GenBall | Timeline/cutscene playback |
| `EvolutionSystem` | GenBall | Weapon evolution via kill points |

Convenience accessors on `GameEntry`:
```
GameEntry.Event, GameEntry.UI, GameEntry.Save, GameEntry.Player,
GameEntry.Map, GameEntry.Execute, GameEntry.Scene, GameEntry.Fsm,
GameEntry.Buff, GameEntry.CharacterCreator, GameEntry.Timeline,
GameEntry.Bullet, GameEntry.Evolution
```

### IEntity and EntityCreator System

The project uses a generic entity factory/pool system as the foundation for all game entities:

- `IEntity` interface defines lifecycle: `OnSpawn()`, `EntityUpdate(float)`, `EntityFixedUpdate(float)`, `OnRecycle()`
- `EntityCreator<TEntityInterface>` (IComponent) manages creation, pooling, and per-frame updates for all entities of a given interface type
- Entity prefabs are registered via `AddPrefab<TEntity>(path)` and created via `CreateEntity<TEntity>()`

Registered EntityCreators:
- `EntityCreator<IBullet>` - Old bullet system (legacy)
- `EntityCreator<IWeapon>` - Old weapon system (legacy)
- `EntityCreator<IEnemy>` - Enemy entities
- `EntityCreator<IUserInterface>` - UI forms
- `EntityCreator<Player>` - Old player (legacy)
- `EntityCreator<IMapBlock>` - Map blocks
- `EntityCreator<CharacterState>` - New character entities (player, enemies)
- `EntityCreator<BulletState>` - New bullet entities
- `EntityCreator<WeaponState>` - New weapon entities

### Singleton System

Cross-cutting singleton services use `SingletonManager` pattern (`GenBall.Utils.Singleton`):

| Singleton | Description |
|-----------|-------------|
| `DamageSystem` | Centralized damage processing pipeline |
| `DeathSystem` | Centralized death processing with cancellation |
| `TeleportSystem` | Cross-scene teleportation |
| `SceneSystem` | Scene-level state tracking (save points, enemy units) |
| `InteractSystem` | Sight-based interaction management |
| `PauseManager` | Multi-flag pause state management |
| `GameManager` | Game data and save management |

### Yueyn Framework

Located in `Assets/Scripts/Yueyn/`:

- **EventPool**: Generic event system with subscribe/fire/fire-now pattern, supports `AllowNoHandler` and `AllowMultiHandler` modes
- **ReferencePool**: Object recycling for data classes implementing `IReference`. Used extensively for `DamageInfo`, `DeathInfo`, `BulletLaunchInfo`, `AddBuffInfo`, `BuffObj`, `StatValue`, `VmBase`, etc.
- **ObjectPool**: GameObject pooling with spawn/despawn lifecycle, used by `EntityCreator`
- **Variable`<T>`**: Observable value wrapper with `SetValue()`, `PostValue()`, `Observe()` pattern
- **FSM**: Generic finite state machine (`Fsm<T>`, `FsmState<T>`, `FsmManager`) with data sharing via `Variable<T>`
- **Timer**: Timer system with event callbacks
- **Resource**: `ResourceManager` for asset loading

### Battle System

Located in `Assets/Scripts/GenBall/BattleSystem/`:

#### Damage System

`DamageSystem` (singleton) provides centralized damage processing with Buff trigger pipeline:

1. Attacker's `ITriggerBeforeCauseDamage` buffs fire
2. Defender's `ITriggerBeforeTakeDamage` buffs fire
3. `IDamageable.TakeDamage()` executes actual damage
4. Attacker's `ITriggerAfterCauseDamage` buffs fire
5. Defender's `ITriggerAfterTakeDamage` buffs fire
6. `DamageInfo` released back to `ReferencePool`

Key types:
- `DamageInfo`: Carries Defender, Attacker, Damage, ImpactForce, Direction, Tags, AddBuffs
- `DamageValue`: Multi-zone damage calculation - base damage * zone multipliers + add damage
- `IHealth`: Base interface (Health, MaxHealth, IsDead, Die)
- `IDamageable`: Extends IHealth with `TakeDamage(DamageInfo)`
- `IHealable`: Extends IHealth with `Heal(int)`
- `IArmor`: Armor/MaxArmor management

#### Death System

`DeathSystem` (singleton) handles death processing with cancellation support:

1. Victim's `ITriggerBeforeDie` buffs fire (can set `DeathInfo.Cancelled = true`)
2. If cancelled, abort death
3. Victim's `ITriggerAfterDie` buffs fire
4. Killer's `ITriggerAfterKill` buffs fire
5. `IHealth.Die()` executes
6. `DeathInfo` released

#### Buff System

核心架构，所有战斗相关的效果都通过Buff系统实现：

- `IBuff`: Buff基础接口，支持Priority、CanMultiExist、Tags
- `BuffObj`: Buff实例基类（IReference），管理Caster、Carrier、Stacks、TickTimer，支持OnAdd/OnStack/OnUnstack/OnRemove/OnUpdate生命周期
- `IBuffContainer`: Buff容器接口，由`CharacterState`、`BulletState`、`WeaponState`实现
- `BuffSystem` (IComponent): 全局Buff管理器，处理添加（含叠层逻辑）/移除/每帧Tick
- `BuffModel`: ScriptableObject配置，`BuffModelConfig`提供按BuffId查询
- `AddBuffInfo`: 创建Buff的请求数据（BuffId → BuffModel自动查找）

Buff回调接口（trigger points）：
- 子弹: `ITriggerBeforeFireBullet`, `ITriggerAfterFireBullet`, `ITriggerBeforeBulletBeFired`, `ITriggerAfterBulletBeFired`
- 伤害: `ITriggerBeforeCauseDamage`, `ITriggerAfterCauseDamage`, `ITriggerBeforeTakeDamage`, `ITriggerAfterTakeDamage`
- 生命周期: `ITriggerBeforeDie`, `ITriggerAfterDie`, `ITriggerAfterKill`
- Buff管理: `ITriggerBeforeAddBuff`, `ITriggerAfterAddBuff`, `ITriggerBeforeStackBuff`, `ITriggerAfterStackBuff`

#### Character System

- `CharacterState` (MonoBehaviour): 核心角色组件，实现`IDamageable`、`IBuffContainer`、`IEntity`
  - 自动收集子对象的`ICharacterInitializer`和`ICharacterController`（按Priority排序）
  - Command模式处理移动/旋转：`MoveCommand`、`RotateCommand`通过`IMove`/`IRotate`接口
  - 能力标志位：CanMove, CanRotate, CanJump, CanAttack
  - 血量变化自动触发DeathSystem
  - 支持PauseManager暂停
- `CharacterStats`: 从`CharacterStatsModel`创建，当前包含MaxHealth (IntStat)
- `ICharacterInitializer`: 初始化器接口（Priority排序），在OnSpawn时调用
- `ICharacterController`: 控制器接口（Priority排序），每帧Tick
- `CharacterInitializerBase` / `CharacterControllerBase`: 提供MonoBehaviour基类

#### Bullet System

新弹药架构：
- `BulletSystem` (IComponent): 统一发射入口`FireBullet(BulletLaunchInfo)`，处理发射前后的Buff触发
- `BulletState` (IBuffContainer, IEntity): 子弹状态组件，持有BulletModel、Source、生成点等
- `BulletLaunchInfo` (IReference): 发射参数（Source、Model、LogicSpawnPoint、RendererSpawnPoint、SpawnDirection）
- `IBulletController`: 子弹行为接口（Init、Fire、Tick），如`RayBulletController`
- `BulletModel`: 子弹配置数据

#### Weapon System

新旧两套武器架构并存：

**新架构 (WeaponState)**:
- `WeaponState` (IBuffContainer, IEntity): 武器状态组件，管理WeaponStats、Accessory、Trigger/Reload
- `IWeaponTriggerController`: 射击控制器接口（如`NormalTriggerController`）
- `IWeaponReloadController`: 换弹控制器接口（如`NormalReloadController`）
- `WeaponStats`: 包含Damage (DamageValue)、FireInterval (FloatStat)、ReloadTime (FloatStat)
- `WeaponModel`: 序列化配置数据（damage, fireInterval, reloadTime）

**旧架构 (WeaponBase)** - 仍在使用中:
- `WeaponBase`: 抽象MonoBehaviour，IWeapon + IEffectable，管理IEffect列表和IWeaponComponent
- `IWeapon`: 定义Equip/Unequip/Trigger/Attack + Stats接口
- `IWeaponComponent`: 武器子组件接口（如`FireComponent`、`MagazineComponent`）

#### Accessory System

配件通过Buff系统修改武器属性：
- `AccessoryObj` (IReference): 配件实例，OnAdd时向WeaponState添加Buff，OnRemove时移除
- `AccessoryModel`: 配件配置，定义要添加的Buff列表
- `AccessoryModelConfig`: 配件配置管理器

#### Evolution System

`EvolutionSystem` (IComponent) 管理武器进化：
- 击杀获得KillPoints，达到阈值可进化
- `EvolutionConfig` ScriptableObject配置各阶段所需击杀数
- `EquipInfo` 映射进化等级到武器ID + 配件列表
- 最大进化等级可配置（默认4级）

#### Stats System

通用数值计算系统：
- `StatValue<T>` (IReference): 泛型属性值，支持BaseValue + Modifier列表
- `IntStat` / `FloatStat`: 具体实现，通过ReferencePool创建
- `StatModifier<T>`: 修饰器基类
  - `AddModifier<T>`: 加法修饰
  - `FloatMultiplyModifier`: 浮点乘法修饰
  - `IntMultiplyModifier`: 整数乘法修饰（内部用float）
- `DamageValue` (IReference): 多区域伤害计算 = Σ(区域倍率) × 基础伤害 + 附加伤害

#### Timeline System

`TimelineSystem` (IComponent, partial): 管理演出/技能时间线
- `TimelineObj`: 时间线实例，支持TimeScale
- `TimelineModelConfig`: 配置数据
- 在FixedUpdate中Tick，受PauseManager暂停控制
- 示例实现：`DashTimeline`

**注意**: `IEffect`、旧版`WeaponBase`部分功能、Enemy的Module系统为重构前代码，正在迁移到新Buff系统，暂未删除

### Interact System

Located in `Assets/Scripts/GenBall/Interact/`:

- `InteractSystem` (singleton): 管理可交互物体列表和当前选择
  - `Variable<List<IInteractable>>` 可观察的交互物列表
  - `Variable<int>` 当前选中索引，支持NextSelection/LastSelection循环切换
  - `TriggerInteractable()` 触发当前选中的交互
- `IInteractable`: 可交互物接口，定义`OperationDescription`（UI显示文本）和`Interact()`
- `InteractController` (CharacterControllerBase): 玩家视线检测
  - SphereCast检测前方可交互物体
  - 每帧对比上一帧和当前帧，自动Add/Remove交互物
  - 滚轮切换选择
- 已实现：`JumpHelp`、`OrbisAppear`

### Player System

Located in `Assets/Scripts/GenBall/Player/`:

- `PlayerManager` (IComponent): 管理玩家创建，通过`EntityCreator<CharacterState>`创建Player实体
  - 支持按Transform或Position/Rotation创建
  - 预制体路径注册在Init中

- Player使用CharacterState作为基础，通过Controller和Initializer模式组织功能：

Controllers (`ICharacterController`):
- `PlayerMover`: 移动控制
- `JumpController`: 跳跃控制
- `PhysicsController`: 物理更新
- `WeaponController`: 武器控制
- `InteractController`: 视线交互检测
- `PlayerStateMachine`: 玩家状态机控制

Initializers (`ICharacterInitializer`):
- `PlayerArmorInitializer`: 护甲初始化
- `PlayerUiInitializer`: UI初始化
- `PlayerCameraInitializer`: 相机初始化

Player States FSM: `PlayerInitState` → `PlayerMoveState` ↔ `PlayerJumpState` / `PlayerDashState`

Input: `InputHandler` + `InputController` 处理输入事件

旧版`Player`类（partial: Player.cs/Control/Countdown/Fsm/Health/Physics/Weapon）仍存在但正在迁移

### Enemy System

Located in `Assets/Scripts/GenBall/Enemy/`:

- 敌人基于新的Character系统构建，使用`CharacterState`作为基础
- `IEnemy` + `EnemyBase`: 敌人接口和基类
- `EnemyId`: 敌人类型枚举
- 旧的模块化架构仍存在（`AttackModule`, `DetectModule`, `MoveModule`, `HurtModule`, `FsmModule`）
- 近战敌人FSM: `InitState` → `WanderState` ↔ `ChaseState` → `AttackState` / `BackState` → `DeathState`
- 特殊敌人: `NormalOrbis`, `Barrier`
- 攻击模块: `AttackCollider`, `DashAttackModule`

### UI System

Located in `Assets/Scripts/GenBall/UI/`:

- Stack-based UI management via `UIManager`
- `FormBase` 抽象基类提供完整生命周期和ViewModel支持：
  - 自动收集子对象的`ItemBase`组件
  - 支持`VmBase` ViewModel模式（通过ReferencePool管理生命周期）
  - 生命周期: Init → Open → Focus/Unfocus → Pause/Resume → Close
- `IUserInterface`: UI接口，扩展IEntity，定义Init/Open/Close/Focus/Unfocus/Pause/Resume + Canvas
- `VmBase` (IReference): ViewModel基类，用于UI数据绑定
- `ItemBase`: 可复用UI子组件基类，跟随Form生命周期
- UI binding system via `UiBindTool` 生成 `.Bind.cs` partial classes
- Canvas sorting order根据栈深度自动管理

现有UI Forms:
- `SplashForm` + `SplashFormVm`: 过场画面
- `StartForm` + `StartVm`: 开始界面
- `AccessoryForm`: 配件界面
- `MainHud` + `MainHudVm`: 主HUD
- `UpgradeTip`: 升级提示
- `HpBar`: 血条
- `HeartItem`: 心形血量显示
- `InteractTipItem` + `InteractTipVm`: 交互提示

工具类: `CellViewSpawner` (动态列表), `UIExpense` (UI扩展方法)

### Map System

Located in `Assets/Scripts/GenBall/Map/`:

- `MapModule` (IComponent): 地图块加载管理
  - 基于玩家位置加载周围地图块（可配置加载层数）
  - `MapBlockBase` / `IMapBlock`: 地图块基类和接口
  - `MapBlockAuthoring`: 地图块编辑工具
- `SceneModule` (IComponent): 异步场景加载
- `SceneSystem` (singleton): 场景状态追踪
  - 记录每个场景已解锁的存档点和已击杀的敌人单元
  - 从`MapModel`初始化场景配置
  - 从`MapSaveData`初始化场景状态
- `TeleportSystem` (singleton): 跨场景传送
  - `TeleportRequestInfo`(SceneName + SavePointIndex)触发传送
  - 通过SceneSystem查找SavePointModel，然后加载目标场景
- `SavePoint` / `SavePointModel`: 存档/重生点
- `MapModel` / `MapConfig`: 结构化地图数据配置
- `ConfigProvider`: ScriptableObject配置加载工具

### Procedure System

Located in `Assets/Scripts/GenBall/Procedure/`:

- `ExecuteComponent` (IComponent, priority 10000): 游戏启动流程FSM
  - States: `ProcedureLoadState` → `StartFormState` → `LoadSceneState`
  - 支持StartNewGame / ContinueLastGame / LoadGame
  - `RunningMode` flags: SaveData, LoadData（编辑器可配置跳过存档）
- `GameManager` (singleton): 游戏数据管理
  - 持有当前`GameData`和存档索引
  - `SaveGame()` 异步保存
- `PauseManager` (singleton): 多标志位暂停管理
  - `PauseState` flags: Unpaused, LogicPaused, PhysicsPaused, GamePaused, AnimationPaused, AudioPaused
  - 各系统检查对应标志位决定是否暂停（如BuffSystem、TimelineSystem检查LogicPaused）
- `SaveComponent` (IComponent): 存档读写
- `GameData`: 游戏运行时数据

### Event System

Two event systems are used:

1. **Global Events** (`Yueyn.Event.EventManager`): Cross-module communication
   - 代码生成: `GlobalEventSystem.Generated.cs` 提供类型安全的Fire/Subscribe扩展方法
   - 模板配置: `ValueEventTemplateConfig`
2. **Local Events** (`ILocalEventManager`): Entity-specific events (weapons, enemies)
   - 代码生成: `EffectEvents.Generated.cs`

Events use `GameEventArgs` base class and are managed through `EventPool`.

### Code Generation

The project uses custom code generators:

- **UI Binding**: `UiBindTool` generates `.Bind.cs` files for UI element references
- **Global Event Code Generation**: `GlobalEventCodeGenerator` generates in `Assets/Scripts/GenBall/Event/Generated/`
- **Effect Event Code Generation**: `EventCodeGenerator` generates in `Assets/Scripts/GenBall/BattleSystem/Generated/`

## Development Commands

### Opening the Project

Open `GenBall_Impact.sln` in your IDE or open the project folder in Unity 2022.3.42f1c1.

### Building

Build through Unity Editor: File → Build Settings → Build

### Running

Press Play in Unity Editor or run the built executable.

### Scenes

Main scenes located in `Assets/Scenes/`:
- `Launcher.unity`: Entry scene
- `Prologue.unity`: Prologue chapter
- `Episode1.unity`: First episode
- `YueynScene.unity`, `SSTest2.unity`, `YueynTestMap.unity`, `SunnyStrikeScene.unity`: Test/development scenes

## Code Patterns

### Creating a New Buff

1. 在`BuffId`枚举中添加新的Buff ID
2. 创建Buff类继承自`BuffObj`
3. 实现需要的回调接口（如`ITriggerBeforeTakeDamage`）
4. 在`BuffIdToExtension.ToType()`中注册类型映射
5. 在`BuffModelConfig`中配置Buff数据

示例：
```csharp
public class MyBuff : BuffObj, ITriggerAfterCauseDamage
{
    public override void OnAdd(AddBuffInfo addBuffInfo)
    {
        // Buff添加时的初始化逻辑
    }

    public void TriggerAfterCauseDamage(DamageInfo damageInfo)
    {
        // 造成伤害后触发的逻辑
    }

    public override void Clear()
    {
        base.Clear();
        // 清理资源
    }
}
```

### Adding a Buff to Entity

```csharp
var info = AddBuffInfo.Create(BuffId.MyBuff, targetGameObject, addStacks: 1, caster: casterGameObject);
GameEntry.Buff.AddBuff(info);
```

### Applying Damage

```csharp
var damageInfo = DamageInfo.Create(
    defender: targetGameObject,
    damage: 10,
    tags: new List<string> { "bullet" },
    direction: hitDirection,
    impactForce: 5,
    attacker: sourceGameObject
);
DamageSystem.Instance.ApplyDamage(damageInfo);
// DamageInfo is auto-released to ReferencePool after processing
```

### Firing a Bullet

```csharp
var launchInfo = BulletLaunchInfo.Create(
    model: bulletModel,
    logicSpawnPoint: muzzlePosition,
    rendererSpawnPoint: muzzleVisualPosition,
    spawnDirection: aimDirection,
    source: playerGameObject
);
GameEntry.Bullet.FireBullet(launchInfo);
// BulletLaunchInfo is auto-released after processing
```

### Creating a New Character

1. 创建GameObject并添加`CharacterState`组件
2. 配置`CharacterStatsModel`字段（baseHealth等）
3. 添加所需的`ICharacterInitializer`和`ICharacterController`组件
4. 实现`IMove`和`IRotate`接口（如需要）
5. 通过EntityCreator注册和创建

### Creating a New Weapon (New Architecture)

1. 创建WeaponState预制体
2. 添加`IWeaponTriggerController`实现（如继承已有的NormalTriggerController）
3. 添加`IWeaponReloadController`实现（如NormalReloadController）
4. 配置`WeaponModel`序列化字段（damage, fireInterval, reloadTime）
5. 注册预制体到EntityCreator

### Creating a New Enemy

1. 创建敌人prefab，添加`CharacterState`组件
2. 配置`CharacterStatsModel`定义属性
3. 添加所需的Controller和Initializer组件
4. 在场景中放置或通过SceneSystem配置EnemyUnitModel

### Creating a New UI Form

1. Create prefab with Canvas component
2. Add `UiBindTool` component and configure bindings
3. Create form class inheriting from `FormBase`
4. Generate `.Bind.cs` using UiBindTool inspector
5. Override OnInit/OnOpen/OnClose/OnFocus/OnUnfocus as needed
6. (Optional) Create VmBase子类管理UI数据
7. Open via `GameEntry.UI.OpenForm<TForm>()`

### Using the EntityCreator

```csharp
// Register prefab (usually in Init)
GameEntry.CharacterCreator.AddPrefab<CharacterState>("EnemyName", "Assets/path/to/prefab.prefab");

// Create entity
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("EnemyName", position, rotation, parent);

// Recycle entity
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

### Subscribing to Events

```csharp
// Global events (type-safe generated methods)
GameEntry.Event.SubscribePlayerKillPoints(OnKillPointsChanged);
GameEntry.Event.FireWeaponLevel(newLevel);

// Global events (manual)
GameEntry.Event.Subscribe(eventId, OnEventHandler);

// Local events (weapons)
weapon.Subscribe(WeaponEventId, OnWeaponEvent);
```

### Using the Interact System

```csharp
// Implement IInteractable on a MonoBehaviour
public class MyInteractable : MonoBehaviour, IInteractable
{
    public string OperationDescription => "打开";
    public void Interact()
    {
        // 交互逻辑
    }
}
// InteractController will auto-detect via SphereCast on interactableLayer
```

## File Naming Conventions

- UI forms use `FormName.cs` and `FormName.Bind.cs` pattern
- UI ViewModels use `FormNameVm.cs` pattern
- Partial classes split functionality (e.g., `GameEntry.Bullet.cs`, `GameEntry.Weapon.cs`, `Player.Health.cs`)
- ScriptableObject configs end with `Config` suffix
- Generated code in `Generated/` subdirectories

## Notes

- The project uses Chinese comments in some areas
- Git branch: `master`
- Uses Unity's new Input System package
- Custom inspector attributes available: `[InspectorButton]`, `[LiveData]`
- 新旧武器系统并存：`WeaponBase` (IEffect based) 为旧架构，`WeaponState` (IBuff based) 为新架构，正在迁移中
- 旧版Player类和Enemy Module系统正在向新的CharacterState架构迁移
