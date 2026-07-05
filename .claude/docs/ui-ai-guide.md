# UI 代码知识库 — AI 开发指南

> **目标读者**: AI（Claude Code）。本文档提供 UI 开发中所有设计决策的**确定性答案**——不是选项列表，而是决策结果。
>
> **何时读**: 需要创建新的 Form/Part、设计 ViewData、选择通信方式、或写业务逻辑时。
>
> **所有结论均已经过人工审查确认。** 不要用现有代码反推规范——现有代码本身可能是 AI 生成未经审查的。

---

## 0. 架构定位：MVP 三层

理解 UI 代码在整体架构中的位置，是正确决策的前提。

```
┌─────────────────────────────────────────────┐
│ M 层 — System（被动工具集 / 数据库）          │
│                                             │
│ · 实现 ISystem，注册到 SystemRepository      │
│ · 定位：被调用、被刷新、响应查询               │
│ · 不主动触发任何行为                          │
│ · 例：ISaveService, IAbilityWeaponSystem     │
│   IInteractSystem, IConfigProvider           │
├─────────────────────────────────────────────┤
│ P 层 — Logic（编排 / 协调）                   │
│                                             │
│ · BusinessLogicBase — 业务编排器              │
│   ├─ 决策何时打开/关闭哪些 Form               │
│   ├─ 承载生命周期管理（它关闭 → 下属 Form     │
│   │   不会被误打开）                          │
│   └─ 按业务功能组织多个 Form                  │
│                                             │
│ · BusinessFormLogic — 单页面交互逻辑          │
│   └─ 只管自己页面的交互，不跨 Form 编排       │
│                                             │
│ · BusinessPartLogic — 单子组件交互逻辑         │
├─────────────────────────────────────────────┤
│ V 层 — View（纯渲染 / 输入收集）               │
│                                             │
│ · UIBusinessFormBase<T> / PartViewBase<T>   │
│ · 只负责：显示数据 + 收集按钮点击              │
│ · 不持有业务状态，不做业务判断                 │
└─────────────────────────────────────────────┘
```

### 数据流方向

```
System (M) ──CEventRouter──→  Logic (P) ──SetViewData──→  View (V)
    ↑                            │                           │
    └──── SystemRepository ──────┘          UIEventRouter ───┘
         (Logic 调用 System)              (View 按钮 → Logic)
```

- System **不主动**触发任何事。它是被调用的工具。
- Logic 是**唯一的主动层**：订阅事件、决策、调用 System、驱动 View。
- View 是**纯被动层**：接收数据渲染，收集输入上报。

### BusinessLogicBase 的核心价值

> **生命周期管理**。如果某个 BusinessLogic 被关闭，那么由它管理的页面就从根本上不可能被误打开。

这就是它存在的主要理由——不是功能复用，而是**安全地组织和管理一组 Form 的生命周期**。当一个业务场景结束（如启动流程完毕），销毁对应的 BusinessLogic 就等价于"这个场景的所有 UI 都已安全关闭"。

---

## 1. 快速决策树

### Form vs Part

```
该 UI 是否有独立生命周期（不依附于某界面、可被单独创建和销毁）？
├─ 是 → Form
└─ 否 → 必须依附于父界面存在 → Part
```

**Form 的特征**：
- 可被单独打开/关闭，不依赖其他界面存在
- 挂在 UIManager 的 Persistent/Popup/Transition 层
- 有自己的 Canvas + UIFormScript

**Part 的特征**：
- 必须依附于父 Form/Part，随父界面一起生命周期
- 以下任一情况应拆为 Part：
  - **复用**：跨多个页面使用同一个子组件
  - **多实例**：同一界面内动态生成多个
  - **拆分**：Form 太庞大，拆 Part 简化逻辑（类似 C# partial class 的 UI 等价）
  - **布局适配**：方便分辨率变化时独立调整布局

### FormType 选择（仅 Form）

| FormType | 渲染层 | 行为 | 示例 |
|----------|--------|------|------|
| `Persistent` | Persistent 层 | 始终显示、不可关闭、不参与排序 | HUD、血条 |
| `Popup` | Popup 层 | 弹窗、可堆叠、自动 sortingOrder、有淡入淡出 | 商店、存档、轮盘 |
| `Transition` | Transition 层（独立） | 独占层、阻塞所有其他 UI | Loading 画面、过场动画 |
| `WorldSpace` | 3D 世界空间 | Canvas RenderMode.WorldSpace | 存档点全息 UI |

