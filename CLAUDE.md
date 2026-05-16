# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GenBall_Impact is a Unity 2022.3.42f1c1 game project featuring a custom game framework built on top of Unity. The project uses a component-based architecture with two main code namespaces:

- **Yueyn**: Core framework providing event systems, FSM, object pooling, reference pooling, timers, and resource management
- **GenBall**: Game-specific implementation including battle systems, enemies, UI, maps, player mechanics, and interaction systems

## Documentation

Detailed documentation is organized into specialized guides:

### Architecture & Core Systems
- **[Architecture Guide](.claude/docs/architecture.md)** - Entry system, modules, EntityCreator, singletons
- **[Framework Reference](.claude/docs/framework-reference.md)** - Yueyn framework utilities (EventPool, ObjectPool, FSM, Timer, Resource)
- **[Event System Guide](.claude/docs/event-system-guide.md)** - New event system usage (no EventArgs, direct parameters)

### Battle & Combat Systems
- **[Battle Systems](.claude/docs/battle-systems.md)** - Damage, Death, Character, Bullet, Weapon, Accessory, Evolution, Timeline
- **[Buff System Reference](.claude/docs/buff-system-reference.md)** - Comprehensive Buff system guide (核心架构, triggers, stats, modifiers)

### Game Systems
- **[Systems Overview](.claude/docs/systems-overview.md)** - Player, Enemy, UI, Map, Interact, Procedure, Events, Code Generation

### Development
- **[Code Patterns](.claude/docs/code-patterns.md)** - Implementation examples (creating Buffs, Weapons, Characters, UI, Events)
- **[Setup Guide](.claude/docs/setup-guide.md)** - Building, running, scenes
- **[Conventions](.claude/docs/conventions.md)** - Naming conventions, custom attributes, project notes
- **[Migration Guide](.claude/docs/migration-guide.md)** - Old vs new system architectures (WeaponBase→WeaponState, Player→CharacterState)

## Quick Reference

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

**Convenience Accessors**:
```csharp
GameEntry.Event, GameEntry.UI, GameEntry.Save, GameEntry.Player,
GameEntry.Map, GameEntry.Execute, GameEntry.Scene, GameEntry.Fsm,
GameEntry.Buff, GameEntry.CharacterCreator, GameEntry.Timeline,
GameEntry.Bullet, GameEntry.Evolution
```

### EntityCreator Types

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

### Singleton Services

| Singleton | Description |
|-----------|-------------|
| `DamageSystem` | Centralized damage processing pipeline |
| `DeathSystem` | Centralized death processing with cancellation |
| `TeleportSystem` | Cross-scene teleportation |
| `SceneSystem` | Scene-level state tracking (save points, enemy units) |
| `InteractSystem` | Sight-based interaction management |
| `PauseManager` | Multi-flag pause state management |
| `GameManager` | Game data and save management |

## Key Concepts

### Entry and GameEntry

- `GameEntry.cs` is the main MonoBehaviour that initializes the Entry system
- Modules implement `IComponent` interface and are registered via `Entry.Register()`
- Access modules via `GameEntry.GetModule<T>()` or convenience accessors

### IEntity and EntityCreator

- `IEntity` defines lifecycle: `OnSpawn()`, `EntityUpdate()`, `EntityFixedUpdate()`, `OnRecycle()`
- `EntityCreator<T>` manages creation, pooling, and per-frame updates for entities
- Register prefabs via `AddPrefab<T>(name, path)`, create via `CreateEntity<T>(name, pos, rot)`

### Buff System (核心架构)

All combat-related effects are implemented through the Buff system:

- **`BuffObj`**: Buff instance base class (IReference), manages Caster, Carrier, Stacks, lifecycle
- **`IBuffContainer`**: Implemented by CharacterState, BulletState, WeaponState
- **`BuffSystem`**: Global Buff manager (IComponent), handles add/remove/tick
- **Trigger Interfaces**: ITriggerBeforeCauseDamage, ITriggerAfterTakeDamage, ITriggerBeforeDie, etc.

See [Buff System Reference](.claude/docs/buff-system-reference.md) for comprehensive details.

### Character System

- **`CharacterState`**: Core character component (implements IDamageable, IBuffContainer, IEntity)
- **`ICharacterController`**: Controller interface for behaviors (Priority sorted, called every frame)
- **`ICharacterInitializer`**: Initializer interface for setup (Priority sorted, called on spawn)
- Used for both player and enemies in the new architecture

## Common Tasks

### Creating a Buff
```csharp
public class MyBuff : BuffObj, ITriggerAfterCauseDamage
{
    public override void OnAdd(AddBuffInfo addBuffInfo) { /* init */ }
    public void TriggerAfterCauseDamage(DamageInfo damageInfo) { /* logic */ }
    public override void Clear() { base.Clear(); /* cleanup */ }
}
```

### Applying Damage
```csharp
var damageInfo = DamageInfo.Create(defender: target, damage: 10, attacker: source);
DamageSystem.Instance.ApplyDamage(damageInfo);
```

### Firing a Bullet
```csharp
var launchInfo = BulletLaunchInfo.Create(model: bulletModel, logicSpawnPoint: pos, 
    rendererSpawnPoint: visualPos, spawnDirection: dir, source: player);
GameEntry.Bullet.FireBullet(launchInfo);
```

### Creating an Entity
```csharp
GameEntry.CharacterCreator.AddPrefab<CharacterState>("EnemyName", "Assets/path/prefab.prefab");
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("EnemyName", pos, rot);
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

For more examples, see [Code Patterns](.claude/docs/code-patterns.md).

## Project Status

The project is undergoing a major architecture refactor:

- **Weapon System**: Migrating from WeaponBase (IEffect) to WeaponState (Buff-based)
- **Player System**: Migrating from Player partial classes to CharacterState + Controllers
- **Enemy System**: Migrating from Module system to CharacterState + Controllers

See [Migration Guide](.claude/docs/migration-guide.md) for details on old vs new architectures.
