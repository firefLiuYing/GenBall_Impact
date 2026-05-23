# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Unity 2022.3.42f1c1 action game with a dual-track architecture ¡ª the old `IComponent` system (stable, being phased out) and the new `ISystem` framework (under active migration). The project is mid-refactor with **both systems running in parallel**.

## Quick Start

- **Open project**: Unity 2022.3.42f1c1 via Unity Hub
- **Entry scene**: `Assets/Scenes/Launcher.unity`
- **Primary namespace (framework)**: `Yueyn.*`
- **Primary namespace (game)**: `GenBall.*`
- **Current refactor branch**: `v2_framework`

## Architecture ¡ª Dual-Track System

### New Framework (ISystem ¡ª Active Migration)

```
Yueyn.Main
©À©¤©¤ ISystem           ¡ª Core interface (Init/UnInit)
©À©¤©¤ IFrameUpdate      ¡ª Frame update interface
©À©¤©¤ ILogicUpdate      ¡ª Logic update interface
©À©¤©¤ ILateFrameUpdate  ¡ª Late frame update interface
©À©¤©¤ FrameworkBase     ¡ª MonoBehaviour entry point (DontDestroyOnLoad)
©À©¤©¤ SystemRepository  ¡ª IoC container for system registration/lookup
©À©¤©¤ SystemUpdater     ¡ª Per-system update scheduler
©¸©¤©¤ SystemUpdaterManager ¡ª Unified update dispatch with pause support
```

**Access pattern**: `SystemRepository.Instance.GetSystem<ISomeSystem>()`

**Registered infrastructure modules** (in `FrameworkBase` / `FrameworkDefault`):
- `IEventSystem` (CEventSystem) ¡ª Event system
- `IResourceSystem` (ResourceSystemEditor/AssetBundle) ¡ª Resource loading
- `IUISystem` (UISystemDefault) ¡ª UI management
- `IPoolSystem` (PoolSystemDefault) ¡ª Object pooling

**Rules for new systems**:
- Must implement `ISystem`, never inherit `MonoBehaviour`
- Frame update via `IFrameUpdate`/`ILogicUpdate`/`ILateFrameUpdate`
- Interfaces in `Yueyn` namespace, implementations in `Yueyn`
- Business logic in `GenBall` namespace, not `Yueyn`
- Register via `SystemRepository.Instance.RegisterSystem<T>(impl)`

### Old Framework (IComponent ¡ª Still Running, No New Features)

```
Yueyn.Main.Entry
©À©¤©¤ Entry        ¡ª Module registration and lifecycle
©À©¤©¤ IComponent   ¡ª Module interface (Init/ComponentUpdate/Shutdown)
©¸©¤©¤ GameEntry    ¡ª MonoBehaviour entry, drives Update/FixedUpdate
```

**Access pattern**: `GameEntry.GetModule<T>()` or convenience accessors (`GameEntry.Event`, `GameEntry.UI`, `GameEntry.Buff`, `GameEntry.Player`, etc.)

**Registered modules**: EventManager, UIManager, FsmManager, ObjectPoolManager, ResourceManager, TimerManager, SaveComponent, PlayerManager, MapModule, SceneModule, ExecuteComponent, BuffSystem, BulletSystem, TimelineSystem, EvolutionSystem

**Do NOT modify** old system code unless absolutely necessary for migration.

## Core Patterns

| Pattern | Usage |
|---------|-------|
| **EntityCreator** | Generic factory+pool for all entities (Character, Bullet, Weapon, Enemy, UI, Map) |
| **IEntity** | Entity lifecycle: OnSpawn ¡ú EntityUpdate/EntityFixedUpdate ¡ú OnRecycle |
| **Singleton** | Cross-cutting services: DamageSystem, DeathSystem, InteractSystem, PauseManager, GameManager, SceneSystem, TeleportSystem |
| **FSM** | State machines for Player, Enemy, Game Procedure |
| **Buff System** | All combat effects via BuffObj + trigger interfaces (ITriggerBeforeCauseDamage, etc.) |
| **ReferencePool** | IReference-based object recycling (DamageInfo, DeathInfo, BuffObj, VmBase, etc.) |
| **ObjectPool** | GameObject pooling via EntityCreator |
| **Variable\<T\>** | Observable value wrapper with change notification |
| **CharacterState** | Core component for all characters (player + enemies), uses Controller/Initializer pattern |

