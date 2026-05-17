# 事件系统重构总结

## 当前状态

### 新框架事件系统（ISystem 体系）

**已完成**：
- ✅ `IEventSystem` 接口定义
- ✅ `CEventSystem` 实现（支持 0-4 个参数，无需 EventArgs）
- ✅ `EventDispatcher` 类（用于局部事件总线）
- ✅ 注册到 SystemRepository（在 FrameworkDefault 中）

**访问方式**：
```csharp
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();
eventSystem.Fire<int>((int)GlobalEvents.Player.HealthChanged, newHealth);
eventSystem.Subscribe<int>((int)GlobalEvents.Player.HealthChanged, OnHealthChanged);
```

**特性**：
- 无需 EventArgs 包装
- 无需 ReferencePool 管理
- 支持延迟触发（Fire）和立即触发（FireNow）
- 线程安全的事件队列

### 旧框架事件系统（IComponent 体系）

**保持运行**：
- `EventManager` (MonoBehaviour + IComponent)
- `EventPool<GameEventArgs>`
- 代码生成器生成的扩展方法

**访问方式**：
```csharp
GameEntry.Event.Fire(sender, eventArgs);
GameEntry.Event.Subscribe(id, handler);
```

**状态**：保持不变，继续支持现有代码

---

## 新旧系统对比

| 特性 | 旧系统（IComponent） | 新系统（ISystem） |
|------|---------------------|------------------|
| 访问方式 | `GameEntry.Event` | `SystemRepository.Instance.GetSystem<IEventSystem>()` |
| 参数传递 | 必须用 EventArgs 包装 | 直接传递参数（0-4个） |
| 对象池 | 需要 Acquire/Release | 无需管理 |
| 线程安全 | 是（队列） | 是（队列） |
| 延迟触发 | Fire | Fire |
| 立即触发 | FireNow | FireNow |
| 事件 ID | int（代码生成） | int（enum 定义） |

---

## 迁移指南

### 1. 定义事件 ID

在 `GlobalEvents.cs` 中定义：
```csharp
public static class GlobalEvents
{
    public enum Player
    {
        HealthChanged = 1000,
        MaxHealthChanged = 1001,
        // ...
    }
}
```

### 2. 触发事件

**旧代码**：
```csharp
var args = ReferencePool.Acquire<ValueChangeEventArgs<int>>();
args.Value = newHealth;
GameEntry.Event.Fire(this, args);
```

**新代码**：
```csharp
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();
eventSystem.Fire<int>((int)GlobalEvents.Player.HealthChanged, newHealth);
```

### 3. 订阅事件

**旧代码**：
```csharp
GameEntry.Event.Subscribe(eventId, OnEvent);

void OnEvent(object sender, GameEventArgs e)
{
    var args = (ValueChangeEventArgs<int>)e;
    var value = args.Value;
}
```

**新代码**：
```csharp
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();
eventSystem.Subscribe<int>((int)GlobalEvents.Player.HealthChanged, OnHealthChanged);

void OnHealthChanged(int newHealth)
{
    // 直接使用参数
}
```

---

## 注意事项

1. **不要修改旧系统**：EventManager 和 EventPool 保持不变
2. **不要修改生成的代码**：`GlobalEventSystem.Generated.cs` 等文件
3. **新功能使用新系统**：新开发的模块直接使用 IEventSystem
4. **旧代码逐步迁移**：按模块逐个迁移，不要一次性全部修改

---

## 文件位置

### 新系统
- `Yueyn/Event/IEventSystem.cs` - 接口定义
- `Yueyn/Event/CEventSystem.cs` - 实现类
- `Yueyn/Event/EventDispatcher.cs` - 局部事件总线
- `GenBall/Event/GlobalEvents.cs` - 事件 ID 定义
- `GenBall/Framework/FrameworkDefault.cs` - 系统注册

### 旧系统（保持不变）
- `Yueyn/Event/EventManager.cs` - 旧事件管理器
- `Yueyn/Base/EventPool/` - 旧事件池实现
- `GenBall/Event/Generated/` - 代码生成的文件

---

## 后续工作

1. 逐步迁移现有代码到新系统
2. 所有代码迁移完成后删除旧系统
3. 删除 EventArgs 相关类
4. 更新或移除代码生成器
