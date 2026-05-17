# 事件系统迁移清单

## 迁移状态

当前项目正在从旧的 EventArgs 系统迁移到新的直接参数系统。

### 系统状态

- ✅ **新系统已实现**：`CEventSystem`, `EventDispatcher`
- ✅ **双系统并存**：`EventManager` 同时支持新旧系统
- ✅ **访问接口**：`GameEntry.Event`（旧）, `GameEntry.EventV2`（新）
- ✅ **事件定义**：`GlobalEvents` 枚举
- ✅ **文档完成**：使用指南和迁移示例

### 迁移策略

1. **保留旧系统**：所有旧代码继续正常工作
2. **新代码使用新系统**：新功能直接使用 `GameEntry.EventV2`
3. **逐步迁移旧代码**：按模块逐个迁移
4. **验证后删除**：所有代码迁移完成后再删除旧系统

---

## 需要迁移的文件

### 输入系统

- [ ] `Assets/Scripts/GenBall/Player/Input/InputController.cs`
  - 当前：使用 `InputEventArgs<T>` + `ReferencePool`
  - 目标：使用 `GameEntry.EventV2.Fire<T>`
  - 示例：`InputControllerV2Example.cs`

### 玩家系统

- [ ] `Assets/Scripts/GenBall/Player/Player.Control.cs`
- [ ] `Assets/Scripts/GenBall/Player/States/PlayerMoveState.cs`
- [ ] `Assets/Scripts/GenBall/Player/States/PlayerJumpState.cs`

### 敌人系统

- [ ] `Assets/Scripts/GenBall/Enemy/Fsm/Melee/MeleeFsmModule.cs`
- [ ] 其他使用 `EnemyDeadEventArgs` 的文件

### 武器系统

- [ ] 使用 `EffectEventArgs` 的文件

---

## 迁移步骤

### 1. 识别事件使用

查找文件中的：
- `ReferencePool.Acquire<XXXEventArgs>()`
- `GameEntry.Event.Fire(sender, eventArgs)`
- `GameEntry.Event.Subscribe(id, handler)`

### 2. 定义事件 ID

在 `GlobalEvents.cs` 中添加对应的枚举：
```csharp
public enum ModuleName
{
    EventName = 1000,
    // ...
}
```

### 3. 替换触发代码

**旧代码**：
```csharp
var eventArgs = ReferencePool.Acquire<InputEventArgs<Vector2>>();
eventArgs.Name = "MoveInput";
eventArgs.Args = value;
GameEntry.Event.Fire(this, eventArgs);
```

**新代码**：
```csharp
GameEntry.EventV2.Fire<Vector2>((int)GlobalEvents.Input.MoveInput, value);
```

### 4. 替换订阅代码

**旧代码**：
```csharp
GameEntry.Event.Subscribe(eventId, OnEvent);

void OnEvent(object sender, GameEventArgs e)
{
    var args = (InputEventArgs<Vector2>)e;
    var value = args.Args;
}
```

**新代码**：
```csharp
GameEntry.EventV2.Subscribe<Vector2>((int)GlobalEvents.Input.MoveInput, OnEvent);

void OnEvent(Vector2 value)
{
    // 直接使用参数
}
```

### 5. 移除引用

删除不再需要的：
- `using Yueyn.Base.ReferencePool;`
- `using GenBall.Event.Generated;`（如果只用于 EventArgs）
- EventArgs 类的定义（如果不再使用）

### 6. 测试验证

- 确保事件正常触发
- 确保订阅者正常接收
- 确保没有内存泄漏（取消订阅）

---

## 迁移优先级

### 高优先级（新功能）

新开发的功能直接使用新系统，不需要迁移。

### 中优先级（频繁修改）

正在活跃开发的模块，建议迁移：
- 输入系统
- UI 交互
- 玩家状态

### 低优先级（稳定代码）

稳定运行的旧代码，可以延后迁移：
- 敌人 AI
- 武器效果
- 地图系统

---

## 注意事项

1. **不要一次性迁移所有文件**：按模块逐个迁移，便于测试和回滚
2. **保持代码可编译**：每次提交都应该能正常编译运行
3. **更新文档**：迁移完成后更新此清单
4. **删除旧代码**：只有在所有使用点都迁移完成后才删除 EventArgs 类

---

## 完成标准

当以下条件全部满足时，可以删除旧系统：

- [ ] 所有 `InputEventArgs` 使用点已迁移
- [ ] 所有 `EffectEventArgs` 使用点已迁移
- [ ] 所有 `EnemyDeadEventArgs` 使用点已迁移
- [ ] 所有 `ValueChangeEventArgs` 使用点已迁移
- [ ] 所有自定义 EventArgs 类已迁移
- [ ] 代码生成器已更新（如果需要）
- [ ] 所有测试通过
- [ ] 运行时无错误

完成后可删除：
- `Assets/Scripts/Yueyn/Base/EventPool/`
- `Assets/Scripts/GenBall/Event/ValueChangeEventArgs.cs`
- `Assets/Scripts/GenBall/BattleSystem/EffectEventArgs.cs`
- `Assets/Scripts/GenBall/Enemy/EnemyDeadEventArgs.cs`
- `Assets/Scripts/GenBall/Player/Input/InputEventArgs.cs`
- `EventManager` 中的旧系统代码
