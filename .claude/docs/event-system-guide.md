# 新事件系统使用指南

## 概述

新的事件系统移除了对 `EventArgs` 的依赖，支持直接传递参数，使用更加简洁。

**当前状态**：新旧系统并存，逐步迁移中。

## 访问方式

### 旧系统（保持兼容）
```csharp
GameEntry.Event.Fire(sender, eventArgs);  // 使用 EventArgs
GameEntry.Event.Subscribe(id, handler);
```

### 新系统（推荐使用）
```csharp
GameEntry.EventV2.Fire<int>(id, value);  // 直接传参
GameEntry.EventV2.Subscribe<int>(id, handler);
```

## 核心组件

### 1. IEventSystem 接口
框架层的事件系统接口，支持 0-4 个参数的事件。

### 2. CEventSystem 实现
`IEventSystem` 的具体实现，提供线程安全的事件队列。

### 3. EventDispatcher 类
用于局部事件总线，可以被任何系统持有。

### 4. EventManager 组件
全局事件管理器，通过 `GameEntry.Event` 访问。

## 使用方式

### 定义事件 ID

推荐使用 enum 定义事件 ID：

```csharp
public enum PlayerEvent
{
    HealthChanged,
    MaxHealthChanged,
    ArmorChanged,
    KillPointsChanged,
    PositionChanged
}

public enum InputEvent
{
    MoveInput,
    ViewInput,
    FireInput,
    JumpInput,
    DashInput,
    ReloadInput,
    UpgradeInput
}
```

### 订阅事件

```csharp
// 无参数事件
GameEntry.EventV2.Subscribe((int)SystemEvent.GameStart, OnGameStart);

// 单参数事件
GameEntry.EventV2.Subscribe<int>((int)PlayerEvent.HealthChanged, OnHealthChanged);

// 多参数事件
GameEntry.EventV2.Subscribe<Vector2, float>((int)InputEvent.MoveInput, OnMoveInput);
```

### 触发事件

```csharp
// 延迟触发（下一帧执行，线程安全）
GameEntry.EventV2.Fire((int)SystemEvent.GameStart);
GameEntry.EventV2.Fire<int>((int)PlayerEvent.HealthChanged, newHealth);
GameEntry.EventV2.Fire<Vector2, float>((int)InputEvent.MoveInput, direction, magnitude);

// 立即触发（当前帧执行，非线程安全）
GameEntry.EventV2.FireNow((int)SystemEvent.GameStart);
GameEntry.EventV2.FireNow<int>((int)PlayerEvent.HealthChanged, newHealth);
```

### 取消订阅

```csharp
GameEntry.EventV2.Unsubscribe((int)SystemEvent.GameStart, OnGameStart);
GameEntry.EventV2.Unsubscribe<int>((int)PlayerEvent.HealthChanged, OnHealthChanged);
```

## 局部事件总线

如果某个系统需要自己的事件总线，可以使用 `EventDispatcher`：

```csharp
public class MySystem
{
    private readonly EventDispatcher _eventBus = new();

    public void Init()
    {
        _eventBus.Subscribe<int>((int)MyEvent.SomethingHappened, OnSomethingHappened);
    }

    public void Update()
    {
        // 处理延迟事件
        _eventBus.Update();
    }

    public void Dispose()
    {
        _eventBus.Dispose();
    }

    private void OnSomethingHappened(int value)
    {
        // 处理事件
    }
}
```

## 迁移指南

### 旧代码（使用 EventArgs）

```csharp
// 定义 EventArgs
public class HealthChangedEventArgs : GameEventArgs
{
    public override int Id => 1001;
    public int NewHealth;
    public int OldHealth;
}

// 触发事件
var args = ReferencePool.Acquire<HealthChangedEventArgs>();
args.NewHealth = 100;
args.OldHealth = 80;
GameEntry.Event.Fire(this, args);

// 订阅事件
GameEntry.Event.Subscribe(1001, OnHealthChanged);

void OnHealthChanged(object sender, GameEventArgs e)
{
    var args = (HealthChangedEventArgs)e;
    Debug.Log($"Health: {args.OldHealth} -> {args.NewHealth}");
}
```

### 新代码（直接传参）

```csharp
// 定义事件 ID
public enum PlayerEvent
{
    HealthChanged = 1001
}

// 触发事件
GameEntry.EventV2.Fire<int, int>((int)PlayerEvent.HealthChanged, newHealth, oldHealth);

// 订阅事件
GameEntry.EventV2.Subscribe<int, int>((int)PlayerEvent.HealthChanged, OnHealthChanged);

void OnHealthChanged(int newHealth, int oldHealth)
{
    Debug.Log($"Health: {oldHealth} -> {newHealth}");
}
```

## 优势

1. **无需创建 EventArgs 类**：减少样板代码
2. **无需对象池管理**：不需要 Acquire/Release
3. **类型安全**：编译时检查参数类型
4. **性能更好**：减少对象分配和 GC 压力
5. **代码更简洁**：直接传递参数，无需包装

## 注意事项

1. **事件 ID 管理**：建议使用 enum 定义事件 ID，避免硬编码数字
2. **参数数量限制**：最多支持 4 个参数，如需更多参数建议封装成结构体
3. **线程安全**：`Fire` 方法是线程安全的，`FireNow` 不是
4. **取消订阅**：记得在对象销毁时取消订阅，避免内存泄漏
