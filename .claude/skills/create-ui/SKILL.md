---
name: create-ui
description: "Create a new UI Form or Part with the full workflow — prefab, code generation, and business logic."
---

# /create-ui — UI Form / Part 完整创建工作流

Use this skill when the user asks to create a new UI Form or Part. Do NOT write any binding code before the code generator has been run.

## 核心原则

1. **绝不跳步** — 手动步骤必须在用户完成后才能继续，不要预写代码
2. **生成器先行** — 控件绑定、PrefabPath、View 属性由 `UiViewBinding` 扫描生成
3. **只写标记外** — 手写代码只能放在 `// ### GENERATED_BINDINGS_START ###` / `END` 标记之外

---

## Phase 0: 收集需求

Ask the user:

1. **Form 还是 Part？**
2. **名称** (e.g., `ShopForm`, `HpBarPart`)
3. **用途** — 这个界面的功能是什么？
4. **需要的控件** — 哪些按钮、文本、图片？按前缀命名：

| 前缀 | 组件类型 | 示例 |
|------|---------|------|
| `Btn` | Button | `BtnConfirm`, `BtnClose` |
| `Txt` | Text | `TxtTitle`, `TxtGold` |
| `Img` | Image | `ImgIcon`, `ImgBg` |
| `RawImg` | RawImage | `RawImgPortrait` |
| `Input` | InputField | `InputName` |
| `Slider` | Slider | `SliderVolume` |
| `Toggle` | Toggle | `ToggleMute` |
| `Scroll` | ScrollRect | `ScrollList` |
| `Dropdown` | Dropdown | `DropdownLevel` |
| `Rect` | RectTransform | `RectPanel` |
| `CanvasGroup` | CanvasGroup | `CanvasGroupFade` |
| `LayoutElem` | LayoutElement | (layout control) |
| `Fitter` | ContentSizeFitter | (layout control) |
| `HLayout` | HorizontalLayoutGroup | (layout control) |
| `VLayout` | VerticalLayoutGroup | (layout control) |
| `Grid` | GridLayoutGroup | (layout control) |

5. **FormType** (仅 Form): `Persistent` (HUD) / `Popup` (弹窗) / `Transition` (过场)

Do NOT proceed to Step 1 until these are answered.

---

## Step 1: 预制体白模 (用户手动)

**目标**: 在 Unity 中搭建 prefab 的视觉结构。

### 操作清单

1. 在 Project 窗口找到 `Assets/AssetBundles/UI/` 目录
2. 创建子目录 `{Name}/`
3. 右键 → Create → Prefab，命名 `{Name}.prefab`
4. 双击打开 prefab，添加 Canvas（如无）
5. 按需求创建子控件（Panel、Button、Text、Image...）
6. **按前缀命名**每个控件（参考 Phase 0 的前缀表）
7. 调整布局、锚点、字体、颜色（参考 `.claude/docs/ui-layout-guide.md`）
   - 字体: Hero=48, Title=36, Body=28, Button=22, Info=16, Caption=12
   - 颜色: Canvas #12121f, Panel #1a1a2e, 文字 #e2e2e2, 辅助 #9999aa
   - 按钮: 200×56（主要）/ 160×44（紧凑）
   - 间距: 4px 网格, 标准 padding=16px
   - 锚点: HUD 左上(0,1) / 右上(1,1), Popup 居中(0.5,0.5), 遮罩全屏拉伸
   - CanvasScaler: 1920×1080, MatchWidthOrHeight, Match=0.5

### Part 额外注意

- Part prefab 创建在 `Assets/AssetBundles/UI/{ParentForm}Parts/{Name}.prefab` 或同级目录
- Part 根节点同样需要 Canvas（如果是独立渲染的 UI 元素）

完成后告知我，进入 Step 2。

---

## Step 2: 挂组件 + 代码生成 (用户手动)

### 操作清单

1. **挂 `UIFormScript`** — 选中 prefab 根节点 → Add Component → 搜索 `UIFormScript` → 添加
2. **挂 `UiViewBinding`** — 同样在根节点 Add Component → 搜索 `UiViewBinding` → 添加
3. **配置 UiViewBinding**:
   - `View Type`: 选择 **Form** 或 **Part**
   - `Form Name`: 输入 `{Name}`（如 `ShopForm`）
   - `Form Type` (仅 Form): 选 Persistent/Popup/Transition
   - `Namespace`: 通常保持 `GenBall.UI`
   - `Output Path`: 保持默认或自定义
