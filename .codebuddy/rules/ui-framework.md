---
description: GenBall_Impact 项目中 UI 开发的强制规范，包括框架选择、文本组件、命名约定等。
alwaysApply: false
enabled: true
updatedAt: 2026-05-31T12:00:00.000Z
provider: ""
---

# UI 框架开发规范

## 目标
确保所有新 UI 使用新框架（Yueyn.UI MVP 架构），避免使用已废弃的 TextMeshPro 组件和旧 MVVM 框架。

## 触发条件
- 文件路径匹配 `**/UI/**/*.cs` 或 `**/GenBall/UI/**/*.cs`
- 用户提到"UI"、"界面"、"页面"、"弹窗"
- 创建或修改 UI 相关代码

## 框架类层次

### Logic 层
```
BusinessLogicBase
└── BusinessPartLogicContainer   (子 Part 管理)
    ├── BusinessFormLogic         (页面 Logic)
    └── BusinessPartLogic         (子组件 Logic)
        └── BusinessPartLogic<TView>  (强类型)
```

### View 层
```
UIComponent (MonoBehaviour)
├── UIBusinessFormBase<T>         (页面 View, 数据驱动)
└── PartViewBase<T>               (子组件 View, 数据驱动)
```

### Unity 层
```
UIManager (Singleton)             (页面管理, UICamera, EventSystem)
UIFormScript (MonoBehaviour)      (挂 prefab 根, Canvas, 生命周期分发)
```

## 行为约束

### 1. 新 UI 必须使用 Yueyn.UI 框架（必须）
- **必须** Logic 层继承 `BusinessFormLogic` (Form) 或 `BusinessPartLogic<TView>` (Part)
- **必须** View 层继承 `UIBusinessFormBase<T>` (Form) 或 `PartViewBase<T>` (Part)
- **禁止** 使用旧 `FormBase` / `VmBase` (MVVM 架构)

### 2. 禁用 TextMeshPro（必须）
- **必须** 所有 UI 文本组件使用 `UnityEngine.UI.Text`
- **禁止** 使用 `TMP_Text` / `TextMeshProUGUI` / `TMPro` 相关类型

### 3. 命名约定（必须）
- **必须** Logic: `{Name}Logic.cs` → class `{Name}Logic`
- **必须** View: `{Name}View.cs` → class `{Name}View`
- **必须** ViewData: `{Name}ViewData.cs` → class `{Name}ViewData`
- **必须** 控件按前缀命名: `Btn*`/`Txt*`/`Img*`/`RawImg*`/`Input*`/`Slider*`/`Toggle*`/`Scroll*`/`Dropdown*`/`Scrollbar*`/`CanvasGroup*`/`LayoutElem*`/`Fitter*`/`HLayout*`/`VLayout*`/`Grid*`/`Rect*`

### 4. 代码生成（必须）
- **必须** 使用 `UiViewBinding` 组件进行 Scan → Generate
- **必须** 手写代码放在 `### GENERATED_BINDINGS_START ###` / `END` 标记之外
- **禁止** 手动写 BindControls / 控件属性声明

### 5. 页面类型（建议）
- **建议** 常驻 UI（HUD）使用 `UIFormType.Persistent`
- **建议** 弹窗使用 `UIFormType.Popup`
- **建议** 过场界面使用 `UIFormType.Transition`

### 6. ViewData 策略（必须）
- **必须** Logic 中统一管理一个 ViewData 实例
- **建议** 大部分不变的数据在 OnFormBound 时全量刷新
- **建议** 频繁变化的个别数据在 View 提供专用刷新方法

## 通信模式

| 方向 | 方式 |
|------|------|
| View → Logic | `UIManager.Instance.UIEventRouter.FireNow(UIEventKey)` |
| Logic → View | `View.SetViewData(viewData)` 全量 / `View.SpecificMethod()` 局部 |
| 全局 → Logic | `CEventRouter.Instance.Subscribe<GlobalEventId>()` |
| Logic → 全局 | `SystemRepository.Instance.GetSystem<T>()` |
| 打开 Form | `BusinessLogicManager.Instance.CreateLogic<T>()` |

## 示例

### 正确示例：Form View
```csharp
public class ShopFormView : UIBusinessFormBase<ShopFormViewData>
{
    // ### GENERATED_BINDINGS_START ###
    // (控件属性和 BindControls 由代码生成器管理)
    // ### GENERATED_BINDINGS_END ###

    protected override void DoBusinessStart()
    {
        base.DoBusinessStart();
        BindControls(); // 调用生成的绑定方法
        BtnConfirm.onClick.AddListener(() =>
            UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.ShopForm_Confirm));
    }

    protected override void RefreshView()
    {
        if (ViewData == null) return;
        TxtTitle.text = ViewData.Title;
    }
}
```

### 正确示例：Form Logic
```csharp
public class ShopFormLogic : BusinessFormLogic
{
    // ### GENERATED_BINDINGS_START ###
    // (PrefabPath, FormType, View 由代码生成器管理)
    // ### GENERATED_BINDINGS_END ###

    private readonly ShopFormViewData _viewData = new();

    protected override void OnFormBound(UIFormScript form)
    {
        base.OnFormBound(form);
        View = form.GetComponentInChildren<ShopFormView>();
        UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.ShopForm_Confirm, OnConfirm);
        RefreshAll();
    }

    protected override void OnFormUnbound(UIFormScript form)
    {
        UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.ShopForm_Confirm, OnConfirm);
        View = null;
        base.OnFormUnbound(form);
    }

    public static ShopFormLogic Open()
    {
        return BusinessLogicManager.Instance.CreateLogic<ShopFormLogic>();
    }

    private void OnConfirm() { /* 业务逻辑 */ }
    private void RefreshAll() { View?.SetViewData(_viewData); }
}
```

### 错误示例
```csharp
// 错误 1：使用旧框架
public class ShopForm : FormBase { }
public class ShopFormVm : VmBase { }

// 错误 2：使用 TextMeshPro
using TMPro;
[SerializeField] private TMP_Text titleText;

// 错误 3：自己写绑定代码
private void BindControls() { BtnConfirm = transform.Find("...").GetComponent<Button>(); }
// ↑ 这应该由代码生成器生成

// 错误 4：每次刷新都 new ViewData
private void OnDataChanged() { View.SetViewData(new ShopFormViewData { ... }); }
```

## 例外情况
- 旧 UI 代码维护时可继续使用旧框架，但不得新建
- 非 UI 文本（如 3D 世界空间文本）可根据需求选择组件