> **Transition 是独立渲染层**，不是 Popup 的别名。LoadingForm 用了 `Popup` 是**不正确**的，应该用 `Transition`。

### 何时用 BusinessLogicBase（业务编排器）

当需要**按业务场景组织和管理一组 Form** 时：

- 多个 Form 共同服务于一个业务场景（如"启动流程"包含 LoadingForm + StartForm）
- 需要安全的生命周期管理：编排器关闭 = 下属所有 Form 不会被误打开
- 承载"何时打开/关闭哪个 Form"的业务决策逻辑

**与 FormLogic 的区别**：
- BusinessLogicBase = 只管"什么时候开哪个页面"（编排）
- FormLogic = 只管"这一个页面内的交互"（页面逻辑）

示例：`LoadingBusinessLogic` 编排 Loading→StartForm 流程；`InGameUIBusinessLogic` 管理局内 HUD+轮盘的打开时机。

> BusinessLogicBase 之间的关系（平级 vs 父子、所有权）尚未最终确定，当前大部分场景由裸 Logic 管理 FormLogic。

---

## 2. ViewData 设计规则

### 规则 1: 使用统一实例

```csharp
// ✅ 正确 — Logic 中维护一个 _viewData 实例，事件处理器修改字段后刷新
private readonly MyFormViewData _viewData = new();

private void OnHealthChanged(int health)
{
    _viewData.Health = health;
    View?.SetViewData(_viewData);
}
```

### 规则 2: 例外——复杂动态列表

当 ViewData 承载完整列表且内容不可增量更新时：

```csharp
// ✅ 可接受 — 一次性构建新 ViewData
View.SetViewData(new SaveSlotFormViewData { Slots = slotInfos });
```

### 规则 3: 字段声明

- 普通数据 → `public field`（`public int Health;`）
- 简单 bool 标记 → `public bool CanContinue { get; set; }`（可接受）
- 嵌套数据类 → 放在 ViewData.cs 同文件（如 `SaveSlotItemInfo`）

---

## 3. 通信方式选择

### 核心原则

| 通信方向 | 机制 | 适用场景 |
|----------|------|---------|
| **UI 内部**（View 按钮 → Logic） | `UIEventRouter` | 同一 Form 内的按钮点击、UI 事件 |
| **跨系统**（游戏状态 → UI Logic） | `CEventRouter` (GlobalEventId) | HP 变化、关卡事件等游戏系统通知 |
| **Logic → View**（数据刷新） | `SetViewData()` / View 专用方法 | 数据驱动渲染 |

### UIEventRouter（UI 内部通信标准）

```csharp
// View 端 — BindEvents() 中绑定
BtnConfirm.onClick.AddListener(() =>
    UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.MyForm_Confirm));

// Logic 端 — OnFormBound 中订阅
UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.MyForm_Confirm, OnConfirm);
```

### CEventRouter（跨系统通信标准）

```csharp
// 游戏系统 → UI
CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.HealthChanged, OnHealthChanged);
```

### C# event — 不规范，避免使用

`SaveSlotForm.SlotClicked` 使用的 C# `event Action<int>` 模式**不规范**。正确做法是用 UIEventRouter 传参或 CEventRouter 跨系统通信。新代码不要模仿。

---

## 4. 生命周期与刷新策略

### Logic 生命周期标准流程

```
OnFormCreated     → 获取 View 引用 + 初始化数据 + 首次刷新
OnFormBound       → 订阅所有事件（UIEventRouter + CEventRouter）
OnFormUnbound     → 取消所有事件订阅 + View = null
OnFormDestroying  → 兜底取消订阅（双重保险，防止异常路径 OnFormUnbound 未调用）
```

### 事件订阅铁律

```csharp
protected override void OnFormBound(UIFormScript form)
{
    // 订阅 UI 事件
    UIManager.Instance.UIEventRouter.Subscribe(...);
    // 订阅全局事件
    CEventRouter.Instance.Subscribe(...);
}

protected override void OnFormUnbound(UIFormScript form)
{
    // 取消所有事件（与 OnFormBound 镜像顺序）
    UIManager.Instance.UIEventRouter.Unsubscribe(...);
    CEventRouter.Instance.Unsubscribe(...);
    View = null;
    base.OnFormUnbound(form);
}

protected override void OnFormDestroying()
{
    // 兜底取消（防止异常路径）
    UIManager.Instance.UIEventRouter.Unsubscribe(...);
    CEventRouter.Instance.Unsubscribe(...);
    base.OnFormDestroying();
}
```

