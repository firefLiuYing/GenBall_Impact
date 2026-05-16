# Architecture Guide

This document provides a deep dive into the core architectural patterns of the GenBall_Impact project.

## Entry Point and Module System

The game uses a custom component/module system managed through `Entry.cs` (Yueyn framework):

### Core Components

- **`GameEntry.cs`**: Main MonoBehaviour that initializes the Entry system via `Awake()`, drives `Update()` and `FixedUpdate()`
- **`IComponent` interface**: Defines module lifecycle
  - `Priority`: Determines initialization and update order
  - `Init()`: Called during system initialization
  - `ComponentUpdate(float deltaTime)`: Called every frame
  - `ComponentFixedUpdate(float fixedDeltaTime)`: Called every physics frame
  - `Shutdown()`: Called during cleanup
- **Module Registration**: Modules are registered via `Entry.Register()`
  - MonoBehaviour-based modules: Discovered via `GetComponentsInChildren<IComponent>()` on GameEntry
  - Non-MonoBehaviour modules (EntityCreators): Registered manually
- **Module Access**: Access modules via `GameEntry.GetModule<T>()`

### Registered Modules

From `GameEntry.Register.cs`:

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

### Convenience Accessors

`GameEntry` provides direct accessors for frequently used modules:

```csharp
GameEntry.Event        // EventManager
GameEntry.UI           // UIManager
GameEntry.Save         // SaveComponent
GameEntry.Player       // PlayerManager
GameEntry.Map          // MapModule
GameEntry.Execute      // ExecuteComponent
GameEntry.Scene        // SceneModule
GameEntry.Fsm          // FsmManager
GameEntry.Buff         // BuffSystem
GameEntry.CharacterCreator  // EntityCreator<CharacterState>
GameEntry.Timeline     // TimelineSystem
GameEntry.Bullet       // BulletSystem
GameEntry.Evolution    // EvolutionSystem
```

## IEntity and EntityCreator System

The project uses a generic entity factory/pool system as the foundation for all game entities.

### IEntity Interface

Defines the lifecycle for all entities:

```csharp
public interface IEntity
{
    void OnSpawn();                      // Called when entity is created/spawned from pool
    void EntityUpdate(float deltaTime);  // Called every frame
    void EntityFixedUpdate(float fixedDeltaTime);  // Called every physics frame
    void OnRecycle();                    // Called when entity is returned to pool
}
```

### EntityCreator<TEntityInterface>

`EntityCreator<TEntityInterface>` is an `IComponent` that manages:
- **Creation**: Instantiates entities from registered prefabs
- **Pooling**: Recycles entities to reduce allocation overhead
- **Per-frame updates**: Automatically calls `EntityUpdate()` and `EntityFixedUpdate()` on all active entities

### Usage Pattern

```csharp
// Register prefab (usually in Init)
GameEntry.CharacterCreator.AddPrefab<CharacterState>("EnemyName", "Assets/path/to/prefab.prefab");

// Create entity
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("EnemyName", position, rotation, parent);

// Recycle entity (returns to pool)
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

### Registered EntityCreators

| EntityCreator | Purpose | Status |
|---------------|---------|--------|
| `EntityCreator<IBullet>` | Old bullet system | Legacy |
| `EntityCreator<IWeapon>` | Old weapon system | Legacy |
| `EntityCreator<Player>` | Old player | Legacy |
| `EntityCreator<IEnemy>` | Enemy entities | Active |
| `EntityCreator<IUserInterface>` | UI forms | Active |
| `EntityCreator<IMapBlock>` | Map blocks | Active |
| `EntityCreator<CharacterState>` | New character entities (player, enemies) | Active (New) |
| `EntityCreator<BulletState>` | New bullet entities | Active (New) |
| `EntityCreator<WeaponState>` | New weapon entities | Active (New) |

## Singleton System

Cross-cutting singleton services use `SingletonManager` pattern (`GenBall.Utils.Singleton`).

### Registered Singletons

| Singleton | Description | Key Responsibilities |
|-----------|-------------|---------------------|
| `DamageSystem` | Centralized damage processing pipeline | Applies damage with Buff trigger points |
| `DeathSystem` | Centralized death processing with cancellation | Handles death with Buff cancellation support |
| `TeleportSystem` | Cross-scene teleportation | Manages scene transitions and player positioning |
| `SceneSystem` | Scene-level state tracking | Tracks save points and enemy units per scene |
| `InteractSystem` | Sight-based interaction management | Manages list of interactable objects in player's view |
| `PauseManager` | Multi-flag pause state management | Controls logic/physics/animation/audio pause states |
| `GameManager` | Game data and save management | Holds current GameData and save index |

### Usage Pattern

```csharp
// Access singleton instance
DamageSystem.Instance.ApplyDamage(damageInfo);
DeathSystem.Instance.ProcessDeath(deathInfo);
InteractSystem.Instance.TriggerInteractable();
```

### Singleton vs Module

**Use Singleton when:**
- Service is cross-cutting and doesn't fit into the module hierarchy
- Service needs to be accessed from many different systems
- Service manages global state (e.g., PauseManager, GameManager)

**Use Module (IComponent) when:**
- System needs lifecycle management (Init, Update, Shutdown)
- System needs to participate in the update loop
- System is a core game system (UI, Events, Resources)

## Component Lifecycle

### Module Initialization Order

1. `GameEntry.Awake()` calls `Entry.Init()`
2. Modules are sorted by `Priority` (higher priority = earlier initialization)
3. Each module's `Init()` is called in priority order
4. `ExecuteComponent` (priority 10000) runs game startup FSM

### Update Loop

Every frame:
1. `GameEntry.Update()` → `Entry.Update()` → Each module's `ComponentUpdate(deltaTime)`
2. `GameEntry.FixedUpdate()` → `Entry.FixedUpdate()` → Each module's `ComponentFixedUpdate(fixedDeltaTime)`

### Entity Lifecycle

1. **Creation**: `EntityCreator.CreateEntity<T>()` instantiates or retrieves from pool
2. **Spawn**: `IEntity.OnSpawn()` called for initialization
3. **Update Loop**: `EntityUpdate()` and `EntityFixedUpdate()` called automatically by EntityCreator
4. **Recycle**: `EntityCreator.RecycleEntity()` → `IEntity.OnRecycle()` → returned to pool

## Key Architectural Principles

1. **Component-Based**: Systems are modular components registered with Entry
2. **Entity-Component**: Entities use composition (ICharacterInitializer, ICharacterController)
3. **Pooling**: Extensive use of object pooling (GameObject, Reference objects)
4. **Centralized Systems**: Damage, Death, Buff processing go through centralized systems
5. **Event-Driven**: Global and local event systems for decoupled communication
6. **FSM-Based**: State machines for player, enemies, and game flow
