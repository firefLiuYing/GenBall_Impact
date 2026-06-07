# UI 模块

## 核心设计

**Logic 驱动渲染**。Logic 层持有状态和业务逻辑，View 层只负责渲染和收集输入。

分层关系：
- `PartLogic` → `PartView`：页面子组件，绑定到特定 PartView
- `FormLogic` → `FormView`：完整页面，绑定到特定 Form
- `BusinessLogicBase`（裸 Logic）：跨表单协调器，不绑定任何 View，管理多个 FormLogic 的协作

Logic 之间支持**层级管理**：高层 Logic 可以创建和管理低层 Logic。例如局内战斗 UI 由 `InGameUIBusinessLogic` 统一管理 HUD Form、能力轮盘 Form 等多个表单。

> 注意：`GenBall.UI` 命名空间下的 `FormBase`/`VmBase`/`ItemBase` 是旧框架，已废弃不用，后续重写。

## 类层次

```
BusinessLogicBase                          ← 裸 Logic，可用于跨表单协调
├── BusinessPartLogicContainer             ← 管理子 PartLogic 列表
│   ├── BusinessFormLogic                  ← 驱动 FormView（abstract PrefabPath）
│   └── BusinessPartLogic / BusinessPartLogic<TView>  ← 驱动 PartView

UIComponent (MonoBehaviour)
├── UIBusinessFormBase / UIBusinessFormBase<TViewData>
└── PartViewBase / PartViewBase<TViewData>
```

## 生命周期

### Form 创建流程（同步，自顶向下）

```
CreateLogic<T>() → OnCreateInternal → OpenForm → InternalInit → DoStart (View)
                                      → BindForm → OnFormBound
                                      → OnFormCreated
                                      → DiscoverChildPartLogics
                                      → InternalOpen → DoOpen
                                      → InternalFocus → DoFocus
```

### Form 销毁流程

```
DestroyLogic() → OnFormDestroying → UnbindForm → OnFormUnbound
               → CloseForm → InternalUnfocus → InternalClose → DoClose → Destroy GO
               → ClearPartLogics
```

### Part 绑定

- **静态**：PartViewBase 已存在于 Form 预制体子节点 → `DiscoverChildPartLogics` 自动发现 → 反射找 `BusinessPartLogic<TView>` → 自动创建并绑定
- **动态**：Logic 手动 `CreateLogic<TPart>(p => p.ParentTransform = ...)` → `LoadAndBindPart` → 加载预制体实例化 → 生命周期补偿 → 绑定

两种方式都是正式用法。静态适合固定布局，动态适合条件性/可变的子组件。

Dynamic Part 需要通过 `AddPartLogic()`/`RemovePartLogic()` 注册到父 Logic，或直接通过父 Logic 管理其生命周期。

## 通信模式

| 方向 | 方式 | 说明 |
|------|------|------|
| View → Logic | `UIManager.Instance.UIEventRouter.FireNow(UIEventKey)` | 按钮点击等用户交互 |
| View → Logic | 直接 C# `event Action<T>` | View 暴露事件，Logic 在 OnFormBound 订阅 |
| Logic → View | `View.SetViewData(data)` → `RefreshView()` | 全量刷新（主要模式） |
| Logic → View | 直接调用 View 特定方法 | 频繁增量更新 |
| Logic ↔ 全局 | `CEventRouter.Instance.Subscribe<T>(eventId)` | 监听/触发游戏事件 |

所有订阅在 `OnFormBound`/`OnViewBound` 中建立，在 `OnFormUnbound`/`OnViewUnbound` 中取消。

## UiViewBinding 代码生成

1. 在 prefab 根节点挂 `UiViewBinding` 组件
2. 设置 ViewType（Form/Part）、FormType（Persistent/Popup）
3. Inspector 中 Scan → Generate
4. 生成代码写入 `// ### GENERATED_BINDINGS_START/END ###` 标记之间
5. 标记外的代码不会被覆盖