> LoadingForm 在 `OnFormDestroying` 中取消订阅**不是错误**——这是双重保险模式，更安全。推荐同时保留 OnFormUnbound 和 OnFormDestroying 的取消逻辑。

### 刷新策略

**默认：全量刷新**（大部分数据初始化时）

```csharp
// OnFormCreated 中首次刷新
protected override void OnFormCreated()
{
    base.OnFormCreated();
    View = form.GetComponentInChildren<MyFormView>();
    _viewData.Title = "My Form";
    View.SetViewData(_viewData);
}
```

**优化：专用增量刷新**（频繁变化的控件）

```csharp
// View — 频繁更新的控件单独处理，不触发完整 RefreshView
public void RefreshHealth(int health)
{
    TxtHealth.text = $"HP: {health}";
}
```

**守卫模式**（防止解绑后回调）

```csharp
private bool _isViewReady;

protected override void OnFormBound(UIFormScript form) { /* 初始化... */ _isViewReady = true; }
protected override void OnFormUnbound(UIFormScript form) { _isViewReady = false; /* ... */ }

private void OnSomeEvent()
{
    if (!_isViewReady) return;  // 防止解绑后的回调
}
```

---

## 5. 标准模板

> 标注 `// ### GENERATED_BINDINGS ###` 的区域由代码生成器管理。模板反映了**经过审查的正确模式**。

### 5A. Form Logic 模板

```csharp
using Yueyn.UI;
using Yueyn.Event;

namespace GenBall.UI
{
    public class MyFormLogic : BusinessFormLogic
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath => "Assets/AssetBundles/UI/MyForm/MyForm.prefab";
        public override UIFormType FormType => UIFormType.Popup;
        public MyFormView View { get; private set; }
        // ### GENERATED_BINDINGS_END ###

        private readonly MyFormViewData _viewData = new();

        protected override void OnFormCreated()
        {
            base.OnFormCreated();
            View = BoundForm.GetComponentInChildren<MyFormView>();
            _viewData.Title = "My Form";
            View?.SetViewData(_viewData);
        }

        protected override void OnFormBound(UIFormScript form)
        {
            base.OnFormBound(form);
            UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.MyForm_Confirm, OnConfirm);
            UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.MyForm_CloseRequest, OnCloseRequest);
            CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.SomeEvent, OnSomeEvent);
        }

        protected override void OnFormUnbound(UIFormScript form)
        {
            UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.MyForm_Confirm, OnConfirm);
            UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.MyForm_CloseRequest, OnCloseRequest);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.SomeEvent, OnSomeEvent);
            View = null;
            base.OnFormUnbound(form);
        }

        protected override void OnFormDestroying()
        {
            // 兜底取消订阅
            UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.MyForm_Confirm, OnConfirm);
            UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.MyForm_CloseRequest, OnCloseRequest);
            CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.SomeEvent, OnSomeEvent);
            base.OnFormDestroying();
        }

        public static MyFormLogic Open()
        {
            return BusinessLogicManager.Instance.CreateLogic<MyFormLogic>();
        }

        private void OnConfirm() { /* ... */ }
        private void OnCloseRequest() { CloseForm(); }
        private void OnSomeEvent(int value) { _viewData.SomeValue = value; View?.SetViewData(_viewData); }
    }
}
```

### 5B. Form View 模板

```csharp
using GenBall.Utils.CodeGenerator.UI;
using UnityEngine.UI;
using Yueyn.UI;

namespace GenBall.UI
{
    public class MyFormView : UIBusinessFormBase<MyFormViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;
        public Text TxtTitle { get; private set; }
        public Button BtnConfirm { get; private set; }
        public Button BtnClose { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            TxtTitle   = _binding.GetBinding<Text>("TxtTitle");
            BtnConfirm = _binding.GetBinding<Button>("BtnConfirm");
            BtnClose   = _binding.GetBinding<Button>("BtnClose");
        }
        // ### GENERATED_BINDINGS_END ###

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
            BindEvents();
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;
            TxtTitle.text = ViewData.Title;
            BtnConfirm.interactable = ViewData.CanConfirm;
        }

        private void BindEvents()
        {
            BtnConfirm?.onClick.AddListener(() =>
                Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.MyForm_Confirm));
            BtnClose?.onClick.AddListener(() =>
                Yueyn.UI.UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.MyForm_CloseRequest));
        }
    }
}
```

