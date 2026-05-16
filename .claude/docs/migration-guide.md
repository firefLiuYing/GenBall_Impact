# Migration Guide

This document explains the differences between old and new system architectures and provides guidance for migration.

## Overview

The GenBall_Impact project is undergoing a major architecture refactor from effect-based systems to a unified Buff-based architecture. This migration affects weapons, players, and enemies.

## Weapon System Migration

### Old Architecture (WeaponBase)

**Status**: Still in use, being phased out

**Key Components**:
- `WeaponBase`: Abstract MonoBehaviour base class
- `IWeapon`: Weapon interface (Equip/Unequip/Trigger/Attack + Stats)
- `IEffect`: Effect system for weapon behaviors
- `IEffectable`: Interface for entities that can have effects
- `IWeaponComponent`: Weapon sub-components (FireComponent, MagazineComponent)

**Characteristics**:
- Effect-based behavior system
- Tightly coupled to MonoBehaviour
- Limited composability
- Effects are weapon-specific

**Example**:
```csharp
public class OldWeapon : WeaponBase
{
    private List<IEffect> effects;
    private FireComponent fireComponent;
    private MagazineComponent magazineComponent;
    
    public override void Attack()
    {
        // Trigger effects
        foreach (var effect in effects)
        {
            effect.Execute();
        }
    }
}
```

### New Architecture (WeaponState)

**Status**: Active, recommended for new weapons

**Key Components**:
- `WeaponState`: Implements `IBuffContainer`, `IEntity`
- `IWeaponTriggerController`: Shooting control interface
- `IWeaponReloadController`: Reload control interface
- `WeaponStats`: Dynamic stats (Damage, FireInterval, ReloadTime)
- `WeaponModel`: ScriptableObject configuration
- Buff system for all weapon effects

**Characteristics**:
- Buff-based behavior system
- Decoupled from MonoBehaviour (uses IEntity)
- Highly composable via Buffs
- Effects are universal (can apply to any IBuffContainer)

**Example**:
```csharp
// WeaponState setup
WeaponPrefab (GameObject)
├── WeaponState (Component)
│   └── WeaponModel: damage, fireInterval, reloadTime
├── NormalTriggerController (IWeaponTriggerController)
└── NormalReloadController (IWeaponReloadController)

// Weapon effects via Buffs
var damageBuffInfo = AddBuffInfo.Create(BuffId.IncreaseDamage, weaponGameObject, 1, caster);
GameEntry.Buff.AddBuff(damageBuffInfo);
```

### Migration Path

1. **Identify weapon behaviors**: List all IEffect implementations
2. **Convert to Buffs**: Create Buff equivalents for each effect
3. **Update weapon prefab**: Replace WeaponBase with WeaponState
4. **Add controllers**: Attach IWeaponTriggerController and IWeaponReloadController
5. **Configure WeaponModel**: Set up stats in ScriptableObject
6. **Test**: Verify all behaviors work correctly

### When to Use Each

- **Use Old (WeaponBase)**: Only for existing weapons not yet migrated
- **Use New (WeaponState)**: For all new weapons and migrated weapons

## Player System Migration

### Old Architecture (Player Partial Classes)

**Status**: Still in use, being phased out

**Key Components**:
- `Player`: Main MonoBehaviour (partial class)
- `Player.Health.cs`: Health management
- `Player.Physics.cs`: Physics and movement
- `Player.Weapon.cs`: Weapon handling
- `Player.Control.cs`: Input control
- `Player.Fsm.cs`: State machine
- `Player.Countdown.cs`: Timer management

**Characteristics**:
- Monolithic partial class structure
- Tightly coupled systems
- Hard to extend or reuse
- Player-specific implementations

**Example**:
```csharp
// Player.Health.cs
public partial class Player
{
    private int health;
    private int maxHealth;
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }
}

// Player.Physics.cs
public partial class Player
{
    private void HandleMovement()
    {
        // Movement logic
    }
}
```

