# Event 模块

## EventAdapter — 事件总线适配层

`EventAdapter` 是可序列化的事件容器，内部持有 `List<EventEntry>`（类似 UnityEvent 的 persistent calls），让任何 MonoBehaviour 都能在 Inspector 中配置触发事件。

### 在组件中使用

在任何 MonoBehaviour 中声明单个字段即可获得紧凑的行内事件列表：

```csharp
[SerializeField] private EventAdapter _onInteract;
// Inspector 自动出现：每行 [Event 下拉] [参数] [-按钮]，底部 [+ Add Event] 按钮
```

无需 `List<EventAdapter>` 即可支持多事件——`EventAdapter` 本身就是容器。

触发事件：

```csharp
_onInteract?.Fire();  // 遍历所有 entry 依次触发
```

### 事件 ID 体系

| 范围 | 类别 |
|------|------|
| 1-99 | Launch |
| 1000-1999 | Player |
| 2000-2999 | Input |
| 3000-3999 | Weapon |
| 4000-4999 | Enemy |
| 5000-5999 | System |
| 6000+ | Placed（放置事件，CSV 导入） |

### 参数类型

| 参数类 | 事件 ID | 用途 |
|--------|---------|------|
| `SpawnEnemyParams` | 6001 | 生成敌人 |
| `OpenDoorParams` | 6002 | 开门 |
| `PlayDialogueParams` | 6003 | 播放对话 |
| `GrantAccessoryParams` | 6004 | 授予配件 |
| `UnlockSavePointParams` | 6005 | 解锁存档点 |

添加新参数类型：继承 `EventParameterBase`，实现 `Dispatch(int eventId)`，用 `[EventParamHint(eventId)]` 关联事件。

### 分发原理

```
EventAdapter.Fire()
  → 遍历 _entries:
    → entry.parameters?.Dispatch(entry.eventId)
      → CEventRouter.Instance.FireNow<T>(eventId, parameters)
    → CEventRouter.Instance.FireNow(eventId)  // 无参数时

订阅方：CEventRouter.Instance.Subscribe<T>(eventId, handler)
```

### PropertyDrawer

`EventAdapterDrawer`（Editor 目录下）是全局 PropertyDrawer——所有 `[SerializeField] EventAdapter` 字段自动获得紧凑行内事件列表（类似 UnityEvent），每行 [Event下拉] [参数] [-]，底部 [+ Add Event]。无需为每个组件写 Custom Editor。

### 向后兼容

旧版单事件序列化数据（`eventId` + `parameters` 标量字段）在首次访问时自动迁移到 `_entries[0]`。`List<EventAdapter>` 用法仍然有效。
