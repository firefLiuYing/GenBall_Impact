# Battle Systems

This document provides an overview of the combat-related systems in GenBall_Impact.

Located in `Assets/Scripts/GenBall/BattleSystem/`

## Damage System

`DamageSystem` (singleton) provides centralized damage processing with Buff trigger pipeline.

### Damage Processing Pipeline

1. Attacker's `ITriggerBeforeCauseDamage` buffs fire
2. Defender's `ITriggerBeforeTakeDamage` buffs fire
3. `IDamageable.TakeDamage()` executes actual damage
4. Attacker's `ITriggerAfterCauseDamage` buffs fire
5. Defender's `ITriggerAfterTakeDamage` buffs fire
6. `DamageInfo` released back to `ReferencePool`

### Key Types

- **`DamageInfo`** (IReference): Carries damage data
  - Defender: Target GameObject
  - Attacker: Source GameObject
  - Damage: Amount of damage
  - ImpactForce: Knockback force
  - Direction: Damage direction vector
  - Tags: Damage type tags (e.g., "bullet", "melee")
  - AddBuffs: Buffs to apply on hit

- **`DamageValue`** (IReference): Multi-zone damage calculation
  - Formula: `Σ(zone multipliers) × base damage + add damage`
  - Supports different damage zones (headshot, body, etc.)

- **`IHealth`**: Base health interface
  - Properties: Health, MaxHealth, IsDead
  - Method: Die()

- **`IDamageable`**: Extends IHealth
  - Method: TakeDamage(DamageInfo)

- **`IHealable`**: Extends IHealth
  - Method: Heal(int amount)

- **`IArmor`**: Armor management
  - Properties: Armor, MaxArmor

### Usage