4. **点 Scan Bindings** — 确认扫描结果与设计一致
   - 检查 Detected Bindings 列表
   - 取消勾选不需要绑定的控件
5. **点 Generate Code** — 生成三个文件到输出目录

### 确认事项

- [ ] `{Name}View.cs` 已生成
- [ ] `{Name}Logic.cs` 已生成
- [ ] `{Name}ViewData.cs` 已生成
- [ ] 无 Warnings 提示（有则截图给我）

完成后告知我，我会读取生成的文件进入 Step 3。

---

## Step 3: 读取并确认生成结果 (Claude 自动)

Read the three generated files from `Assets/Scripts/GenBall/UI/{Name}/` (or the configured output path). Confirm:

1. View.cs: `{Name}View : UIBusinessFormBase<{Name}ViewData>` (Form) 或 `PartViewBase<{Name}ViewData>` (Part)
2. Logic.cs: `{Name}Logic : BusinessFormLogic` (Form) 或 `BusinessPartLogic<{Name}View>` (Part)
3. BindControls() 正确绑定了所有控件
4. `### GENERATED_BINDINGS_START ###` / `END` 标记存在

If any file is missing or has errors, tell the user to re-run Generate. Do NOT manually write binding code.

---

## Step 4: 写业务逻辑 (Claude 自动)

所有手写代码放在标记区域**之外**。标记区域内的内容由代码生成器管理，不可修改。

### 4A: ViewData.cs

Fill in the data fields the View needs to display:

```csharp
public class ShopFormViewData
{
    public string Title;
    public int Gold;
    public bool CanPurchase;
    // ... add fields as needed
}
```

### 4B: View.cs (Form)

Add outside the `### GENERATED_BINDINGS ###` markers:

```csharp
protected override void DoBusinessStart()
{
    base.DoBusinessStart();
    BindControls();  // generated — do NOT remove
    BindEvents();     // add this line
}

protected override void RefreshView()
{
    if (ViewData == null) return;
    TxtTitle.text = ViewData.Title;
    TxtGold.text = ViewData.Gold.ToString();
    // ... update all controls from ViewData
}

private void BindEvents()
{
    BtnConfirm?.onClick.AddListener(() =>
        UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.ShopForm_Confirm));
    BtnClose?.onClick.AddListener(() =>
        UIManager.Instance.UIEventRouter.FireNow((int)UIEventKey.ShopForm_CloseRequest));
}

// Optional: dedicated refresh for frequently-updated control
public void RefreshGold(int gold)
{
    TxtGold.text = gold.ToString();
}
```

### 4C: View.cs (Part)

Same pattern but inherits `PartViewBase<T>` instead of `UIBusinessFormBase<T>`:

```csharp
protected override void DoBusinessStart()
{
    base.DoBusinessStart();
    BindControls();
    BindEvents();
}

protected override void RefreshView()
{
    if (ViewData == null) return;
    // ... update controls
}
```

### 4D: Logic.cs (Form)

Add outside the markers. Use **unified ViewData management** (not creating new ViewData each time):

