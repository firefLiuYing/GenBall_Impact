# CODEBUDDY.md This file provides guidance to CodeBuddy when working with code in this repository.

## Project Overview

GenBall_Impact 是基于 **Unity 2022.3.42f1c1** 的游戏项目。当前处于**框架重构阶段**，核心命名空间：
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

## Architecture — 新框架（Yueyn/Main）

> **当前状态**：资源加载和UI框架已重构为新架构，其他模块仍在旧体系中或迁移中。

### 核心启动流程（新架构）

```
[Scene中的GameObject]
  └─ 挂载 FrameworkDefault (FrameworkBase 子类, DontDestroyOnLoad)
      ├─ Awake()
      │   ├─ SystemRepository 单例初始化
      │   └─ DoInit() → 注册子系统：
      │       ├─ RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle())
      │       └─ RegisterSystem<IUISystem>(new UISystemDefault())
      ├─ Update()    → SystemRepository.RenderUpdate(deltaTime)   // 遍历所有 IRenderUpdate
      └─ FixedUpdate()→ SystemRepository.LogicUpdate(deltaTime)   // 遍历所有 ILogicUpdate
```

**核心差异**：新框架不再依赖 GameEntry/Entry/IComponent 作为主启动链路。`FrameworkDefault` 是唯一 MonoBehaviour 入口，通过 `SystemRepository`（IoC容器）管理子系统生命周期和帧更新分发。

### SystemRepository — IoC 容器（★ 核心）

位于 `Main/SystemRepository.cs`，是整个新框架的基石：

```csharp
// 接口定义
public interface ISystem { void Init(); void UnInit(); }
// 可选帧更新接口
public interface IRenderUpdate { void RenderUpdate(float deltaTime); }  // Update驱动
public interface ILogicUpdate { void LogicUpdate(float deltaTime); }     // FixedUpdate驱动

// 注册（强制推荐用接口类型）
SystemRepository.Instance.RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle());
// 获取
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();
```

**关键特性**：
- 面向接口注册（非接口类型会输出警告），同一接口只能注册一个实现
- 注册时自动检测 `IRenderUpdate` / `ILogicUpdate` 并加入对应缓存列表（遍历时防并发修改）
- 唯一性约束，重复注册抛异常

### 资源加载系统（已重构）

采用策略模式，两套可切换的实现：

| 类 | 用途 | 加载方式 |
|---|---|---|
| `IResourceSystem` | 接口定义 (`Load/LoadSync/Unload`) | — |
| `ResourceSystemAssetBundle` | **生产环境默认实现** | AssetBundle + Manifest + 引用计数 |
| `ResourceSystemEditor` | 编辑器开发模式 | Resources.Load |
| `ResourceManager` (旧) | 编辑器辅助 (IComponent) | AssetDatabase.LoadAssetAtPath |

**`AssetBundleLoader` 核心能力**：
- Manifest驱动：启动时加载平台Manifest获取完整依赖图
- 引用计数管理：每Bundle维护引用计数，归零才真正卸载
- 自动依赖加载：加载前递归解析并预加载所有依赖Bundle
- 同步/异步双通道：`LoadBundle` 同步 / `LoadBundleAsync` 协程异步
- 内部创建隐藏 GameObject + CoroutineRunner 运行协程（因为 IResourceSystem 不是 MonoBehaviour）

**`ResourceSystemAssetBundle` 工作流**：
```
Load(path) → ResolveAssetPath(解析path→bundleName+assetName)
          → LoadBundleAsync(bundleName) [含依赖自动加载]
          → LoadAssetAsync<T>(bundleName, assetName) → 回调返回
```

### UI系统（已重构为 MVC 架构）

这是最重量级的重构模块。**新UI框架与旧的 FormBase/VmBase/UIManager 体系完全独立并存**。

#### 三层分离架构

```
UISystemDefault (IUISystem + ILogicUpdate) ← SystemRepository 管理
  │
  ├── 管理所有 UIFormScript 实例 (按 FormId 索引)
  │   ├── _persistentForms  (常驻UI, sortingOrder: 0-99)
  │   ├── _popupForms       (弹窗UI, sortingOrder: 100+, 按10递增)
  │   └── _transitionForm   (过场UI, sortingOrder: 1000, 独占)
  │
  ├── 内置: UI根节点自动创建 / 预制体缓存 / Form对象池 / 焦点管理 / 层级排序 / 渐显渐隐动画
  │
  └── UIFormScript (MonoBehaviour通用容器, ~419行)
        │
        ├── Logic层 (纯 C# class, 无MonoBehaviour):
        │   ├── UILogicBase (抽象基类) ← 所有UI逻辑的基类
        │   │   ├── UIFormLogic (全屏页面, 自动绑定 UIFormView)
        │   │   └── UIPartLogic (子页面/部件, 手动绑定 UIPartView<T>)
        │   └── UILogicManager (单例, 管理所有Logic实例生命周期)
        │
        └── View层 (MonoBehaviour):
            ├── UIComponent (可复用组件基类)
            ├── UIFormView (全屏页面View, 含 UIFormType 配置)
            └── UIPartView (子页面View, 含 RectTransform 操作)
```

