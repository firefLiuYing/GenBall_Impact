# Framework Reference

This document provides reference information for the Yueyn framework utilities.

## Overview

The Yueyn framework is located in `Assets/Scripts/Yueyn/` and provides core utilities for event handling, object pooling, state machines, timers, and resource management.

## EventPool

Generic event system with subscribe/fire pattern.

### Features

- **Subscribe/Fire pattern**: Subscribe to events by ID, fire events with arguments
- **Fire-now mode**: Immediate event delivery
- **Handler modes**:
  - `AllowNoHandler`: Event can be fired even if no subscribers exist
  - `AllowMultiHandler`: Multiple subscribers can listen to the same event

### Usage

```csharp
// Subscribe to event
GameEntry.Event.Subscribe(eventId, OnEventHandler);

// Fire event
GameEntry.Event.Fire(eventId, sender, eventArgs);

// Unsubscribe
GameEntry.Event.Unsubscribe(eventId, OnEventHandler);
```

See [Code Patterns](code-patterns.md#subscribing-to-events) for more examples.

## ReferencePool

Object recycling system for data classes implementing `IReference`.

### Purpose

Reduces garbage collection pressure by reusing data objects instead of creating new instances.

### IReference Interface

```csharp
public interface IReference
{
    void Clear();  // Reset object state for reuse
}
```

### Commonly Pooled Types

- `DamageInfo` - Damage application data
- `DeathInfo` - Death processing data
- `BulletLaunchInfo` - Bullet firing parameters
- `AddBuffInfo` - Buff creation requests
- `BuffObj` - Buff instances
- `StatValue<T>` - Stat calculation values
- `VmBase` - UI ViewModel instances
- `AccessoryObj` - Accessory instances

### Usage Pattern

```csharp
// Acquire from pool
var info = ReferencePool.Acquire<DamageInfo>();
info.Defender = target;
info.Damage = 10;

// Use the object...

// Release back to pool
ReferencePool.Release(info);
```

### Static Helper Methods

Many pooled types provide static `Create()` methods:

```csharp
// Convenient creation
var damageInfo = DamageInfo.Create(
    defender: target,
    damage: 10,
    attacker: source
);
// Auto-released after DamageSystem.ApplyDamage() completes
```

## ObjectPool

GameObject pooling system with spawn/despawn lifecycle.

### Purpose

Reuses GameObjects to avoid instantiation overhead, used internally by `EntityCreator`.

### Lifecycle

1. **Spawn**: GameObject is activated and positioned
2. **Active**: GameObject is in use
3. **Despawn**: GameObject is deactivated and returned to pool

### Usage

Typically accessed through `EntityCreator` rather than directly:

```csharp
// EntityCreator handles pooling internally
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("PrefabName", position, rotation);
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

## Variable<T>

Observable value wrapper with change notification.

### Features

- **SetValue()**: Set value and notify observers
- **PostValue()**: Notify observers without changing value
- **Observe()**: Subscribe to value changes

### Usage

```csharp
// Create observable variable
var health = new Variable<int>(100);

// Observe changes
health.Observe(newValue => Debug.Log($"Health changed to {newValue}"));

// Set value (triggers observers)
health.SetValue(80);

// Post value (triggers observers without changing value)
health.PostValue();
```

### Common Use Cases

- UI data binding
- State synchronization
- Event-driven updates

## FSM (Finite State Machine)

Generic finite state machine system with data sharing.

### Core Types

- **`Fsm<T>`**: State machine instance for owner of type `T`
- **`FsmState<T>`**: Base class for states
- **`FsmManager`**: Manages all FSM instances (IComponent)

### Features

- **Data Sharing**: States can share data via `Variable<T>` stored in FSM
- **State Transitions**: `ChangeState<TState>()` to switch states
- **Lifecycle**: OnEnter → OnUpdate → OnLeave

### Usage

```csharp
// Create FSM
var fsm = GameEntry.Fsm.CreateFsm<Player>("PlayerFSM", player, 
    new PlayerIdleState(),
    new PlayerMoveState(),
    new PlayerJumpState()
);

// Start FSM
fsm.Start<PlayerIdleState>();

// Share data between states
fsm.SetData("Speed", new Variable<float>(5f));

// In state: access shared data
var speed = Fsm.GetData<Variable<float>>("Speed").Value;

// Change state
ChangeState<PlayerMoveState>(Fsm);
```

### State Implementation

```csharp
public class PlayerIdleState : FsmState<Player>
{
    protected override void OnEnter(Fsm<Player> fsm)
    {
        // State entry logic
    }

    protected override void OnUpdate(Fsm<Player> fsm, float deltaTime)
    {
        // State update logic
        if (Input.GetKey(KeyCode.W))
        {
            ChangeState<PlayerMoveState>(fsm);
        }
    }

    protected override void OnLeave(Fsm<Player> fsm, bool isShutdown)
    {
        // State exit logic
    }
}
```

## Timer

Timer system with event callbacks.

### Features

- One-shot and repeating timers
- Pause/resume support
- Event callbacks on completion

### Usage

```csharp
// Create timer (managed by TimerManager)
var timer = GameEntry.Timer.CreateTimer(
    duration: 3f,
    onComplete: () => Debug.Log("Timer complete!"),
    repeat: false
);

// Pause/resume
timer.Pause();
timer.Resume();

// Cancel
timer.Cancel();
```

## Resource Manager

Asset loading system.

### Purpose

Provides unified interface for loading assets, abstracts away Editor vs Runtime loading differences.

### Usage

```csharp
// Load asset
GameEntry.Resource.LoadAsset<GameObject>("Assets/Prefabs/Enemy.prefab", 
    onSuccess: (asset) => {
        var instance = Instantiate(asset);
    },
    onFailure: (error) => {
        Debug.LogError($"Failed to load: {error}");
    }
);
```

### Implementation Notes

- In Editor: Uses `AssetDatabase.LoadAssetAtPath`
- In Build: Uses AssetBundle system
- Async loading supported

## Related Documentation

- [Architecture Guide](architecture.md) - Module system and EntityCreator
- [Code Patterns](code-patterns.md) - Practical usage examples