### New Architecture (CharacterState + Controllers)

**Status**: Active, recommended

**Key Components**:
- `CharacterState`: Core character component (implements IDamageable, IBuffContainer, IEntity)
- `ICharacterController`: Controller interface for behaviors
- `ICharacterInitializer`: Initializer interface for setup
- Controller pattern for modular behaviors
- Buff system for all character effects

**Characteristics**:
- Modular controller-based architecture
- Loosely coupled systems
- Highly reusable (same system for player and enemies)
- Universal implementations

**Example**:
```csharp
// Player setup
PlayerPrefab (GameObject)
├── CharacterState (Component)
│   └── CharacterStatsModel: baseHealth = 100
├── PlayerMover (ICharacterController, IMove)
├── JumpController (ICharacterController)
├── WeaponController (ICharacterController)
├── PlayerStateMachine (ICharacterController)
├── PlayerArmorInitializer (ICharacterInitializer)
└── PlayerUiInitializer (ICharacterInitializer)

// Health is handled by CharacterState (IDamageable)
// Movement is handled by PlayerMover (ICharacterController)
// Weapons are handled by WeaponController (ICharacterController)
```

### Migration Path

1. **Create CharacterState prefab**: Set up base character component
2. **Convert systems to controllers**: Each Player partial becomes a controller
   - `Player.Physics.cs` → `PlayerMover` (ICharacterController)
   - `Player.Weapon.cs` → `WeaponController` (ICharacterController)
   - `Player.Fsm.cs` → `PlayerStateMachine` (ICharacterController)
3. **Convert initialization to initializers**: Setup code becomes initializers
   - Armor setup → `PlayerArmorInitializer`
   - UI setup → `PlayerUiInitializer`
   - Camera setup → `PlayerCameraInitializer`
4. **Use CharacterState health**: Remove custom health code, use IDamageable
5. **Test**: Verify all player functionality works

### When to Use Each

- **Use Old (Player)**: Only for existing player code not yet migrated
- **Use New (CharacterState)**: For all new player features and migrated code

## Enemy System Migration

### Old Architecture (Module System)

**Status**: Still in use, being phased out

**Key Components**:
- `EnemyBase`: Base enemy class
- `AttackModule`: Attack behavior
- `DetectModule`: Detection logic
- `MoveModule`: Movement logic
- `HurtModule`: Damage handling
- `FsmModule`: State machine

**Characteristics**:
- Module-based architecture
- Enemy-specific modules
- Limited reusability
- Separate from player system

**Example**:
```csharp
public class OldEnemy : EnemyBase
{
    private AttackModule attackModule;
    private DetectModule detectModule;
    private MoveModule moveModule;
    
    private void Update()
    {
        detectModule.Detect();
        moveModule.Move();
        attackModule.TryAttack();
    }
}
```

### New Architecture (CharacterState + Controllers)

**Status**: Active, recommended

**Key Components**:
- `CharacterState`: Same as player (unified system)
- `ICharacterController`: Enemy AI controllers
- `ICharacterInitializer`: Enemy setup
- Buff system for enemy effects

**Characteristics**:
- Unified with player system
- Reusable controllers
- Composable behaviors
- Buff-based effects

**Example**:
```csharp
// Enemy setup
EnemyPrefab (GameObject)
├── CharacterState (Component)
│   └── CharacterStatsModel: baseHealth = 50
├── EnemyAI (ICharacterController)
├── EnemyMover (ICharacterController, IMove)
├── EnemyAttack (ICharacterController)
└── EnemyInitializer (ICharacterInitializer)

// AI logic in EnemyAI controller
// Movement in EnemyMover controller
// Attack in EnemyAttack controller
```

### Migration Path

