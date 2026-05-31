# UI 架构参考

本文档描述 GenBall_Impact 项目的 UI 框架架构，供开发参考。

## 类层次结构

```
BusinessLogicManager (Singleton)
└── 管理所有 BusinessLogicBase 的生命周期

BusinessLogicBase
├── OnCreate() / OnDestroy() — sealed, 防重复
├── OnCreateInternal() / OnDestroyInternal() — protected virtual 钩子
└── LogicId — 唯一标识

BusinessPartLogicContainer : BusinessLogicBase
├── 管理子 PartLogic 列表 (SafeIterableList)
├── DiscoverChildPartLogics(Transform root) — 自动发现 PartViewBase 并创建 PartLogic
└── OnDestroyInternal → ClearPartLogics()

├── BusinessFormLogic : BusinessPartLogicContainer
│   ├── PrefabPath (abstract) — 预制体路径
│   ├── FormType (virtual, 默认 Popup)
│   ├── OnCreateInternal → OpenForm → BindForm → OnFormCreated → DiscoverChildPartLogics
│   ├── OnDestroyInternal → OnFormDestroying → UnbindForm → CloseForm
│   ├── OnFormBound / OnFormUnbound — 子类钩子
│   └── CloseForm() — 便捷方法
│
└── BusinessPartLogic : BusinessPartLogicContainer
    ├── PrefabPath (abstract)
    ├── ParentTransform — 设置绑定的父节点
    ├── 静态绑定: PartViewBase 已存在于父节点
    ├── 动态绑定: 加载预制体并实例化
    ├── OnPartCreated / OnPartDestroying — 子类钩子
    ├── OnViewBound / OnViewUnbound — 子类钩子
    └── BusinessPartLogic<TView> — 强类型泛型变体
```

## View 层

```
UIComponent (MonoBehaviour)
├── Form / IsInitialized / IsOpen / IsFocused / IsPaused — 状态
├── DoStart/DoOpen/DoClose/DoFocus/DoUnfocus/DoPause/DoResume — internal 门控
├── DoBusinessStart/DoBusinessOpen/... — protected virtual 钩子
├── Priority — 初始化优先级
└── OnResolutionChanged(Vector2)

├── UIBusinessFormBase : UIComponent
│   └── UIBusinessFormBase<TViewData> — SetViewData() → RefreshView()
│
└── PartViewBase : UIComponent
    └── PartViewBase<TViewData> — SetViewData() → RefreshView()
```

## Unity 层

```
UIManager (Singleton)
├── 管理 UIFormScript 生命周期 (OpenForm / CloseForm / GetForm)
├── 三层根节点: Persistent / Popup / Transition
├── UICamera + EventSystem 自动创建
├── Popup 层自动排序 (Canvas.sortingOrder)
└── UIEventRouter (EventDispatcher) — View → Logic 通信总线

UIFormScript (MonoBehaviour, 挂 prefab 根)
├── Canvas + CanvasScaler (1920×1080, MatchWidthOrHeight=0.5) + GraphicRaycaster
├── 收集子 UIComponent，按 Priority 排序并分发生命周期
├── 淡入淡出动画 (FadeDuration, 默认 0.3s)
└── 分辨率变化监听 (每 0.5s 检测)
```

## 生命周期对照

| 事件 | BusinessFormLogic | UIFormScript | UIBusinessFormBase (View) |
|------|------------------|--------------|--------------------------|
| 创建 | OnCreateInternal → OpenForm → BindForm → **OnFormCreated** | InternalInit → InternalOpen → InternalFocus | DoStart → DoBusinessStart (含 BindControls) |
| 绑定 | **OnFormBound**(form) | — | — |
| 打开 | — | InternalOpen (+ 淡入) | DoOpen → DoBusinessOpen |
| 聚焦 | — | InternalFocus | DoFocus → DoBusinessFocus |
| 失焦 | — | InternalUnfocus | DoUnfocus → DoBusinessUnfocus |
| 解绑 | **OnFormUnbound**(form) | — | — |
| 销毁前 | **OnFormDestroying** | — | — |
| 关闭 | — | InternalClose (+ 淡出) | DoClose → DoBusinessClose |
| 暂停/恢复 | — | InternalPause/Resume | DoPause/Resume |

### Part 生命周期

| 事件 | BusinessPartLogic | PartViewBase |
|------|------------------|--------------|
| 创建 | OnCreateInternal → LoadAndBindPart → **OnPartCreated** → DiscoverChildPartLogics | DoStart |
| 绑定 | **OnViewBound**(view) | — |
| 解绑 | **OnViewUnbound**(view) | — |
| 销毁前 | **OnPartDestroying** → UnbindView → Destroy GameObject (dynamic) | — |

## 通信方式

```
View  ──UIEventRouter.FireNow(UIEventKey)──→  Logic
         (按钮点击、用户交互)

Logic ──View.SetViewData(viewData)─────────→  View
         (数据驱动全量刷新)

Logic ──View.SpecificMethod(data)──────────→  View
         (频繁更新的专用刷新)

Logic ──SystemRepository.GetSystem<T>()────→  其他系统
         (触发游戏逻辑)

全局   ──CEventRouter.Instance.Subscribe───→  Logic
         (HP变化、事件通知)
```

## Form vs Part

| | Form | Part |
|---|------|------|
| 基类 (View) | `UIBusinessFormBase<T>` | `PartViewBase<T>` |
| 基类 (Logic) | `BusinessFormLogic` | `BusinessPartLogic<TView>` |
| 预制体根 | 挂 `UIFormScript` + `UiViewBinding` | 挂 `UiViewBinding` (ViewType=Part) |
| FormType | Persistent/Popup/Transition | 无 |
| 打开方式 | `CreateLogic<T>()` → 自动 OpenForm | 静态: 随父 Form 自动发现; 动态: `Create(partView)` |
| 生命周期 | OnFormCreated/Bound/Unbound/Destroying | OnPartCreated/ViewBound/ViewUnbound/Destroying |
| Part 边界 | 扫描子 PartViewBase 并自动创建 PartLogic | 其子节点不会被父 Form 扫描 (UiViewBinding 作为边界) |

## UiViewBinding 代码生成

```
挂 UiViewBinding 到 prefab 根 ↓
配置 ViewType / FormType / FormName ↓
Scan Bindings (扫描子节点，匹配前缀) ↓
Generate Code (输出到 Assets/Scripts/GenBall/UI/{Name}/)
├─ {Name}View.cs     — public class {Name}View : UIBusinessFormBase<{Name}ViewData>
├─ {Name}Logic.cs    — public class {Name}Logic : BusinessFormLogic
└─ {Name}ViewData.cs — public class {Name}ViewData
```

所有绑定代码写在 `// ### GENERATED_BINDINGS_START ###` / `END` 标记之间。重新 Generate 会覆盖标记内内容，但标记外手写代码保持不变。

## 文件组织

```
Assets/Scripts/GenBall/UI/
├── UIEventKey.cs            — UI 事件枚举
├── {Name}/
│   ├── {Name}View.cs        — View 层 (部分生成)
│   ├── {Name}Logic.cs       — Logic 层 (部分生成)
│   └── {Name}ViewData.cs    — View 数据类
├── SplashForm/              — 启动画面
├── StartForm/               — 主菜单
├── MainHudForm/             — 游戏内 HUD
└── GMConsoleForm/           — 开发者控制台

Assets/AssetBundles/UI/
├── {Name}/
│   └── {Name}.prefab
└── ...
```