#### 三种UI页面类型

| 类型 | 枚举值 | 行为特征 | 典型场景 |
|---|---|---|---|
| Persistent | 常驻UI | 始终显示不关闭, order 0-99 | MainHUD, 血条 |
| Popup | 弹窗UI | 可多层叠加后开在上, order 100+ | 背包, 设置面板 |
| Transition | 过场UI | 独占显示(隐藏其他), 同时只存在一个, order 1000 | 加载界面, 过场动画 |

#### UI打开完整生命周期

```
logic.OpenForm(param)
  → UISystemDefault.OpenForm(prefabPath, logic, param)
    ├─ 尝试从对象池获取 (CanReuse=true 时复用)
    ├─ 否则: LoadPrefab → Instantiate → GetComponent<UIFormScript>
    ├─ InternalInit(logic, param):
    │   ├─ 初始化Canvas (ScreenSpaceOverlay, 1920x1080参考分辨率)
    │   ├─ 收集所有UIComponent (按Priority排序)
    │   ├─ logic.BindView(form) → 绑定View引用
    │   ├─ c.InternalInit(form) → 初始化Component
    │   ├─ logic.OnInit(param) + logic.SetViewData(param)
    ├─ 按 UIFormType 分类设置父节点
    └─ InternalOpen():
        ├─ c.InternalOpen()
        ├─ logic.OnEnter()
        └─ PlayFadeIn() (0.3秒渐显)
        └─ UpdateFocus() / UpdateSortingOrder()
```

关闭时反向执行：logic.OnExit() → c.Close() → FadeOut(0.3s) → 回收对象池或Destroy

### 双轨组件机制（重要！）

当前存在**两套组件体系并存**：

| 维度 | **新体系 (ISystem)** | **旧体系 (IComponent)** |
|---|---|---|
| 接口 | `ISystem` (+可选 `ILogicUpdate`/`IRenderUpdate`) | `IComponent` (含Priority/完整生命周期) |
| 管理器 | `SystemRepository` (IoC容器) | `Entry` 类 |
| 入口驱动 | `FrameworkBase.Update/FixedUpdate` | `GameEntry.Update/FixedUpdate` |
| 注册方式 | `RegisterSystem<ITInterface>(instance)` | `Entry.Register()` 或自动发现 |
| 适用范围 | **Resource、UI（已迁移）** | Event, FSM, ObjectPool, Timer, 及 GenBall 业务模块 |

**迁移方向推测**：未来可能将 Event/FSM/ObjectPool/Timer 等也迁移到 ISystem 体系统一管理。

## Architecture — 旧架构（仍在使用中）

以下模块仍在使用旧体系（`IComponent`/`Entry`/`GameEntry`），通过 `GameEntry.GetModule<T>()` 访问：

```csharp
GameEntry.Event, GameEntry.Fsm, GameEntry.Buff, GameEntry.CharacterCreator,
GameEntry.Timeline, GameEntry.Bullet, GameEntry.Evolution,
GameEntry.Save, GameEntry.Player, GameEntry.Map, GameEntry.Execute, GameEntry.Scene
```

### IEntity / EntityCreator 实体工厂系统

|| EntityCreator | 管理的接口 | 用途 |
|---|---|---|---|
| | `EntityCreator<CharacterState>` | 角色状态 | 玩家、敌人（新架构） |
| | `EntityCreator<BulletState>` | 子弹状态 | 新子弹系统 |
| | `EntityCreator<WeaponState>` | 武器状态 | 新武器系统 |
| | `EntityCreator<IEnemy>` | 敌人 | 敌人基类 |
| | `EntityCreator<IMapBlock>` | 地图块 | 地图分块加载 |

`IEntity` 生命周期：`OnSpawn()` → `EntityUpdate(float)` → `OnRecycle()`

### Singleton 系统

跨模块单例（`GenBall.Utils.Singleton`）：`DamageSystem`, `DeathSystem`, `TeleportSystem`, `SceneSystem`, `InteractSystem`, `PauseManager`, `GameManager`

### 战斗系统架构（核心）