1. **Create CharacterState prefab**: Set up base enemy character
2. **Convert modules to controllers**:
   - `AttackModule` → `EnemyAttack` (ICharacterController)
   - `DetectModule` → Part of `EnemyAI` (ICharacterController)
   - `MoveModule` → `EnemyMover` (ICharacterController)
   - `FsmModule` → `EnemyStateMachine` (ICharacterController)
3. **Use CharacterState health**: Remove HurtModule, use IDamageable
4. **Test**: Verify enemy behavior

### When to Use Each

- **Use Old (Module System)**: Only for existing enemies not yet migrated
- **Use New (CharacterState)**: For all new enemies and migrated enemies

## IEffect vs Buff System

### Old: IEffect System

**Characteristics**:
- Weapon-specific effects
- Tightly coupled to IEffectable entities
- Limited trigger points
- Hard to compose

**Example**:
```csharp
public class DamageEffect : IEffect
{
    public void Execute(IEffectable target)
    {
        target.TakeDamage(10);
    }
}
```

### New: Buff System

**Characteristics**:
- Universal effects (work on any IBuffContainer)
- Rich trigger point system
- Highly composable
- Stats system for dynamic values

**Example**:
```csharp
public class DamageBuff : BuffObj, ITriggerAfterCauseDamage
{
    private FloatStat damageMultiplier;
    
    public override void OnAdd(AddBuffInfo info)
    {
        // Add damage multiplier to weapon stats
        var weaponState = Carrier.GetComponent<WeaponState>();
        damageMultiplier = ReferencePool.Acquire<FloatStat>();
        damageMultiplier.SetBaseValue(1.5f);
        weaponState.Stats.Damage.AddModifier(new FloatMultiplyModifier(1.5f));
    }
    
    public void TriggerAfterCauseDamage(DamageInfo info)
    {
        // Additional logic after causing damage
    }
}
```

### Migration Path

1. **Identify all IEffect implementations**
2. **Create Buff equivalents**: Implement BuffObj with appropriate trigger interfaces
3. **Register Buffs**: Add to BuffId enum and BuffModelConfig
4. **Replace IEffect calls**: Use GameEntry.Buff.AddBuff() instead
5. **Test**: Verify behavior matches

## Migration Timeline

### Phase 1: Foundation (Completed)
- ✅ Buff system implementation
- ✅ CharacterState system
- ✅ BulletState system
- ✅ WeaponState system

### Phase 2: Player Migration (In Progress)
- ⏳ Convert Player partial classes to controllers
- ⏳ Migrate player-specific effects to Buffs
- ⏳ Test player functionality

### Phase 3: Enemy Migration (In Progress)
- ⏳ Convert enemy modules to controllers
- ⏳ Migrate enemy-specific effects to Buffs
- ⏳ Test enemy AI

### Phase 4: Weapon Migration (In Progress)
- ⏳ Convert WeaponBase weapons to WeaponState
- ⏳ Migrate IEffect to Buff system
- ⏳ Test weapon behaviors

### Phase 5: Cleanup (Pending)
- ⬜ Remove old Player partial classes
- ⬜ Remove old enemy module system
- ⬜ Remove IEffect system
- ⬜ Remove WeaponBase

## Best Practices

### During Migration

1. **Keep both systems working**: Don't break existing functionality
2. **Migrate incrementally**: One system at a time
3. **Test thoroughly**: Verify behavior matches before removing old code
4. **Document changes**: Update documentation as you migrate
5. **Communicate**: Let team know what's being migrated

### After Migration

1. **Remove old code**: Clean up deprecated systems
2. **Update documentation**: Remove references to old systems
3. **Refactor**: Improve new code based on lessons learned
4. **Share knowledge**: Document migration patterns for future reference

## Related Documentation

- [Architecture Guide](architecture.md) - New system architecture
- [Battle Systems](battle-systems.md) - Combat system details
- [Buff System Reference](buff-system-reference.md) - Comprehensive Buff guide
- [Code Patterns](code-patterns.md) - Implementation examples
- [Conventions](conventions.md) - Project conventions