See [Code Patterns - Applying Damage](code-patterns.md#applying-damage)

## Death System

`DeathSystem` (singleton) handles death processing with cancellation support.

### Death Processing Pipeline

1. Victim's `ITriggerBeforeDie` buffs fire
   - Can set `DeathInfo.Cancelled = true` to prevent death
2. If cancelled, abort death processing
3. Victim's `ITriggerAfterDie` buffs fire
4. Killer's `ITriggerAfterKill` buffs fire
5. `IHealth.Die()` executes
6. `DeathInfo` released to ReferencePool

### Key Types

- **`DeathInfo`** (IReference): Death event data
  - Victim: GameObject that died
  - Killer: GameObject that caused death
  - Cancelled: Flag to prevent death

### Cancellation Use Cases

- Resurrection buffs
- Invincibility effects
- "Second chance" mechanics

## Character System

`CharacterState` is the core component for all characters (player, enemies).

### CharacterState Component

**Implements**: `IDamageable`, `IBuffContainer`, `IEntity`

**Features**:
- Automatically collects child `ICharacterInitializer` and `ICharacterController` components (sorted by Priority)
- Command pattern for movement/rotation: `MoveCommand`, `RotateCommand` via `IMove`/`IRotate` interfaces
- Ability flags: CanMove, CanRotate, CanJump, CanAttack
- Health changes automatically trigger DeathSystem
- Supports PauseManager pause states

### CharacterStats

Created from `CharacterStatsModel`, currently includes:
- MaxHealth (IntStat)

### Controller and Initializer Pattern

**`ICharacterInitializer`**: Initialization interface (Priority sorted)
- Called during OnSpawn
- Used for setup (armor, UI, camera, etc.)

**`ICharacterController`**: Control interface (Priority sorted)
- Called every frame (Tick)
- Used for behavior (movement, jumping, weapons, state machines, etc.)

**Base Classes**:
- `CharacterInitializerBase`: MonoBehaviour base for initializers
- `CharacterControllerBase`: MonoBehaviour base for controllers

### Movement Commands

- **`IMove`**: Interface for movement capability
- **`IRotate`**: Interface for rotation capability
- **`MoveCommand`**: Encapsulates movement logic
- **`RotateCommand`**: Encapsulates rotation logic

## Bullet System

New bullet architecture using BulletState and BulletSystem.

### BulletSystem Component

**Type**: IComponent

**Responsibilities**:
- Unified bullet firing entry point: `FireBullet(BulletLaunchInfo)`
- Handles before/after fire Buff triggers
- Manages bullet lifecycle

### BulletState Component

**Implements**: `IBuffContainer`, `IEntity`

**Properties**:
- BulletModel: Configuration data
- Source: GameObject that fired the bullet
- LogicSpawnPoint: Logical spawn position
- RendererSpawnPoint: Visual spawn position
- SpawnDirection: Initial direction

### Key Types

- **`BulletLaunchInfo`** (IReference): Bullet firing parameters
  - Source: Firing GameObject
  - Model: BulletModel configuration
  - LogicSpawnPoint: Logic position
  - RendererSpawnPoint: Visual position
  - SpawnDirection: Direction vector

- **`IBulletController`**: Bullet behavior interface
  - Methods: Init(), Fire(), Tick()
  - Example: `RayBulletController` for hitscan bullets

- **`BulletModel`**: ScriptableObject configuration for bullets

### Usage

See [Code Patterns - Firing a Bullet](code-patterns.md#firing-a-bullet)

## Weapon System

Two weapon architectures coexist: old (WeaponBase) and new (WeaponState).

### New Architecture (WeaponState)

**`WeaponState`**: Implements `IBuffContainer`, `IEntity`

**Components**:
- WeaponStats: Stat values (Damage, FireInterval, ReloadTime)
- Accessory: Attached accessories
- Trigger Controller: Firing logic
- Reload Controller: Reload logic

**Key Interfaces**:
- **`IWeaponTriggerController`**: Shooting control
  - Example: `NormalTriggerController`
- **`IWeaponReloadController`**: Reload control
  - Example: `NormalReloadController`

**WeaponStats**:
- Damage (DamageValue)
- FireInterval (FloatStat)
- ReloadTime (FloatStat)

**WeaponModel**: Serialized configuration (damage, fireInterval, reloadTime)

### Old Architecture (WeaponBase)

**Status**: Still in use, being migrated

**`WeaponBase`**: Abstract MonoBehaviour

**Implements**: `IWeapon`, `IEffectable`

**Features**:
- Manages IEffect list
- Manages IWeaponComponent list

**Key Interfaces**:
- **`IWeapon`**: Equip/Unequip/Trigger/Attack + Stats
- **`IWeaponComponent`**: Weapon sub-components
  - Examples: `FireComponent`, `MagazineComponent`

See [Migration Guide](migration-guide.md) for details on transitioning between architectures.

## Accessory System

Accessories modify weapon properties through the Buff system.

### Key Types

- **`AccessoryObj`** (IReference): Accessory instance
  - OnAdd: Adds Buffs to WeaponState
  - OnRemove: Removes Buffs from WeaponState

- **`AccessoryModel`**: ScriptableObject configuration
  - Defines list of Buffs to add

- **`AccessoryModelConfig`**: Accessory configuration manager

### How It Works

1. Accessory is equipped to weapon
2. AccessoryObj.OnAdd() adds configured Buffs to WeaponState
3. Buffs modify weapon stats (damage, fire rate, etc.)
4. When unequipped, AccessoryObj.OnRemove() removes Buffs

## Evolution System

`EvolutionSystem` (IComponent) manages weapon evolution through kills.

### Features

- Gain KillPoints from kills
- Evolve weapon when threshold reached
- Configurable evolution stages
- Each stage grants new weapon + accessories

### Key Types

- **`EvolutionConfig`**: ScriptableObject
  - Defines kill requirements per stage
  - Maximum evolution level (default: 4)

- **`EquipInfo`**: Maps evolution level to:
  - Weapon ID
  - Accessory list

### Workflow

1. Player kills enemy
2. KillPoints increment
3. When threshold reached, evolution available
4. Player evolves weapon
5. New weapon + accessories equipped

## Timeline System

`TimelineSystem` (IComponent, partial) manages cutscenes and skill timelines.

### Key Types

- **`TimelineObj`**: Timeline instance
  - Supports TimeScale
  - Tick in FixedUpdate
  - Respects PauseManager pause states

- **`TimelineModelConfig`**: Configuration data

### Example Implementation

- `DashTimeline`: Dash skill timeline

### Usage

Timelines are used for:
- Cutscenes
- Skill animations
- Timed sequences

## Related Documentation

- [Buff System Reference](buff-system-reference.md) - Detailed Buff system documentation
- [Code Patterns](code-patterns.md) - Implementation examples
- [Migration Guide](migration-guide.md) - Old vs new system architectures