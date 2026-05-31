# 武器轮盘 UI 设计

> **创建**：2026-05-31 | **状态**：待实现（等 UI Skill 就绪）

---

## 架构

```
Form (AbilityWheelForm)        Part (WheelPart, 可复用)        Part (WheelSlotPart)
                                                      ┌─→ SlotPartLogic → SlotPartView
FormLogic ──DataList──→ WheelPartLogic ──透传data──→  ├─→ SlotPartLogic → SlotPartView
  (桥接)                  (通用轮盘逻辑)                └─→ SlotPartLogic → SlotPartView
                              │
                              ├─ cursorOffset → 计算选中Slot → 更新高亮
                              ├─ 配置 _slotPrefab (可替换的槽位预制体)
                              └─ WheelPartView: 环状背景 + 鼠标追踪 + 槽位布局
```

## 数据流

```
IAbilityWeaponSystem (武器列表/冷却)
  ↓
FormLogic (桥接层, 薄)
  → _viewData: IsVisible only
  → 调用 wheelPartLogic.SetItems(items, cursorOffset)
    - items: IReadOnlyList<object> (透传, wheelPart 不理解内容)
    - cursorOffset: Vector2 (当前鼠标相对轮盘中心的偏移)
  ↓
WheelPartLogic (通用, 可放到任何 Form)
  - 接收 items + cursorOffset
  - 根据 items.Count 创建 N 个 SlotPartLogic
  - 透传 items[i] 给每个 SlotPartLogic
  - 根据 cursorOffset 角度计算当前选中 index
  - 通知对应 SlotPartLogic 高亮
  ↓
WheelSlotPartLogic (业务相关)
  - 接收 object data → 强转为 AbilityWheelSlotData
  - 转换成 WheelSlotPartViewData → SetViewData
  ↓
WheelSlotPartView
  - 显示: 图标/名字/冷却/高亮
```

## WheelPart 通用性

`WheelPartLogic` 本身不理解 data 内容，只负责：
1. 根据 items 数量动态创建槽位 PartLogic
2. 根据 cursorOffset 计算角度→选中 index
3. 管理槽位的高亮状态

可复用场景：技能轮盘、道具快捷栏、标记轮盘、对话选项轮盘

## Form 职责

`AbilityWheelFormLogic` 只做桥接：
1. 订阅全局事件 (WheelOpened/WheelClosed)
2. 从 IAbilityWeaponSystem 读取武器列表
3. 将数据转换成 object list 传给 WheelPartLogic

## 文件清单（待创建）

| 文件 | 路径 |
|------|------|
| Form Logic | `GenBall/UI/AbilityWheelForm/AbilityWheelFormLogic.cs` |
| Form View | `GenBall/UI/AbilityWheelForm/AbilityWheelFormView.cs` |
| Form ViewData | `GenBall/UI/AbilityWheelForm/AbilityWheelFormViewData.cs` |
| WheelPart Logic | `GenBall/UI/WheelPart/WheelPartLogic.cs` |
| WheelPart View | `GenBall/UI/WheelPart/WheelPartView.cs` |
| WheelPart ViewData | `GenBall/UI/WheelPart/WheelPartViewData.cs` |
| SlotPart Logic | `GenBall/UI/WheelSlotPart/WheelSlotPartLogic.cs` |
| SlotPart View | `GenBall/UI/WheelSlotPart/WheelSlotPartView.cs` |
| SlotPart ViewData | `GenBall/UI/WheelSlotPart/WheelSlotPartViewData.cs` |

## 依赖(需先实现)

- `ITimeScaleSystem` + 桩实现
- `WheelCommand` + `WheelExecutor` (Command 管道)
- `GlobalEventId`: WheelOpened / WheelConfirmed / WheelCancelled