### 5C. Part Logic 模板

```csharp
using Yueyn.UI;

namespace GenBall.UI
{
    public class MyPartLogic : BusinessPartLogic<MyPartView>
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath => "Assets/AssetBundles/UI/MyPart/MyPart.prefab";
        // ### GENERATED_BINDINGS_END ###

        private readonly MyPartViewData _viewData = new();

        protected override void OnViewBound(PartViewBase view)
        {
            base.OnViewBound(view);
            BoundView.SetViewData(_viewData);
            // 订阅事件或系统 Observable
        }

        protected override void OnViewUnbound(PartViewBase view)
        {
            // 取消订阅
            base.OnViewUnbound(view);
        }

        public static MyPartLogic Create(MyPartView partView)
        {
            return BusinessLogicManager.Instance.CreateLogic<MyPartLogic>(
                p => p.ParentTransform = partView.transform);
        }
    }
}
```

### 5D. BusinessLogicBase 模板（业务编排器）

```csharp
using Yueyn.Event;
using Yueyn.UI;

namespace GenBall.UI
{
    /// <summary>
    /// {Module} 业务编排器 — 管理 {FormA}Form 和 {FormB}Form 的生命周期。
    /// 此 Logic 关闭 → 下属 Form 不会被误打开。
    /// </summary>
    public class MyModuleBusinessLogic : BusinessLogicBase
    {
        private MyFormALogic _formA;
        private bool _isActive;

        protected override void OnCreateInternal()
        {
            // 订阅触发业务流程的全局事件
            CEventRouter.Instance.Subscribe((int)GlobalEventId.SomeTrigger, OnTrigger);
        }

        protected override void OnDestroyInternal()
        {
            _isActive = false;
            CEventRouter.Instance.Unsubscribe((int)GlobalEventId.SomeTrigger, OnTrigger);

            // 编排器销毁 → 确保下属 Form 全部关闭
            CloseAllForms();
            base.OnDestroyInternal();
        }

        private void OnTrigger()
        {
            _isActive = true;
            _formA = MyFormALogic.Open();
            // 业务决策：FormA 关闭时打开 FormB
            // ...（可通过回调或事件实现）
        }

        private void CloseAllForms()
        {
            if (_formA != null)
            {
                _formA.OnDestroy();
                _formA = null;
            }
        }
    }
}
```

> BusinessLogicBase 之间的关系（平级 vs 父子/所有权）**尚未最终确定**。当前可参考 InGameUIBusinessLogic 和 LoadingBusinessLogic 的模式。

### 5E. ViewData 模板

```csharp
namespace GenBall.UI
{
    public class MyFormViewData
    {
        public string Title;
        public int Gold;
        public bool CanConfirm;

        // 嵌套数据类（如需要）
        public System.Collections.Generic.List<MyItemInfo> Items = new();
    }

    public class MyItemInfo
    {
        public int Id;
        public string Name;
    }
}
```

---

## 6. UIEventKey 注册规范

### 格式

```
{FormName}_{Action}   // PascalCase，下划线分隔
```

### ID 分配

每个 Form 预留 5-10 个连续 ID。在 `Assets/Scripts/GenBall/UI/UIEventKey.cs` 中添加：

```csharp
// ===== MyForm 事件 =====
MyForm_Confirm = 100,
MyForm_Cancel = 101,
MyForm_CloseRequest = 102,
```

### 现有分配（避免冲突）

| Form | ID 范围 |
|------|---------|
| LoadingForm | 1-4 |
| StartForm | 10-13 |
| GMConsole | 20-22 |
| SaveSlotForm | 30 |
| (下一个可用) | **40+** |

---

## 7. 代码审计参考

> **注意**：以下代码大多由 AI 生成，未经完整人工审查。这里标注的是它们在多大程度上符合上述经过确认的规范。

### 符合规范的部分

| 实现 | 符合点 | 不符合点 |
|------|--------|---------|
| **MainHudForm** | `_viewData` 统一管理、CEventRouter 订阅/取消对称 | View 引用在 OnFormBound 获取（应该在 OnFormCreated） |
| **StartForm** | UIEventRouter 按钮绑定、Form 间跳转 | View 引用在 OnFormBound 获取；缺少 OnFormDestroying 兜底 |
| **InteractTipPart** | Observable 响应式模式、动态子 Part 管理 | — |

