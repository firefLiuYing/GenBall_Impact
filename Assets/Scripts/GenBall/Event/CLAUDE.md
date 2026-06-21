# Event 模块

## EventAdapter — 事件总线适配层

`EventAdapter` 是可序列化的事件配置单元，封装"事件 ID + 参数"，让任何 MonoBehaviour 都能在 Inspector 中配置触发事件。

### 在组件中使用

在任何 MonoBehaviour 中声明字段即可获得下拉选择 + 搜索 + 参数配置：

```csharp
[SerializeField] private EventAdapter _onInteract;
// Inspector 自动出现：Event 下拉选择框、Parameters 折叠区
```

如需多个事件（类似 UnityEvent）：

```csharp
[SerializeField] private List<EventAdapter> _interactEvents = new();
// Inspector 显示可增减的列表，每项都有下拉选择
```

触发事件：

```csharp
_onInteract?.Fire();           // 触发单个
foreach (var e in _events) e?.Fire();  // 触发全部
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
  → parameters?.Dispatch(eventId)
    → CEventRouter.Instance.FireNow<T>(eventId, parameters)
  → CEventRouter.Instance.FireNow(eventId)  // 无参数时

订阅方：CEventRouter.Instance.Subscribe<T>(eventId, handler)
```

### PropertyDrawer

`EventAdapterDrawer`（Editor 目录下）是全局 PropertyDrawer——所有 `[SerializeField] EventAdapter` 字段自动拥有事件下拉选择 + 搜索框 + 参数管理。无需为每个组件写 Custom Editor。
