# Buff System Reference

This document provides comprehensive reference for the Buff system, which is the core architecture for all combat-related effects.

## Overview

核心架构，所有战斗相关的效果都通过Buff系统实现。

The Buff system provides a flexible, event-driven architecture for implementing combat effects, stat modifications, and gameplay mechanics.

## Core Components

### IBuff Interface

Buff基础接口，支持Priority、CanMultiExist、Tags

```csharp
public interface IBuff
{
    int Priority { get; }           // Execution order (higher = earlier)
    bool CanMultiExist { get; }     // Can multiple instances exist on same carrier
    List<string> Tags { get; }      // Buff classification tags
}
```

### BuffObj Base Class

Buff实例基类（IReference），管理Caster、Carrier、Stacks、TickTimer，支持OnAdd/OnStack/OnUnstack/OnRemove/OnUpdate生命周期

**Implements**: `IReference` (pooled via ReferencePool)

**Properties**:
- **Caster**: GameObject that applied the buff
- **Carrier**: GameObject carrying the buff
- **Stacks**: Current stack count
- **TickTimer**: Timer for periodic effects

**Lifecycle Methods**:
```csharp
public virtual void OnAdd(AddBuffInfo addBuffInfo)      // Called when buff is first added
public virtual void OnStack(AddBuffInfo addBuffInfo)    // Called when buff stacks
public virtual void OnUnstack(int removeStacks)         // Called when stacks are removed
public virtual void OnRemove()                          // Called when buff is completely removed
public virtual void OnUpdate(float deltaTime)           // Called every frame while active
public override void Clear()                            // IReference cleanup
```

### IBuffContainer Interface

Buff容器接口，由`CharacterState`、`BulletState`、`WeaponState`实现

Entities that can carry buffs implement this interface.

**Implementers**:
- `CharacterState` - Characters (player, enemies)
- `BulletState` - Bullets
- `WeaponState` - Weapons

### BuffSystem Component

**Type**: IComponent

全局Buff管理器，处理添加（含叠层逻辑）/移除/每帧Tick

**Responsibilities**:
- Add buffs with stacking logic
- Remove buffs
- Tick all active buffs every frame
- Trigger buff callbacks at appropriate times

**Key Methods**:
```csharp
void AddBuff(AddBuffInfo info)          // Add or stack a buff
void RemoveBuff(GameObject carrier, BuffId buffId, int stacks = -1)  // Remove buff
```

### BuffModel

**Type**: ScriptableObject

配置数据，`BuffModelConfig`提供按BuffId查询

Defines buff configuration:
- BuffId
- Duration
- Max stacks
- Tick interval
- Visual effects
- etc.

### AddBuffInfo

**Type**: IReference

创建Buff的请求数据（BuffId → BuffModel自动查找）

**Properties**:
- BuffId: Buff type identifier
- Carrier: Target GameObject
- Caster: Source GameObject
- AddStacks: Number of stacks to add
- (BuffModel is automatically looked up from BuffId)

## Buff Trigger Interfaces

Buff回调接口（trigger points）

Buffs implement these interfaces to hook into game events:

### Bullet Triggers

子弹相关触发点

- **`ITriggerBeforeFireBullet`**: Before bullet is fired
- **`ITriggerAfterFireBullet`**: After bullet is fired
- **`ITriggerBeforeBulletBeFired`**: Before this bullet is fired (on bullet itself)
- **`ITriggerAfterBulletBeFired`**: After this bullet is fired (on bullet itself)

### Damage Triggers

伤害相关触发点

- **`ITriggerBeforeCauseDamage`**: Before dealing damage (on attacker)
- **`ITriggerAfterCauseDamage`**: After dealing damage (on attacker)
- **`ITriggerBeforeTakeDamage`**: Before taking damage (on defender)
- **`ITriggerAfterTakeDamage`**: After taking damage (on defender)

### Lifecycle Triggers

生命周期相关触发点

- **`ITriggerBeforeDie`**: Before death (can cancel death)
- **`ITriggerAfterDie`**: After death (on victim)
- **`ITriggerAfterKill`**: After killing (on killer)

### Buff Management Triggers

Buff管理相关触发点

- **`ITriggerBeforeAddBuff`**: Before buff is added
- **`ITriggerAfterAddBuff`**: After buff is added
- **`ITriggerBeforeStackBuff`**: Before buff is stacked
- **`ITriggerAfterStackBuff`**: After buff is stacked

## Stats System

通用数值计算系统

The Stats system provides dynamic stat calculation with modifiers.

### StatValue<T>

**Type**: IReference (泛型属性值，支持BaseValue + Modifier列表)

Generic stat value supporting base value + modifier list.