## Code Generation ¡ª NEVER EDIT MANUALLY

Files in these categories are auto-generated. Modify the generator template/config instead:

- `**/Generated/*.Generated.cs` ¡ª Event system generated code
- `**/*.Bind.cs` ¡ª UI binding code
- `Assets/Scripts/GenBall/Event/Generated/GlobalEventSystem.Generated.cs`
- `Assets/Scripts/GenBall/BattleSystem/Generated/EffectEvents.Generated.cs`

## Battle System Architecture

```
Damage Pipeline: ITriggerBeforeCauseDamage ¡ú ITriggerBeforeTakeDamage ¡ú IDamageable.TakeDamage() ¡ú ITriggerAfterCauseDamage ¡ú ITriggerAfterTakeDamage
Death Pipeline:  ITriggerBeforeDie (can cancel) ¡ú ITriggerAfterDie ¡ú ITriggerAfterKill ¡ú IHealth.Die()
```

**Key types**: DamageInfo, DeathInfo, BulletLaunchInfo, AddBuffInfo (all IReference-pooled)

## Migration Status (v2_framework branch)

The project is migrating from effect-based systems (`IEffect`, `IWeapon`, `WeaponBase`) to a unified Buff-based architecture:

| System | Old (IComponent) | New (ISystem/CharacterState) |
|--------|------------------|------------------------------|
| Framework | `GameEntry` + `IComponent` | `FrameworkBase` + `ISystem` + `SystemRepository` |
| Weapons | `WeaponBase` + `IEffect` | `WeaponState` + Buff system |
| Player | `Player` partial classes | `CharacterState` + Controllers/Initializers |
| Enemy | Module-based (AttackModule, etc.) | `CharacterState` + Controllers/Initializers |
| Resource | Editor/AssetBundle helpers | `IResourceSystem` (compile-macro switched) |

**See detailed docs in**: `.claude/docs/` and `.codebuddy/`

## Key Development Conventions

- **UI**: Stack-based forms (FormBase), MVVM (VmBase), Legacy Text components (no TextMeshPro)
- **Resource loading**: `#if UNITY_EDITOR` compile macros (no runtime `Application.isEditor`)
- **Event handling**: Prefer type-safe generated methods over manual Subscribe/Fire
- **Memory**: Use ReferencePool for data objects, ObjectPool/EntityCreator for GameObjects
- **Custom attributes**: `[InspectorButton]`, `[LiveData]` for editor tooling
- **Partial classes**: Used for module separation (GameEntry.*.cs, Player.*.cs, TimelineSystem.cs)

## Documentation Index

- `.claude/docs/architecture.md` ¡ª Full module system deep-dive
- `.claude/docs/battle-systems.md` ¡ª Combat system details (Damage, Death, Character, Bullet, Weapon, Buff)
- `.claude/docs/buff-system-reference.md` ¡ª Comprehensive Buff system documentation
- `.claude/docs/systems-overview.md` ¡ª All game systems (Player, Enemy, UI, Map, Procedure, Events)
- `.claude/docs/framework-reference.md` ¡ª Yueyn framework utilities (EventPool, ReferencePool, FSM, Timer)
- `.claude/docs/code-patterns.md` ¡ª Implementation recipes and examples
- `.claude/docs/migration-guide.md` ¡ª Old vs new system migration path
- `.claude/docs/conventions.md` ¡ª Naming, organization, best practices
- `.claude/rules/code-modification-rules.md` ¡ª Rules for modifying code