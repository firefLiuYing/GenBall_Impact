# CODEBUDDY.md This file provides guidance to CodeBuddy when working with code in this repository.

## Project Overview

GenBall_Impact 是基于 **Unity 2022.3.42f1c1** 的游戏项目。当前处于**框架重构阶段（v2_framework 分支）**，核心命名空间：
- **Yueyn** (`Assets/Scripts/Yueyn/`)：底层框架，包含新旧两套体系
- **GenBall** (`Assets/Scripts/GenBall/`)：游戏业务逻辑（战斗、敌人、UI、地图、玩家、交互）

## Development Commands

### 打开项目
用 Unity 2022.3.42f1c1 打开项目根目录，或直接打开 `GenBall_Impact.sln` 进行代码编辑。

### 构建与运行
- **Build**：Unity Editor → File → Build Settings → Build
- **Run**：点击 Unity Editor Play 按钮，或运行构建出的可执行文件

### 主场景
入口场景为 `Launcher.unity`（`Assets/Scenes/Launcher.unity`），其他场景包括 Prologue、Episode1 及多个测试场景。

---

## 重构背景与目标

> 详细重构计划见 `Assets/Scripts/系统重构方案.md`

旧框架存在 14 个核心痛点，正在按优先级逐步解决：

| # | 痛点 | 解决方案 | 状态 |
|---|------|----------|------|
| 一 | 单例设计不佳（仅支持有new()的class） | 拆分 `Singleton<T>`(纯C#) + `MonoSingleton<T>`(MonoBehaviour) | ✅ 已完成 |
| 二 | GameEntry 与系统管理紧耦合 + 切换场景重复注册 | `SystemRepository`(IoC容器) + `FrameworkBase`(唯一入口) + `ISystem` 接口化 | ✅ 已完成 |
| 三 | 业务系统未接口化，IComponent 冗余接口多 | 用 `ISystem` 取代 `IComponent`，简化接口，需要Update的自行继承 `IRenderUpdate`/`ILogicUpdate` | 🔄 部分完成(Resource/UI/Event) |
| 四 | 资源管理不可用于打包（原方案AB未完成） | 定义 `IResourceSystem` 接口 + 两套可切换实现 | ✅ 已完成 |
| 五 | UI框架耦合严重 + MVVM过繁杂 | 重构为三层MVP架构（UISystem → UIScript → UIComponent），从GenBall迁移到Yueyn命名空间 | ✅ 已完成 |
| 六 | FSM状态转移权限设计缺陷 | 让FSM本身拥有状态转移权限，由持有者主动控制更新 | ⬜ 待做 |
| 七 | Entity泛型池冗余（ObjectPoolManager过度设计） | 简化ObjectPoolManager或重新设计Entity池方案 | ⬜ 待做 |
| 八 | 事件总线旧版与Buff耦合 + 事件ID无统一结构 | 已由新CEventSystem解决（保留旧版兼容） | 🔄 部分完成 |
| 九 | Buff配置繁琐（Enum标识） | 优化配置方式，替换Enum为更灵活的标识方案 | ⬜ 待做 |
| 十 | 指令系统缺失（优先级/打断/条件执行） | 设计完整的指令系统：优先级覆盖、打断机制、执行条件 | ⬜ 待做 |
| 十一 | ReferencePool使用复杂（手动Acquire/Release易遗忘） | **`IPoolSystem`(ISystem)接口 + `PoolSystemDefault`实现，包装ReferencePool纳入SystemRepository管理** | ✅ 已完成 |
| 十二 | 流程控制简陋（仅启动流程） | 扩展流程控制能力 | ⬜ 待做 |
| 十三 | 启动流程过度异步 | 简化启动流程为同步/可控方式 | ⬜ 待做 |
| 十四 | 存档系统未验证 | 验证并简化存档系统 | ⬜ 待做 |

---

## Architecture — 新框架（Yueyn/Main）

### 核心启动流程

```
[Scene中的GameObject]
  └─ 挂载 FrameworkDefault (FrameworkBase 子类, DontDestroyOnLoad)
      ├─ Awake()
      │   ├─ SystemRepository 单例初始化
      │   └─ DoInit() → 注册子系统：
│       ├─ RegisterSystem<IEventSystem>(new CEventSystem())        // 事件系统
│       │  #if UNITY_EDITOR
│       ├─ RegisterSystem<IResourceSystem>(new ResourceSystemEditor())   // 编辑器模式
│       │  #else
│       ├─ RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle()) // 生产环境AB包
│       │  #endif
│       ├─ RegisterSystem<IUISystem>(new UISystemDefault())          // UI系统
│       └─ RegisterSystem<IPoolSystem>(new PoolSystemDefault())      // 对象池系统
      ├─ Update()    → SystemRepository.RenderUpdate(deltaTime)
      └─ FixedUpdate()→ SystemRepository.LogicUpdate(deltaTime)
```

**关键设计决策**：
- `FrameworkDefault` 位于 `GenBall.Framework` 命名空间（针对本游戏特化），不再放在 Yueyn 框架层
- `FrameworkDefault` 是**唯一的 MonoBehaviour 入口**，标记 `DontDestroyOnLoad`
- 所有系统不再继承 MonoBehaviour，与 Unity 解耦
- 编辑器/生产环境的资源加载通过 **`#if UNITY_EDITOR` 宏切换**（非运行时判断）

### SystemRepository — IoC 容器（★ 核心）

位于 `Main/SystemRepository.cs`，轻量级服务定位器：

```csharp
// 最小接口
public interface ISystem { void Init(); void UnInit(); }
// 可选帧更新接口（按需继承）
public interface IRenderUpdate { void RenderUpdate(float deltaTime); }   // Update驱动
public interface ILogicUpdate { void LogicUpdate(float deltaTime); }     // FixedUpdate驱动

// 注册（推荐用接口类型作为泛型参数）
SystemRepository.Instance.RegisterSystem<IResourceSystem>(new ResourceSystemEditor());
// 获取
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();
```

**特性**：
- 强制面向接口注册（传入非接口类型会警告但不阻断）
- 同一接口只能注册一个实现（唯一性约束）
- 自动检测并分发到 `RenderUpdate` / `LogicUpdate` 缓存列表（防遍历中并发修改）

### 资源加载系统（✅ 已完成）

采用策略模式，**编辑器与生产环境完全分离**：

|| 类 | 环境 | 加载方式 |
|---|---|---|---|
| **接口** | `IResourceSystem` (`Load/LoadSync/Unload`) | — | — |
| **默认(编辑器)** | `ResourceSystemEditor` | `#if UNITY_EDITOR` | `AssetDatabase.LoadAssetAtPath<T>(path)` |
| **生产环境** | `ResourceSystemAssetBundle` | `#else` | AssetBundle + Manifest + 引用计数 |

**切换机制**：在 `FrameworkDefault.DoInit()` 中通过编译宏选择实现，打包后自动使用 AB 包路径。

**`ResourceSystemAssetBundle` 核心能力**：
- `AssetBundleLoader` 底层支持：Manifest驱动依赖图、引用计数管理、同步/异步双通道
- 内部创建隐藏 GameObject + `CoroutineRunner` 运行协程（因 IResourceSystem 非 MonoBehaviour）
- 路径解析：`Assets/path/to/file.prefab` → 推断 Bundle 名（取第一级目录）+ asset名
- 异步加载带进度回调：0(开始) → 0.3(Bundle加载完) → 0.7(Asset加载完) → 1.0(完成)

**`ResourceSystemEditor` 实现细节**：
- 直接使用 `AssetDatabase.LoadAssetAtPath<T>(path)` 加载（非 Resources.Load）
- `LoadSync<T>` 和 `Load` 均同步返回（进度回调直接 0→1）
- `Unload` 为空实现（由 Unity 管理资源释放）

### UI系统（✅ 已完成 — MVP 三层架构）

**这是最重量级的重构模块**，已从 GenBall 命名空间迁移到 Yueyn/UI 下。

#### 架构总览

```
UISystemDefault (IUISystem + ILogicUpdate) ← SystemRepository 管理
  │
  ├── 页面集合管理 (按 FormId 索引):
  │   ├── _persistentForms  (常驻UI, sortingOrder: 0-99, 不关闭)
  │   ├── _popupForms       (弹窗UI, sortingOrder: 100+, 按10递增, 后开在上)
  │   └── _transitionForm   (过场UI, sortingOrder: 1000, 独占隐藏其他)
  │
  ├── 内置功能:
  │   ├── UI根节点自动创建 (UIRoot/Persistent/Popup/Transition/Camera/EventSystem)
  │   ├── 预制体缓存 (_prefabCache) + Form对象池 (_formPool, CanReuse控制复用)
  │   ├── 焦点管理 (UpdateFocus) + 层级排序 (UpdateSortingOrder)
  │   └── CanvasGroup渐显渐隐动画 (FadeIn/FadeOut 各0.3秒)
  │
  └── UIFormScript (MonoBehaviour通用容器)
        │
        ├── Logic层 (纯 C# class, 无MonoBehaviour依赖):
        │   ├── UILogicBase (抽象基类, 含完整生命周期)
        │   │   ├── UIFormLogic (全屏页面便捷基类)
        │   │   │   ├─ PrefabPath 属性 → 定义预制体路径
        │   │   │   ├─ BindView() → 绑定View引用 + 向View传递Logic引用
        │   │   │   ├─ OpenFormAsync(param) / OpenForm(param) → 打开页面
        │   │   │   └─ CloseForm() → 关闭当前页面
        │   │   └── UIPartLogic (子页面/部件便捷基类)
        │   └── UILogicManager (单例, CreateLogic<T>/DestroyLogic 管理生命周期)
        │
        └── View层 (MonoBehaviour, 挂载到预制体上):
            ├── UIComponent (可复用组件基类)
            ├── UIFormView (全屏页面View, 含 UIFormType 配置)
            └── UIPartView (子页面View, 含 RectTransform 操作)
```

#### 三种页面类型

| 类型 | UIFormType | 行为 | 典型用途 |
|---|---|---|---|
| Persistent | 常驻UI | 始终显示不关闭, order 0-99 | MainHUD, 血条 |
| Popup | 弹窗 | 可多层叠加, 后开的在上, order 100+ | 背包, 设置面板 |
| Transition | 过场 | 独占显示(隐藏所有其他), 同时只一个, order 1000 | 加载界面, 过场动画 |

#### 打开UI的完整生命周期（以实际代码为例）

```csharp
// Step 1: 创建Logic实例（通过UILogicManager管理）
var logic = UILogicManager.Instance.CreateLogic<TestFormLogic>();

// Step 2: 调用OpenFormAsync（内部自动读取Logic.PrefabPath加载预制体）
logic.OpenFormAsync("Test Data: Hello World!");
```

内部流程：
```
OpenFormAsync(param)
  → 读取 logic.PrefabPath 获取预制体路径
  → UISystemDefault.OpenFormAsync(prefabPath, logic, param)
    ├─ LoadPrefab (通过IResourceSystem异步加载)
    ├─ Instantiate → GetComponent<UIFormScript>
    ├─ InternalInit(logic, param):
    │   ├─ 初始化Canvas (ScreenSpaceOverlay, 1920x1080)
    │   ├─ 收集子对象UIComponent (按Priority排序)
    │   ├─ logic.BindView(form): base.BindView绑定View引用 → 自定义逻辑(如向View传Logic引用)
    │   ├─ c.InternalInit(form) 初始化各Component
    │   ├─ logic.OnInit(param)
    │   └─ logic.SetViewData(param) ← 数据传递给View
    ├─ 按 UIFormType.FormType 分类挂到对应父节点
    └─ InternalOpen():
        ├─ c.InternalOpen()
        ├─ logic.OnEnter()
        └─ PlayFadeIn(0.3秒) → UpdateFocus() → UpdateSortingOrder()
```

关闭时反向执行：`CloseForm()` → logic.OnExit() → c.Close() → FadeOut(0.3s) → 回收对象池(CanReuse=true) 或 Destroy

#### View ↔ Logic 通信模式（以TestForm为例）

```csharp
// Logic层: TestFormLogic : UIFormLogic
protected override string PrefabPath => "Assets/.../TestForm.prefab";

internal override void BindView(UIFormScript form) {
    base.BindView(form);
    if (View is TestFormView testView) testView.SetLogic(this);  // 向View传递自身引用
}
public override void SetViewData(object param) {
    if (View is TestFormView testView) testView.SetTitle($"Title - {param}");
}
public void OnCloseButtonClicked() { CloseForm(); }  // View回调到这里

// View层: TestFormView : UIFormView
private TestFormLogic _logic;
public void SetLogic(TestFormLogic logic) => _logic = logic;
public void SetTitle(string title) => titleText.text = title;  // Logic调用此方法更新显示
private void OnCloseButtonClicked() => _logic?.OnCloseButtonClicked();  // 按钮事件转发到Logic
```

### 事件系统（✅ 已完成 — 框架层）

**核心设计**：`IEventSystem` 接口 + `CEventSystem` 实现类，全局/局部复用同一个类。

**框架层 vs 业务层职责分离**：
- **框架层** (`Yueyn.Event`)：`IEventSystem` + `CEventSystem`，只认 `int` 事件ID，不定义任何业务 enum
- **业务层** (`GenBall.*`)：自行定义 enum（如全局事件ID、UI专用事件ID），转为 `int` 使用

**关键特性**：
- 参数直达：`Subscribe<T1, T2>(id, Action<T1, T2>)` / `FireNow<T1, T2>(id, a, b)` — 0~4个泛型参数，**彻底消灭 EventArgs 包装和代码生成器**
- 延迟触发（`Fire`）通过闭包捕获参数入队，帧末由 `IRenderUpdate` 统一派发；立即触发（`FireNow`）同步调用
- 快照遍历防并发修改（回调中 Subscribe/Unsubscribe 安全）
- `SetDefaultHandler` 处理无订阅者的事件
- `Check(id, handler)` 检查是否已订阅

**全局 vs 局部**：
| 用法 | 代码 | 生命周期 |
|---|---|---|
| 全局 | `RegisterSystem<IEventSystem>(new CEventSystem())` | SystemRepository 管理，DontDestroyOnLoad |
| 局部 | `new CEventSystem()` 持有在某实例中 | 随持有者生命周期 |

**使用示例**：
```csharp
// 获取全局事件系统
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();

// 订阅
eventSystem.Subscribe<int, string>(1001, (code, msg) => Debug.Log($"{code}: {msg}"));
eventSystem.Subscribe(1002, () => Debug.Log("No args event"));

// 立即触发
eventSystem.FireNow<int, string>(1001, 200, "Hello");
eventSystem.FireNow(1002);

// 延迟触发（帧末执行）
eventSystem.Fire<int, string>(1001, 200, "Deferred");

// 取消订阅
eventSystem.Unsubscribe<int, string>(1001, myHandler);

// 局部事件系统（如UI专用）
var uiEvents = new CEventSystem();
uiEvents.Subscribe<string>(1, msg => { /* ... */ });
```

### 对象池系统（✅ 已完成 — 框架层）

**核心设计**：`IPoolSystem` 接口 + `PoolSystemDefault` 实现类，包装现有 `ReferencePool` 能力并纳入 SystemRepository 管理。

**设计决策**：
- **保留现有 ReferencePool 不变**（`Yueyn.Base.ReferencePool`），它本身设计良好且被 71+ 处广泛使用
- `IPoolSystem` 作为 ISystem 层的统一入口，内部委托给 ReferencePool 执行
- 不继承 MonoBehaviour，纯 C# 实现
- 追踪 Acquire/Release 统计信息（UsingCount/UnusedCount）
- **旧 ObjectPoolManager** (`Yueyn.ObjectPool`) 保留不动，仅 `EntityCreator` 一处使用

```
IPoolSystem (ISystem) ← SystemRepository 管理 (FrameworkDefault 注册)
  │
  └─ PoolSystemDefault (实现):
       ├─ Acquire<T>() / Release()    → 委托给 ReferencePool
       ├─ PreCreate<T>(count)          → 预热池
       ├─ GetUsingCount / GetUnusedCount → 追踪统计
       └─ RemoveAll(type) / ClearAll() → 清理缓存
```

**使用方式对比**：

|| 旧方式 (直接用静态类) | 新方式 (通过 ISystem) |
|---|---|---|
| 获取 | `ReferencePool.Acquire<DamageInfo>()` | `pool.Acquire<DamageInfo>()` |
| 归还 | `ReferencePool.Release(info)` | `pool.Release(info)` |
| 预创建 | `ReferencePool.Add<DamageInfo>(10)` | `pool.PreCreate<DamageInfo>(10)` |
| 入口 | 全局静态方法 | `SystemRepository.Instance.GetSystem<IPoolSystem>()` |
| 生命周期 | 随进程 | 随 SystemRepository (DontDestroyOnLoad) |

**关键特性**：
- 线程安全：内部 ReferencePool 使用 lock 保护队列操作
- 自动 Clear：Release 时自动调用 `IReference.Clear()` 重置状态
- 懒创建：按需为每个 Type 创建内部池
- 统计追踪：记录通过 IPoolSystem 接口的 UsingCount（注意：旧代码直接调 ReferencePool 不会被此统计覆盖）

### 双轨组件机制（重要！当前最大特征）

项目中存在**两套组件体系并存**，开发新代码前必须判断应注册到哪套：

| 维度 | **新体系 (ISystem)** | **旧体系 (IComponent)** |
|---|---|---|
| 接口 | `ISystem` (+可选 `ILogicUpdate`/`IRenderUpdate`) | `IComponent` (Priority + Init/Update/FixedUpdate/Shutdown 全套) |
| 管理容器 | `SystemRepository` (IoC) | `Entry` 类 |
| 入口驱动 | `FrameworkBase.Update/FixedUpdate` | `GameEntry.Update/FixedUpdate` |
| 注册方式 | `RegisterSystem<IInterface>(instance)` | `Entry.Register()` 或 GetComponentsInChildren 自动发现 |
| 是否MonoBehaviour | ❌ 所有系统不继承MonoBehaviour | ⚠️ 大量是MonoBehaviour |
| 适用范围 | **Resource ✅ / UI ✅ / Event ✅ / Pool ✅** | Event(旧) / FSM / ObjectPool(旧) / Timer / 全部 GenBall 业务模块 |
| 命名空间 | Yueyn.Main / Yueyn.Resource / Yueyn.UI / Yueyn.Event | Yueyn.Event(旧) / Yueyn.Fsm 等 + GenBall.* |

**迁移方向**：未来将 Event(旧)/FSM/ObjectPool(旧)/Timer 及业务模块逐步迁入 ISystem 体系。**Pool(对象池) ✅ 已完成。**

### 模块迁移准则（重要！）

所有模块迁移都必须遵循以下原则：
1. **先建新后拆旧**：保留旧模块不动，新开发一套（接口 + 实现），验证新模块可用后再逐步将依附于旧模块的业务代码迁移过来，最后再移除旧模块
2. **接口先行**：所有框架层模块必须接口化（`IXxxSystem : ISystem`），再写实现，以方便按需替换不同实现
3. **框架层不定义业务**：框架层（Yueyn 命名空间）只提供通用接口和默认实现，不定义任何业务 enum/常量。业务层（GenBall 命名空间）自行定义 enum 和特化子类
4. **新旧代码可以共存**：当前大量旧代码仍在使用中，这是预期行为，不要主动删除或修改未涉及的旧代码
5. **编写测试验证**：框架层模块完成后必须编写测试脚本验证功能正确性，测试脚本统一放在 `GenBall/Tests/` 目录（MonoBehaviour 挂载即可运行），**优先使用 Debug.Log 输出结果**，仅在 Log 无法满足时才考虑借助 UI
6. **UI 禁用 TextMeshPro**：项目内所有 UI 文本组件统一使用 `UnityEngine.UI.Text`（Legacy Text），**禁止使用 `TMP_Text`/`TextMeshProUGUI`/`TMPro` 相关类型**
7. **任务完成同步文档**：每完成一个迁移任务或模块开发，必须同步更新本 CODEBUDDY.md 文档中对应的状态标记（如 ✅/🔄/⬜）和新增模块的架构说明，确保文档与代码实际状态一致

---

## Architecture — 旧架构（仍在大量使用中）

以下模块仍通过旧体系运行，访问方式：`GameEntry.GetModule<T>()` 或便捷属性：

```csharp
GameEntry.Event, GameEntry.Fsm, GameEntry.Buff, GameEntry.CharacterCreator,
GameEntry.Timeline, GameEntry.Bullet, GameEntry.Evolution,
GameEntry.Save, GameEntry.Player, GameEntry.Map, GameEntry.Execute, GameEntry.Scene
```

### IEntity / EntityCreator 实体工厂

|| EntityCreator | 管理接口 | 用途 |
|---|---|---|---|
| | `EntityCreator<CharacterState>` | 角色状态 | 玩家、敌人 |
| | `EntityCreator<BulletState>` | 子弹状态 | 新子弹系统 |
| | `EntityCreator<WeaponState>` | 武器状态 | 新武器系统 |
| | `EntityCreator<IEnemy>` | 敌人 | 敌人基类 |
| | `EntityCreator<IMapBlock>` | 地图块 | 地图分块加载 |

`IEntity` 生命周期：`OnSpawn()` → `EntityUpdate(float)` → `OnRecycle()`

### Singleton 系统

`GenBall.Utils.Singleton<T>` 单例列表：`DamageSystem`, `DeathSystem`, `TeleportSystem`, `SceneSystem`, `InteractSystem`, `PauseManager`, `GameManager`

### 战斗系统（核心 — Buff驱动）

所有战斗效果通过 Buff系统 实现：

- **伤害管线**：Attacker.BeforeCauseDamage → Defender.BeforeTakeDamage → IDamageable.TakeDamage() → AfterCauseDamage → AfterTakeDamage
- **死亡管线**：Victim.BeforeDie(可Cancelled取消) → AfterDie → Killer.AfterKill → IHealth.Die()
- **Buff核心类型**：
  - `IBuff`/`BuffObj`(IReference)：Buff实例，支持叠层和完整生命周期(OnAdd/OnStack/OnUnstack/OnRemove/OnUpdate)
  - `IBuffContainer`：由 CharacterState / BulletState / WeaponState 实现
  - `BuffSystem`(IComponent)：全局Buff管理器（添加含叠层逻辑 / 移除 / 每帧Tick）
  - `AddBuffInfo`(IReference)：创建请求，BuffId → BuffModelConfig 自动查找
- **15个Trigger Points**：子弹4 + 伤害4 + 生命3 + Buff管理4
- **角色** `CharacterState`：IDamageable + IBuffContainer + IEntity，自动收集 ICharacterInitializer(OnInit) + ICharacterController(每帧Tick)，Command模式处理移动/旋转
- **Stats数值**：`StatValue<T>`(IReference) = BaseValue + Modifier列表；`IntStat`/`FloatStat`具体实现；`DamageValue`= Σ区域倍率 × 基础伤害 + 附加伤害
- **Modifier类型**：Add / FloatMultiply / IntMultiply

### 流程/程序系统

`ExecuteComponent` (priority 10000) 启动FSM：`ProcedureLoadState` → `StartFormState` → `LoadSceneState`

### 地图/传送/存档

- `MapModule`：基于玩家位置按层加载周围地图块
- `SceneModule`：异步场景加载
- `TeleportSystem`：跨场景传送(SceneName + SavePointIndex)
- `SaveComponent` + `GameManager`：存档读写

### 事件系统（两层）

1. **全局事件** `EventManager` → 代码生成 `GlobalEventSystem.Generated.cs` 提供类型安全API
2. **本地事件** `ILocalEventManager` → 生成 `EffectEvents.Generated.cs`
3. 事件数据继承 `GameEventArgs`，通过 `EventPool` 管理

### ReferencePool 对象复用

大量数据类实现 `IReference` 并通过 ReferencePool 复用：`DamageInfo`, `DeathInfo`, `BulletLaunchInfo`, `AddBuffInfo`, `BuffObj`, `StatValue<T>`, `VmBase` 等。`.Create()` 工厂方法创建，管线化自动释放回池。

---

## Key Code Patterns

### 使用新对象池系统（IPoolSystem）

```csharp
var pool = SystemRepository.Instance.GetSystem<IPoolSystem>();

// 获取对象（从池中复用或 new 新实例）
var damageInfo = pool.Acquire<DamageInfo>();
damageInfo.Defender = target;
damageInfo.Damage = 10;
// ... 使用 ...

// 归还对象到池中（自动调用 Clear() 重置状态）
pool.Release(damageInfo);

// 预创建一批对象放入池中
pool.PreCreate<DamageInfo>(20);

// 查看统计
int usingCount = pool.GetUsingCount(typeof(DamageInfo));
int unusedCount = pool.GetUnusedCount(typeof(DamageInfo));

// 清理指定类型的缓存
pool.RemoveAll(typeof(DamageInfo));

// 全清理
pool.ClearAll();
```

**业务层 .Create() 工厂方法模式（推荐，不变）**：
```csharp
// 既有模式保持不变：.Create() 内部封装 Acquire + 初始化
public static DamageInfo Create(IEntity defender, int damage, ...) {
    var info = ReferencePool.Acquire<DamageInfo>(); // 或 pool.Acquire<DamageInfo>()
    info.Defender = defender;
    info.Damage = damage;
    return info;
}
// 用完后：ReferencePool.Release(info); 或 pool.Release(info);
```

### 使用新资源系统

```csharp
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();

// 异步加载（推荐，带进度）
res.Load<GameObject>("Assets/AssetBundles/Common/Player/Prefab/Player.prefab",
    onSuccess: go => { /* 使用资源 */ },
    onFailed: err => Debug.LogError(err),
    onProgress: p => Debug.Log($"Loading: {p:P0}")
);

// 同步加载
var prefab = res.LoadSync<GameObject>("Assets/.../SomePrefab.prefab");
```

### 创建新UI（新框架 MVP）

```csharp
// === Logic层（纯C# class）===
public class MyFormLogic : UIFormLogic
{
    protected override string PrefabPath => "Assets/AssetBundles/UI/MyForm/MyForm.prefab";

    internal override void BindView(UIFormScript form)
    {
        base.BindView(form);
        if (View is MyFormView view) view.SetLogic(this);  // 向View传递Logic引用
    }

    public override void OnInit(object param) { /* 初始化 */ }
    public override void SetViewData(object param)
    {
        // 将数据推送到View
        if (View is MyFormView view) view.SetContent(param.ToString());
    }
    public override void OnEnter() { /* 进入 */ }
    public override void OnExit() { /* 退出 */ }
}

// === View层（MonoBehaviour，挂到预制体上）===
public class MyFormView : UIFormView
{
    [SerializeField] private Text contentText;
    private MyFormLogic _logic;

    public void SetLogic(MyFormLogic logic) => _logic = logic;  // Logic调用
    public void SetContent(string content) => contentText.text = content;  // Logic调用的数据方法

    protected override void OnInit() { base.OnInit(); /* 绑定按钮事件 */ }
    protected override void OnClose() { base.OnClose(); /* 清理事件 */ }
}

// === 打开UI（外部调用）===
var logic = UILogicManager.Instance.CreateLogic<MyFormLogic>();
logic.OpenFormAsync(someData);

// 或手动指定路径打开
var logic2 = new MyPartLogic();
SystemRepository.Instance.GetSystem<IUISystem>().OpenForm("path/to/part.prefab", logic2, data);
```

### 造成伤害（旧体系，不变）

```csharp
var info = DamageInfo.Create(defender: target, damage: 10,
    tags: new List<string>{"bullet"}, direction: hitDir,
    impactForce: 5f, attacker: source);
DamageSystem.Instance.ApplyDamage(info);  // DamageInfo自动释放到ReferencePool
```

### 发射子弹（旧体系，不变）

```csharp
GameEntry.Bullet.FireBullet(BulletLaunchInfo.Create(
    model: bulletModel, logicSpawnPoint: muzzlePos,
    rendererSpawnPoint: muzzleVisualPos, spawnDirection: aimDir,
    source: playerGo));
// BulletLaunchInfo自动释放
```

### 创建角色实体（旧体系，不变）

```csharp
// 注册（通常在某模块Init中执行一次）
GameEntry.CharacterCreator.AddPrefab<CharacterState>("EnemyName", "Assets/.../Enemy.prefab");

// 创建
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("EnemyName", position, rotation, parent);

// 回收
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

### 创建新Buff（不变）

1. `BuffId` 枚举添加ID → 2. 继承 `BuffObj` 实现所需 Trigger 接口 → 3. `BuffIdToExtension.ToType()` 注册映射 → 4. `BuffModelConfig` 配置数据

---

## 重要注意事项

- **双轨并存**：新 ISystem 体系(Resource/UI/Event已完成) vs 旧 IComponent 体系(全部其余)。**开发新功能必须先判断归属哪套体系**
- **新旧UI完全独立**：新UI走 `UISystemDefault`(MVP: UILogicBase + UIFormScript + UIComponent)；旧UI走 `UIManager`(MVVM: FormBase + VmBase + ItemBase + UiBindTool生成.Bind.cs)。**新建UI必须用新框架**
- **UI 文本组件**：统一使用 `UnityEngine.UI.Text`，**禁止使用 TMP_Text / TextMeshProUGUI**
- **测试入口目录**：`GenBall/Tests/`（MonoBehaviour 入口脚本，纯 Log 输出优先）
- **新旧武器并存**：`WeaponBase`(IEffect旧) vs `WeaponState`(IBuff新)
- **资源系统切换靠编译宏**：`FrameworkDefault.DoInit()` 中 `#if UNITY_EDITOR` 选择实现，不是运行时判断
- **Git分支**：`v2_framework`
- **输入系统**：Unity 新 Input System
- **代码生成禁止手改**：Generated目录下(`GlobalEventSystem.Generated.cs`/`EffectEvents.Generated.cs`/`.Bind.cs`)
- **部分中文注释**
- **命名约定**：
  - 新UI：`XxxLogic.cs`(Logic) + `XxxView.cs`(View)，Logic继承 `UIFormLogic`/`UILogicBase`，View继承 `UIFormView`/`UIComponent`
  - 旧UI：`FormName.cs` + `FormNameVm.cs` + `FormName.Bind.cs`(生成)
  - ScriptableObject配置：`Config` 后缀
  - partial类：按功能拆分（如 `GameEntry.Bullet.cs`）