```csharp
// ### GENERATED_BINDINGS_START ###
// (PrefabPath, FormType, View property — generated, do NOT edit)
// ### GENERATED_BINDINGS_END ###

// ---- Business Logic ----

private readonly ShopFormViewData _viewData = new();
private bool _isViewReady;

protected override void OnFormCreated()
{
    base.OnFormCreated();
    View = BoundForm.GetComponentInChildren<ShopFormView>();

    // Initialize display data
    _viewData.Title = "Shop";
    View?.SetViewData(_viewData);
}

protected override void OnFormBound(UIFormScript form)
{
    base.OnFormBound(form);

    // Subscribe to UI events (View button clicks)
    UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.ShopForm_Confirm, OnConfirm);
    UIManager.Instance.UIEventRouter.Subscribe((int)UIEventKey.ShopForm_CloseRequest, OnCloseRequest);

    // Subscribe to global events (data changes from other systems)
    CEventRouter.Instance.Subscribe<int>((int)GlobalEventId.GoldChanged, OnGoldChanged);

    _isViewReady = true;
}

protected override void OnFormUnbound(UIFormScript form)
{
    _isViewReady = false;

    UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.ShopForm_Confirm, OnConfirm);
    UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.ShopForm_CloseRequest, OnCloseRequest);
    CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.GoldChanged, OnGoldChanged);

    View = null;
    base.OnFormUnbound(form);
}

protected override void OnFormDestroying()
{
    // 兜底取消（防止异常路径 OnFormUnbound 未调用）
    UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.ShopForm_Confirm, OnConfirm);
    UIManager.Instance.UIEventRouter.Unsubscribe((int)UIEventKey.ShopForm_CloseRequest, OnCloseRequest);
    CEventRouter.Instance.Unsubscribe<int>((int)GlobalEventId.GoldChanged, OnGoldChanged);
    base.OnFormDestroying();
}

public static ShopFormLogic Open()
{
    return BusinessLogicManager.Instance.CreateLogic<ShopFormLogic>();
}

// ---- Event Handlers ----

private void OnConfirm()
{
    // Handle confirm logic
}

private void OnCloseRequest()
{
    CloseForm();
}

private void OnGoldChanged(int gold)
{
    _viewData.Gold = gold;
    // Use targeted refresh for frequent updates
    View?.RefreshGold(gold);
}

private void RefreshAll()
{
    View?.SetViewData(_viewData);
}
```

### 4E: Logic.cs (Part)

Part Logic inherits `BusinessPartLogic<TView>`. Key differences:

```csharp
// ### GENERATED_BINDINGS_START ###
// (PrefabPath, View property — generated, do NOT edit)
// ### GENERATED_BINDINGS_END ###

private readonly HpBarPartViewData _viewData = new();

protected override void OnPartCreated()
{
    base.OnPartCreated();
    // Initialize display data
    BoundView?.SetViewData(_viewData);
}

protected override void OnViewBound(PartViewBase view)
{
    base.OnViewBound(view);
    // Subscribe to events here
}

protected override void OnViewUnbound(PartViewBase view)
{
    // Unsubscribe events here
    base.OnViewUnbound(view);
}

public static HpBarPartLogic Create(HpBarPartView partView)
{
    return BusinessLogicManager.Instance.CreateLogic<HpBarPartLogic>(
        p => p.ParentTransform = partView.transform);
}
```

### 4F: UIEventKey 注册

If new UI events are needed, add entries to `Assets/Scripts/GenBall/UI/UIEventKey.cs`:

```csharp
// ===== ShopForm 事件 =====
ShopForm_Confirm = 30,
ShopForm_CloseRequest = 31,
```

### ViewData 刷新策略

- **大部分数据初始化时刷新**: 在 `OnFormBound()` 中调用 `RefreshAll()`
- **小部分频繁变化**: 在 View 提供专用方法 (e.g., `RefreshGold(int gold)`)
- **避免**: 每次数据变化都 new 一个 ViewData / 每次都全量 SetViewData

---

## Step 5: 编译验证 (用户手动)

完成所有代码后，提示用户：

1. 运行 `/compile` 或直接在 Unity 中编译
2. 修正编译错误（如有）
3. 验证 UI 在游戏中正常显示

If the user asks for help debugging, re-read the generated files first to avoid overwriting generated code.

---

## 常见错误

| 错误 | 正确做法 |
|------|---------|
| 自己写 BindControls / 控件属性 | 等用户跑完生成器，读取生成结果 |
| 用 TextMeshPro / TMP_Text | 只用 `UnityEngine.UI.Text` |
| 继承旧框架 FormBase | 等生成器输出 `BusinessFormLogic` / `UIBusinessFormBase` |
| ViewData 每次临时 new | 统一管理一个 `_viewData` 实例 |
| 改动标记区域内的代码 | 标记内只有生成器可以修改 |

## 参考

- `.claude/docs/ui-architecture.md` — 完整架构文档
- `.claude/docs/ui-ai-guide.md` — 设计决策指南（Form/Part 选择、ViewData 策略、事件绑定、刷新策略、代码模板、参考实现排名）
- `.claude/docs/ui-layout-guide.md` — 布局白皮书（字体层级、颜色规范、间距体系、锚点规则、层级模板）
- `Assets/Scripts/GenBall/UI/MainHudForm/` — Form 参考实现
- `Assets/Scripts/GenBall/UI/StartForm/` — Popup Form 参考
- `Assets/Scripts/GenBall/UI/UIEventKey.cs` — UI 事件定义
