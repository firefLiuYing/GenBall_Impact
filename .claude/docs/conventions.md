# Conventions

This document outlines project conventions, naming patterns, and important notes.

## File Naming Conventions

### UI Files

- **Forms**: `FormName.cs` and `FormName.Bind.cs` pattern
  - Example: `MainHud.cs`, `MainHud.Bind.cs`
- **ViewModels**: `FormNameVm.cs` pattern
  - Example: `MainHudVm.cs`

### Partial Classes

Partial classes split functionality across multiple files:
- **GameEntry partials**: `GameEntry.Bullet.cs`, `GameEntry.Weapon.cs`, `GameEntry.Register.cs`
- **Player partials**: `Player.cs`, `Player.Health.cs`, `Player.Physics.cs`, `Player.Weapon.cs`
- **System partials**: `TimelineSystem.cs` (partial)

### Configuration Files

- **ScriptableObject configs**: End with `Config` suffix
  - Example: `BuffModelConfig`, `MapConfig`, `EvolutionConfig`

### Generated Code

- **Location**: `Generated/` subdirectories
- **Examples**:
  - `Assets/Scripts/GenBall/Event/Generated/GlobalEventSystem.Generated.cs`
  - `Assets/Scripts/GenBall/BattleSystem/Generated/EffectEvents.Generated.cs`
  - UI binding files: `FormName.Bind.cs`

## Code Organization

### Namespace Structure

- **`Yueyn`**: Core framework code
  - `Yueyn.Event`, `Yueyn.FSM`, `Yueyn.Pool`, etc.
- **`GenBall`**: Game-specific code
  - `GenBall.BattleSystem`, `GenBall.Player`, `GenBall.Enemy`, etc.

### Directory Structure

```
Assets/Scripts/
├── Yueyn/              # Core framework
│   ├── Event/
│   ├── FSM/
│   ├── Main/
│   ├── Pool/
│   ├── Resource/
│   └── UI/
└── GenBall/            # Game code
    ├── BattleSystem/
    ├── Enemy/
    ├── Event/
    ├── Framework/
    ├── Interact/
    ├── Map/
    ├── Player/
    ├── Procedure/
    └── UI/
```

## Custom Inspector Attributes

The project provides custom Unity inspector attributes:

- **`[InspectorButton]`**: Adds a button to the inspector
- **`[LiveData]`**: Displays live data in the inspector during play mode

**Example**:
```csharp
[InspectorButton("TestMethod")]
public void TestMethod()
{
    Debug.Log("Button clicked!");
}

[LiveData]
public int currentHealth;
```

## Language and Comments

- **Mixed Language**: The project uses both English and Chinese
  - Chinese comments are common in some areas (核心架构, 管理, etc.)
  - Code identifiers are primarily English
  - Documentation is bilingual where appropriate

## Version Control

- **Main Branch**: `master`
- **Current Branch**: `v2_framework` (framework refactor)
- **Git User**: Yueyn

## Unity Configuration

### Unity Version

- **Version**: 2022.3.42f1c1
- **Platform**: Windows (primary)

### Packages

- **Unity's New Input System**: Used for player input
- **DOTween**: Animation and tweening
- **TextMeshPro**: UI text rendering

## Architecture Notes

### System Migration Status

The project is undergoing a major architecture refactor:

- **Weapon System**: 新旧武器系统并存
  - Old: `WeaponBase` (IEffect based) 为旧架构
  - New: `WeaponState` (IBuff based) 为新架构，正在迁移中

- **Player System**: 旧版Player类正在向新的CharacterState架构迁移
  - Old: `Player` partial classes (Player.cs, Player.Health.cs, etc.)
  - New: `CharacterState` with Controller/Initializer pattern

- **Enemy System**: Enemy Module系统正在向新的CharacterState架构迁移
  - Old: Module-based (AttackModule, DetectModule, MoveModule, etc.)
  - New: CharacterState with Controller/Initializer pattern

See [Migration Guide](migration-guide.md) for detailed migration information.

### Design Patterns

- **Component Pattern**: IComponent modules registered with Entry system
- **Entity Pattern**: IEntity lifecycle with EntityCreator pooling
- **Singleton Pattern**: Cross-cutting services via SingletonManager
- **Observer Pattern**: Variable<T> for observable values
- **Command Pattern**: MoveCommand, RotateCommand for character control
- **State Pattern**: FSM system for state machines
- **Object Pool Pattern**: ReferencePool and ObjectPool for recycling

## Best Practices

### Memory Management

- Use `ReferencePool` for data objects implementing `IReference`
- Use `ObjectPool` (via EntityCreator) for GameObjects
- Always call `Clear()` when releasing pooled objects

### Event Handling

- Prefer type-safe generated event methods over manual Subscribe/Fire
- Unsubscribe from events in OnDestroy or OnRecycle
- Use local events for entity-specific communication

### Buff System

- All combat effects should use the Buff system
- Implement appropriate trigger interfaces for event hooks
- Use Stats system for dynamic value calculation

### Code Generation

- Regenerate UI bindings after modifying UI prefabs
- Regenerate event code after modifying event templates
- Generated files are marked with `.Generated.cs` suffix

## Related Documentation

- [Architecture Guide](architecture.md) - System architecture
- [Migration Guide](migration-guide.md) - Old vs new system migration
- [Code Patterns](code-patterns.md) - Implementation examples
