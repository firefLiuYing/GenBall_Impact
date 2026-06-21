# Interact 模块

## 概述

交互系统负责"视线内显示交互按钮 → 按键触发"的完整流程。核心是 `IInteractSystem`（ISystem），每帧锥形视线检测发现附近的 `IInteractable`，玩家按交互键（F）触发。

与触发器系统的关系：交互系统 = 视线检测 + 手动按键；触发器 = 碰撞自动触发。互不干扰。

## 核心接口

### IInteractable

所有可交互物实现此接口：

```csharp
public interface IInteractable
{
    string OperationDescription { get; }  // UI 按钮文字
    bool CanInteract { get; }             // false 时不出现在候选列表
    void Interact();                      // 执行交互逻辑
    void OnFocused();                     // 被选中时（预留高亮）
    void OnUnfocused();                   // 取消选中
}
```

### IInteractSystem

全局服务，通过 `SystemRepository.Instance.GetSystem<IInteractSystem>()` 访问：

| 成员 | 说明 |
|------|------|
| `Interactables` | `Variable<List<IInteractable>>`，可观察列表，绑定 UI |
| `CurrentSelectionIndex` | `Variable<int>`，当前选中下标 |
| `NextSelection()` / `LastSelection()` | 轮换选中（滚轮/按键） |
| `TriggerInteractable()` | 触发当前选中的交互 |
| `Configure(coneHalfAngle, maxDistance, layer)` | 设置锥形检测参数（PlayerEntityFactory 调用） |

## 视线检测

InteractSystem 实现 `IFrameUpdate`，每帧从 `ICameraSystem.MainCamera` 发出锥形检测：

- **形状**：OverlapSphere(最大距离) + FOV 点积过滤 → 锥形
- **排序**：距离最近优先
- **粘性选中**：之前选中的未脱离视野则保持选中
- **过滤**：`CanInteract == false` 的不出现（如战斗中篝火不可交互）

配置在 `PlayerConfig`（ScriptableObject）：`coneHalfAngle`（锥角半角，度）、`maxInteractDistance`（最大距离）。

## 输入链路

```
F 键 → InputHandler → PlayerInputAdapter → PlayerDecisionLayer
  → InteractCommand(Trigger) → CommandDispatcher → PlayerInteractExecutor
  → IInteractSystem.TriggerInteractable() → IInteractable.Interact()
```

`PlayerInteractExecutor` 只做命令路由。滚动切换用滚轮（Next/Previous action）。

## 实现 IInteractable

### 篝火示例 (SavePoint)

```csharp
public class SavePoint : MonoBehaviour, IInteractable
{
    [SerializeField] private List<EventAdapter> _interactEvents = new();
    [SerializeField] private string _displayName;

    public void SetConfig(string displayName) { _displayName = displayName; }
    public string OperationDescription => _displayName;
    public bool CanInteract => !_combatStateSystem.IsInCombat; // 脱战才可交互
    public void Interact() { foreach (var e in _interactEvents) e?.Fire(); }
    public void OnFocused() => Debug.Log($"Focused: {_displayName}");
    public void OnUnfocused() => Debug.Log($"Unfocused: {_displayName}");
}
```

`SetConfig` 由 `SceneExecutorSystemDefault.SpawnBonfires()` 在实例化时注入。

### 通用事件代理 (EventAdapter)

任何 MonoBehaviour 可直接挂 `EventAdapter` 字段实现事件驱动的交互——Inspector 自动提供事件下拉选择。详见 `Event/CLAUDE.md`。

## UI

交互提示使用 Part 模式，挂载在 MainHudForm 下：

- **InteractTipPartLogic / InteractTipPartView** — 容器，监听 `IInteractSystem.Interactables`，列表空时隐藏
- **InteractTipSlotLogic / InteractTipSlotView** — 单条提示，粗体表示选中