所有战斗效果通过 **Buff系统** 实现：
- **伤害管线**：Attacker.BeforeCauseDamage → Defender.BeforeTakeDamage → TakeDamage实际扣血 → AfterCauseDamage → AfterTakeDamage
- **死亡管线**：Victim.BeforeDie(可取消) → AfterDie → Killer.AfterKill → Die()
- **Buff核心**：`IBuff`/`BuffObj`(IReference实例) + `IBuffContainer`(CharacterState/BulletState/WeaponState) + `BuffSystem`(全局管理器)
- **Trigger Points**：子弹4个 + 伤害4个 + 生命3个 + Buff管理4个 共15个回调钩子
- **角色**：`CharacterState` 实现 IDamageable + IBuffContainer + IEntity，自动收集 ICharacterInitializer/ICharacterController
- **Stats**：`StatValue<T>`(IReference) 支持 BaseValue + Modifier列表（Add/FloatMultiply/IntMultiply）

### 流程/程序系统

`ExecuteComponent` (priority 10000) 启动FSM：`ProcedureLoadState` → `StartFormState` → `LoadSceneState`

### 地图系统

`MapModule` 按层加载地图块 + `SceneModule` 异步场景加载 + `TeleportSystem` 跨场景传送

### 事件系统

两层事件：全局 `EventManager`(代码生成 GlobalEventSystem.Generated.cs) + 本地 `ILocalEventManager`(生成 EffectEvents.Generated.cs)

### ReferencePool 对象复用

大量数据类通过 ReferencePool 复用（`DamageInfo`, `DeathInfo`, `BulletLaunchInfo`, `AddBuffInfo`, `BuffObj`, `StatValue<T>` 等），使用 `.Create()` 工厂方法创建，用完自动释放回池。

## Key Code Patterns

### 使用新资源系统
```csharp
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();
res.Load<GameObject>("Assets/AssetBundles/Common/Player/Prefab/Player.prefab", go => {
    // 回调中使用加载的资源
});
```

### 创建新UI（新框架）
```csharp
// 1. 创建 Logic 类（纯C#）
public class MyFormLogic : UIFormLogic {  // 或继承 UILogicBase
    public override void OnInit(object param) { /* 初始化 */ }
    public override void OnEnter() { /* 进入 */ }
    public override void OnExit() { /* 退出 */ }
}

// 2. 创建 View 类（MonoBehaviour，挂到预制体上）
public class MyFormView : UIFormView {
    public UIFormType FormType => UIFormType.Popup;
    // UI元素引用...
}

// 3. 打开UI
var logic = new MyFormLogic();
logic.OpenForm("Assets/path/to/prefab.prefab", userData);
```

### 创建新Buff（不变）
1. 在 `BuffId` 枚举添加ID → 2. 继承 `BuffObj` 实现Trigger接口 → 3. 在 `BuffIdToExtension.ToType()` 注册 → 4. 配置 `BuffModelConfig`

### 造成伤害（不变）
```csharp
var info = DamageInfo.Create(defender, damage, tags, direction, impactForce, attacker);
DamageSystem.Instance.ApplyDamage(info); // DamageInfo自动释放
```

### 发射子弹（不变）
```csharp
GameEntry.Bullet.FireBullet(BulletLaunchInfo.Create(model, spawnPoint, renderPoint, dir, source));
```

### 创建角色实体（不变）
```csharp
GameEntry.CharacterCreator.AddPrefab<CharacterState>("Name", "path/to/prefab");
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("Name", pos, rot, parent);
```

## 重要注意事项

- **双轨并存是当前最大特征**：新 ISystem 体系（Resource/UI 已完成）vs 旧 IComponent 体系（Event/FSM/ObjectPool/Timer + 全部业务模块）。开发新功能时需判断应注册到哪套体系
- **新旧UI完全独立**：新UI走 `UISystemDefault`（MVC: UILogicBase + UIFormScript + UIComponent），旧UI走 `UIManager`（FormBase + VmBase）。新建UI优先使用新框架
- **新旧武器并存**：`WeaponBase`（IEffect旧体系）和 `WeaponState`（IBuff新体系）
- **Git分支**：当前在 `v2_framework` 分支
- **输入系统**：Unity 新 Input System
- **代码生成**：不要手动修改 Generated 目录下的文件（GlobalEventSystem.Generated.cs、EffectEvents.Generated.cs、.Bind.cs）
- **部分中文注释**
- **命名约定**：新UI用 `UILogic`/`UIView` 后缀；旧UI用 `FormName.cs`/`FormNameVm.cs`；ScriptableObject配置以 `Config` 结尾；partial类拆分功能
