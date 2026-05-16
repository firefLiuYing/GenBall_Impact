# Systems Overview

This document provides an overview of game-specific systems in GenBall_Impact.

## Interact System

Located in `Assets/Scripts/GenBall/Interact/`

### InteractSystem (Singleton)

管理可交互物体列表和当前选择

**Features**:
- `Variable<List<IInteractable>>` 可观察的交互物列表
- `Variable<int>` 当前选中索引，支持NextSelection/LastSelection循环切换
- `TriggerInteractable()` 触发当前选中的交互

### IInteractable Interface

可交互物接口，定义`OperationDescription`（UI显示文本）和`Interact()`

```csharp
public interface IInteractable
{
    string OperationDescription { get; }  // UI display text
    void Interact();                      // Interaction logic
}
```

### InteractController

**Type**: CharacterControllerBase

玩家视线检测

**Features**:
- SphereCast检测前方可交互物体
- 每帧对比上一帧和当前帧，自动Add/Remove交互物
- 滚轮切换选择

### Implemented Interactables

已实现：
- `JumpHelp` - Jump tutorial trigger
- `OrbisAppear` - Orbis spawn trigger

## Player System

Located in `Assets/Scripts/GenBall/Player/`

### PlayerManager (IComponent)

管理玩家创建，通过`EntityCreator<CharacterState>`创建Player实体

**Features**:
- 支持按Transform或Position/Rotation创建
- 预制体路径注册在Init中

### Player Architecture

Player使用CharacterState作为基础，通过Controller和Initializer模式组织功能

**Controllers** (`ICharacterController`):
- `PlayerMover`: 移动控制
- `JumpController`: 跳跃控制
- `PhysicsController`: 物理更新
- `WeaponController`: 武器控制
- `InteractController`: 视线交互检测
- `PlayerStateMachine`: 玩家状态机控制

**Initializers** (`ICharacterInitializer`):
- `PlayerArmorInitializer`: 护甲初始化
- `PlayerUiInitializer`: UI初始化
- `PlayerCameraInitializer`: 相机初始化

### Player State Machine

Player States FSM: `PlayerInitState` → `PlayerMoveState` ↔ `PlayerJumpState` / `PlayerDashState`

### Input Handling

Input: `InputHandler` + `InputController` 处理输入事件

### Legacy System

旧版`Player`类（partial: Player.cs/Control/Countdown/Fsm/Health/Physics/Weapon）仍存在但正在迁移

See [Migration Guide](migration-guide.md) for details.

## Enemy System

Located in `Assets/Scripts/GenBall/Enemy/`

### Architecture

敌人基于新的Character系统构建，使用`CharacterState`作为基础

**Core Types**:
- `IEnemy` + `EnemyBase`: 敌人接口和基类
- `EnemyId`: 敌人类型枚举

### Enemy State Machine

近战敌人FSM: `InitState` → `WanderState` ↔ `ChaseState` → `AttackState` / `BackState` → `DeathState`

### Special Enemies

特殊敌人:
- `NormalOrbis` - Standard Orbis enemy
- `Barrier` - Barrier enemy

### Attack Modules

攻击模块:
- `AttackCollider` - Collision-based attacks
- `DashAttackModule` - Dash attack behavior

### Legacy System

旧的模块化架构仍存在（`AttackModule`, `DetectModule`, `MoveModule`, `HurtModule`, `FsmModule`）

See [Migration Guide](migration-guide.md) for details.

## UI System

Located in `Assets/Scripts/GenBall/UI/`

### UIManager

Stack-based UI management via `UIManager`

**Features**:
- Stack-based form management
- Canvas sorting order根据栈深度自动管理
- Focus/Unfocus handling
- Pause/Resume support

### FormBase

抽象基类提供完整生命周期和ViewModel支持

**Features**:
- 自动收集子对象的`ItemBase`组件
- 支持`VmBase` ViewModel模式（通过ReferencePool管理生命周期）
- 生命周期: Init → Open → Focus/Unfocus → Pause/Resume → Close

### IUserInterface

UI接口，扩展IEntity，定义Init/Open/Close/Focus/Unfocus/Pause/Resume + Canvas

### VmBase

**Type**: IReference

ViewModel基类，用于UI数据绑定

### ItemBase

可复用UI子组件基类，跟随Form生命周期

### UI Binding System