### 需要修正的模式（不要模仿）

| 实现 | 问题 | 正确做法 |
|------|------|---------|
| **SaveSlotForm** | 用 C# `event Action<int>` 通信 | 改为 UIEventRouter 或 CEventRouter |
| **SaveSlotForm** | 静态 `_pendingCallback` 传参 | 通过 `Open(Action)` 传递，或改用事件机制 |
| **LoadingForm** | FormType 用了 `Popup` | 应该用 `Transition`（独立渲染层） |
| **LoadingForm** | View 引用在 OnFormBound 获取 | 应该在 OnFormCreated |
| **MainHudForm** | View 引用在 OnFormBound 获取 | 应该在 OnFormCreated |
| **StartForm** | View 引用在 OnFormBound 获取 | 应该在 OnFormCreated |

### Part 创建方式参考

| 方式 | 何时用 | 参考 |
|------|--------|------|
| **静态绑定** | prefab 中固定存在 | InteractTipPart（MainHudForm 自动发现） |
| **动态创建** | 运行时条件决定数量/存在 | WheelPart（AbilityWheelForm 手动创建） |

---

## 8. 代码生成器规范

代码生成器（`unity_generate_ui_code`）生成的模板应包含：

- Logic 的 `OnFormUnbound` 中必须生成 `View = null;`
- Logic 的 `OnFormCreated` 中生成 `View = form.GetComponentInChildren<TView>();`（当前生成器将此放在 `OnFormBound`，需要修正）
- Logic 的 `OnFormDestroying` 中应预留兜底取消订阅的注释或空实现

当前生成器未写 `View = null` 是**遗漏**，不是设计意图。

---

## 9. 文件组织规范

> 现有代码的文件位置不规范（"之前偷懒了"）。以下为标准组织方式。

### 核心原则：按 BusinessLogic 划分模块

```
Assets/Scripts/GenBall/UI/
├── UIEventKey.cs                     ← UI 事件枚举（所有 Form 共用）
│
├── Startup/                          ← "启动流程" BusinessLogic 模块
│   ├── LoadingBusinessLogic.cs       ← 启动编排器
│   ├── LoadingForm/
│   │   ├── LoadingFormView.cs
│   │   ├── LoadingFormLogic.cs
│   │   └── LoadingFormViewData.cs
│   └── StartForm/
│       ├── StartFormView.cs
│       ├── StartFormLogic.cs
│       └── StartFormViewData.cs
│
├── InGame/                           ← "局内" BusinessLogic 模块
│   ├── InGameUIBusinessLogic.cs      ← 局内 UI 编排器
│   ├── MainHudForm/
│   │   └── ...
│   └── AbilityWheelForm/
│       └── ...
│
└── Save/                             ← "存档" BusinessLogic 模块
    ├── SaveSlotBusinessLogic.cs      ← 存档流程编排器（如果需要）
    ├── SaveSlotForm/
    │   └── ...
    └── ...
```

### 规则

1. **一个 BusinessLogic 模块 = UI 根目录下的一个子目录**
2. **模块目录直接放 BusinessLogicBase（编排器）**，再加子目录放 Form
3. **只服务一个 Form 的简单模块**：如果只有单个 Form 且没有编排器，可以直接 Form 子目录放在 UI 根目录
4. **跨模块复用的 Part**：放在 `UI/Shared/` 或 `UI/Parts/`（待定）
5. **Prefab 路径**保持 `Assets/AssetBundles/UI/{FormName}/{FormName}.prefab`

### 命名

| 类型 | 命名 | 示例 |
|------|------|------|
| 业务编排器 | `{Module}BusinessLogic` | `InGameUIBusinessLogic`, `LoadingBusinessLogic` |
| Form Logic | `{FormName}Logic` | `StartFormLogic`, `MainHudFormLogic` |
| Form View | `{FormName}View` | `StartFormView`, `MainHudFormView` |
| Part Logic | `{PartName}Logic` | `WheelPartLogic`, `InteractTipSlotLogic` |
| Part View | `{PartName}View` | `WheelPartView`, `InteractTipSlotView` |

---

## 关联文档

- `ui-architecture.md` — 完整架构参考（生命周期表、类层次）
- `ui-layout-guide.md` — 布局/字体/颜色/间距规范
- `../skills/create-ui/SKILL.md` — /create-ui 工作流
- `code-patterns.md` — 非 UI 代码模式（Buff/Damage/Weapon）