**Formula**: `FinalValue = BaseValue + Σ(Modifiers)`

**Concrete Types**:
- **`IntStat`**: Integer stat (通过ReferencePool创建)
- **`FloatStat`**: Float stat (通过ReferencePool创建)

**Usage**:
```csharp
// Create stat
var damage = ReferencePool.Acquire<IntStat>();
damage.SetBaseValue(10);

// Add modifier
var addMod = new AddModifier<int>(5);
damage.AddModifier(addMod);

// Get final value
int finalDamage = damage.Value;  // 15

// Remove modifier
damage.RemoveModifier(addMod);

// Release
ReferencePool.Release(damage);
```

### StatModifier<T>

修饰器基类

Base class for stat modifiers.

**Concrete Types**:

- **`AddModifier<T>`**: 加法修饰
  - Adds a flat value to the stat
  
- **`FloatMultiplyModifier`**: 浮点乘法修饰
  - Multiplies stat by a float value
  
- **`IntMultiplyModifier`**: 整数乘法修饰（内部用float）
  - Multiplies int stat (uses float internally)

### DamageValue

**Type**: IReference

多区域伤害计算 = Σ(区域倍率) × 基础伤害 + 附加伤害

Multi-zone damage calculation.

**Formula**: `Total = Σ(zone multipliers) × base damage + add damage`

**Use Cases**:
- Headshot multipliers
- Weak point damage
- Critical hits
- Damage zones

## Creating Custom Buffs

### Basic Buff Example

```csharp
public class MyBuff : BuffObj, ITriggerAfterCauseDamage
{
    public override void OnAdd(AddBuffInfo addBuffInfo)
    {
        base.OnAdd(addBuffInfo);
        // Initialization logic
    }

    public void TriggerAfterCauseDamage(DamageInfo damageInfo)
    {
        // Logic after causing damage
        Debug.Log($"Dealt {damageInfo.Damage} damage!");
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        // Per-frame logic
    }

    public override void Clear()
    {
        base.Clear();
        // Cleanup resources
    }
}
```

### Stat Modification Buff Example

```csharp
public class DamageBuff : BuffObj
{
    private AddModifier<int> damageModifier;

    public override void OnAdd(AddBuffInfo addBuffInfo)
    {
        base.OnAdd(addBuffInfo);
        
        // Add +10 damage to weapon
        var weaponState = Carrier.GetComponent<WeaponState>();
        if (weaponState != null)
        {
            damageModifier = new AddModifier<int>(10);
            weaponState.WeaponStats.Damage.AddModifier(damageModifier);
        }
    }

    public override void OnRemove()
    {
        // Remove modifier
        var weaponState = Carrier.GetComponent<WeaponState>();
        if (weaponState != null && damageModifier != null)
        {
            weaponState.WeaponStats.Damage.RemoveModifier(damageModifier);
        }
        
        base.OnRemove();
    }
}
```

### Periodic Effect Buff Example

```csharp
public class PoisonBuff : BuffObj
{
    private float tickInterval = 1f;
    private int damagePerTick = 5;

    public override void OnAdd(AddBuffInfo addBuffInfo)
    {
        base.OnAdd(addBuffInfo);
        TickTimer = tickInterval;
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        
        TickTimer -= deltaTime;
        if (TickTimer <= 0)
        {
            // Deal damage
            var damageInfo = DamageInfo.Create(
                defender: Carrier,
                damage: damagePerTick,
                attacker: Caster
            );
            DamageSystem.Instance.ApplyDamage(damageInfo);
            
            // Reset timer
            TickTimer = tickInterval;
        }
    }
}
```

## Buff Registration

### Step 1: Add BuffId

Add new buff ID to `BuffId` enum:

```csharp
public enum BuffId
{
    None = 0,
    MyNewBuff = 100,
    // ...
}
```

### Step 2: Register Type Mapping

In `BuffIdToExtension.ToType()`:

```csharp
public static Type ToType(this BuffId buffId)
{
    return buffId switch
    {
        BuffId.MyNewBuff => typeof(MyNewBuff),
        // ...
    };
}
```

### Step 3: Create BuffModel

Create ScriptableObject configuration in `BuffModelConfig`.

### Step 4: Apply Buff

```csharp
var info = AddBuffInfo.Create(
    BuffId.MyNewBuff,
    targetGameObject,
    addStacks: 1,
    caster: casterGameObject
);
GameEntry.Buff.AddBuff(info);
```

## Related Documentation

- [Battle Systems](battle-systems.md) - Damage, Death, Character systems
- [Code Patterns](code-patterns.md#creating-a-new-buff) - Buff creation examples
- [Architecture Guide](architecture.md) - Module system overview