UI binding system via `UiBindTool` 生成 `.Bind.cs` partial classes

### Existing UI Forms

现有UI Forms:
- `SplashForm` + `SplashFormVm`: 过场画面
- `StartForm` + `StartVm`: 开始界面
- `AccessoryForm`: 配件界面
- `MainHud` + `MainHudVm`: 主HUD
- `UpgradeTip`: 升级提示
- `HpBar`: 血条
- `HeartItem`: 心形血量显示
- `InteractTipItem` + `InteractTipVm`: 交互提示

### Utility Classes

工具类:
- `CellViewSpawner`: 动态列表
- `UIExpense`: UI扩展方法

## Map System

Located in `Assets/Scripts/GenBall/Map/`

### MapModule (IComponent)

地图块加载管理

**Features**:
- 基于玩家位置加载周围地图块（可配置加载层数）
- `MapBlockBase` / `IMapBlock`: 地图块基类和接口
- `MapBlockAuthoring`: 地图块编辑工具

### SceneModule (IComponent)

异步场景加载

### SceneSystem (Singleton)

场景状态追踪

**Features**:
- 记录每个场景已解锁的存档点和已击杀的敌人单元
- 从`MapModel`初始化场景配置
- 从`MapSaveData`初始化场景状态

### TeleportSystem (Singleton)

跨场景传送

**Process**:
1. `TeleportRequestInfo`(SceneName + SavePointIndex)触发传送
2. 通过SceneSystem查找SavePointModel
3. 加载目标场景

### Save Points

- `SavePoint`: Save point component
- `SavePointModel`: Save point configuration

### Map Configuration

- `MapModel` / `MapConfig`: 结构化地图数据配置
- `ConfigProvider`: ScriptableObject配置加载工具

## Procedure System

Located in `Assets/Scripts/GenBall/Procedure/`

### ExecuteComponent (IComponent)

**Priority**: 10000

游戏启动流程FSM

**States**: `ProcedureLoadState` → `StartFormState` → `LoadSceneState`

**Features**:
- 支持StartNewGame / ContinueLastGame / LoadGame
- `RunningMode` flags: SaveData, LoadData（编辑器可配置跳过存档）

### GameManager (Singleton)

游戏数据管理

**Responsibilities**:
- 持有当前`GameData`和存档索引
- `SaveGame()` 异步保存

### PauseManager (Singleton)

多标志位暂停管理

**PauseState Flags**:
- Unpaused
- LogicPaused
- PhysicsPaused
- GamePaused
- AnimationPaused
- AudioPaused

各系统检查对应标志位决定是否暂停（如BuffSystem、TimelineSystem检查LogicPaused）

### SaveComponent (IComponent)

存档读写

### GameData

游戏运行时数据

## Event System

Two event systems are used:

### 1. Global Events

**Type**: `Yueyn.Event.EventManager`

Cross-module communication

**Features**:
- 代码生成: `GlobalEventSystem.Generated.cs` 提供类型安全的Fire/Subscribe扩展方法
- 模板配置: `ValueEventTemplateConfig`

**Usage**:
```csharp
// Type-safe generated methods
GameEntry.Event.SubscribePlayerKillPoints(OnKillPointsChanged);
GameEntry.Event.FireWeaponLevel(newLevel);
```

### 2. Local Events

**Type**: `ILocalEventManager`

Entity-specific events (weapons, enemies)

**Features**:
- 代码生成: `EffectEvents.Generated.cs`

**Usage**:
```csharp
weapon.Subscribe(WeaponEventId, OnWeaponEvent);
```

### Event Base Class

Events use `GameEventArgs` base class and are managed through `EventPool`.

## Code Generation

The project uses custom code generators:

### UI Binding

**Tool**: `UiBindTool`

Generates `.Bind.cs` files for UI element references

**Location**: Same directory as form class

### Global Event Code Generation

**Tool**: `GlobalEventCodeGenerator`

**Output**: `Assets/Scripts/GenBall/Event/Generated/`

### Effect Event Code Generation

**Tool**: `EventCodeGenerator`

**Output**: `Assets/Scripts/GenBall/BattleSystem/Generated/`

## Related Documentation

- [Architecture Guide](architecture.md) - Module system and singletons
- [Battle Systems](battle-systems.md) - Combat-related systems
- [Code Patterns](code-patterns.md) - Implementation examples