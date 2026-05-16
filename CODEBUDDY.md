# CODEBUDDY.md

> **开发规则**：详见 `.codebuddy/rules/` 目录  
> **重构计划**：详见 `.codebuddy/refactoring-plan.md`

## 项目概览

**GenBall_Impact** 是基于 **Unity 2022.3.42f1c1** 的游戏项目，当前处于**框架重构阶段（v2_framework 分支）**。

### 核心命名空间
- **Yueyn** (`Assets/Scripts/Yueyn/`)：底层框架，包含新旧两套体系
- **GenBall** (`Assets/Scripts/GenBall/`)：游戏业务逻辑

### 开发环境
- **Unity 版本**：2022.3.42f1c1
- **入口场景**：`Assets/Scenes/Launcher.unity`
- **输入系统**：Unity 新 Input System
- **Git 分支**：`v2_framework`

---

## 新框架架构（Yueyn/Main）

### 核心启动流程

```
[Scene中的GameObject]
  └─ 挂载 FrameworkDefault (FrameworkBase 子类, DontDestroyOnLoad)
      ├─ Awake()
      │   ├─ SystemRepository 单例初始化
      │   └─ DoInit() → 注册子系统：
      │       ├─ RegisterSystem<IEventSystem>(new CEventSystem())
      │       ├─ RegisterSystem<IResourceSystem>(new ResourceSystemEditor/AssetBundle())
      │       ├─ RegisterSystem<IUISystem>(new UISystemDefault())
      │       └─ RegisterSystem<IPoolSystem>(new PoolSystemDefault())
      ├─ Update()    → SystemRepository.RenderUpdate(deltaTime)
      └─ FixedUpdate()→ SystemRepository.LogicUpdate(deltaTime)
```

**关键设计**：
- `FrameworkDefault` 是**唯一的 MonoBehaviour 入口**（`GenBall.Framework` 命名空间）
- 所有系统不再继承 MonoBehaviour，与 Unity 解耦
- 资源加载通过 `#if UNITY_EDITOR` 宏切换实现

---

## 核心系统快速参考

### SystemRepository — IoC 容器

```csharp
// 最小接口
public interface ISystem { void Init(); void UnInit(); }
// 可选帧更新接口
public interface IRenderUpdate { void RenderUpdate(float deltaTime); }
public interface ILogicUpdate { void LogicUpdate(float deltaTime); }

// 注册（推荐用接口类型）
SystemRepository.Instance.RegisterSystem<IResourceSystem>(new ResourceSystemEditor());
// 获取
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();
```

### 资源系统（IResourceSystem）

| 实现类 | 环境 | 加载方式 |
|---|---|---|
| `ResourceSystemEditor` | `#if UNITY_EDITOR` | `AssetDatabase.LoadAssetAtPath<T>` |
| `ResourceSystemAssetBundle` | `#else` | AssetBundle + Manifest + 引用计数 |

```csharp
var res = SystemRepository.Instance.GetSystem<IResourceSystem>();

// 同步加载
var prefab = res.LoadSync<GameObject>("Assets/Prefabs/Player.prefab");

// 异步加载
res.Load<GameObject>("Assets/Prefabs/Enemy.prefab", 
    progress => Debug.Log($"Loading: {progress * 100}%"),
    asset => Instantiate(asset));

// 卸载
res.Unload("Assets/Prefabs/Player.prefab");
```

### UI系统（IUISystem）— MVP 三层架构

```
UISystemDefault (IUISystem + ILogicUpdate)
  │
  ├── 页面集合管理:
  │   ├── _persistentForms  (常驻UI, sortingOrder: 0-99)
  │   ├── _popupForms       (弹窗UI, sortingOrder: 100+)
  │   └── _transitionForm   (过场UI, sortingOrder: 1000)
  │
  └── UIFormScript (MonoBehaviour容器)
        ├── Logic层 (纯 C# class):
        │   ├── UILogicBase (抽象基类)
        │   ├── UIFormLogic (全屏页面便捷基类)
        │   └── UIPartLogic (子页面/部件便捷基类)
        │
        └── View层 (MonoBehaviour):
            ├── UIComponent (可复用组件基类)
            ├── UIFormView (全屏页面View)
            └── UIPartView (子页面View)
```

**三种页面类型**：

| UIFormType | 行为 | 典型用途 |
|---|---|---|
| Persistent | 始终显示不关闭, order 0-99 | MainHUD, 血条 |
| Popup | 可多层叠加, 后开的在上, order 100+ | 背包, 设置面板 |
| Transition | 独占显示(隐藏所有其他), order 1000 | 加载界面, 过场动画 |

**使用示例**：

```csharp
// Step 1: 创建Logic实例
var logic = UILogicManager.Instance.CreateLogic<TestFormLogic>();

// Step 2: 打开UI（内部自动读取Logic.PrefabPath加载预制体）
logic.OpenFormAsync("Test Data: Hello World!");

// Step 3: 关闭UI
logic.CloseForm();

// Step 4: 销毁Logic
UILogicManager.Instance.DestroyLogic(logic);
```

**Logic ↔ View 通信**：

```csharp
// Logic层: TestFormLogic : UIFormLogic
protected override string PrefabPath => "Assets/.../TestForm.prefab";

internal override void BindView(UIFormScript form) {
    base.BindView(form);
    if (View is TestFormView testView) testView.SetLogic(this);
}

public override void SetViewData(object param) {
    if (View is TestFormView testView) testView.SetTitle($"Title - {param}");
}

public void OnCloseButtonClicked() { CloseForm(); }

// View层: TestFormView : UIFormView
private TestFormLogic _logic;
public void SetLogic(TestFormLogic logic) => _logic = logic;
public void SetTitle(string title) => titleText.text = title;
private void OnCloseButtonClicked() => _logic?.OnCloseButtonClicked();
```

### 事件系统（IEventSystem）

```csharp
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();

// 订阅（0~4个泛型参数，彻底消灭 EventArgs 包装）
eventSystem.Subscribe<int, string>(1001, (code, msg) => Debug.Log($"{code}: {msg}"));
eventSystem.Subscribe(1002, () => Debug.Log("No args event"));

// 立即触发
eventSystem.FireNow<int, string>(1001, 200, "Hello");

// 延迟触发（帧末执行）
eventSystem.Fire<int, string>(1001, 200, "Deferred");

// 取消订阅
eventSystem.Unsubscribe<int, string>(1001, myHandler);

// 局部事件系统（如UI专用）
var uiEvents = new CEventSystem();
```

### 对象池系统（IPoolSystem）

```csharp
var pool = SystemRepository.Instance.GetSystem<IPoolSystem>();

// 获取对象（从池中复用或 new 新实例）
var damageInfo = pool.Acquire<DamageInfo>();
damageInfo.Defender = target;
damageInfo.Damage = 100;

// 归还对象（自动调用 IReference.Clear() 重置状态）
pool.Release(damageInfo);

// 预创建
pool.PreCreate<DamageInfo>(10);

// 统计信息
int using = pool.GetUsingCount<DamageInfo>();
int unused = pool.GetUnusedCount<DamageInfo>();
```

---

## 旧架构（仍在使用中）

以下模块仍通过旧体系运行，访问方式：`GameEntry.GetModule<T>()` 或便捷属性。

### 旧系统列表

```csharp
GameEntry.Event      // 旧事件系统（代码生成）
GameEntry.Fsm        // 状态机
GameEntry.Buff       // Buff系统
GameEntry.CharacterCreator  // 角色实体工厂
GameEntry.Timeline   // 时间轴
GameEntry.Bullet     // 子弹系统
GameEntry.Evolution  // 进化系统
GameEntry.Save       // 存档系统
GameEntry.Player     // 玩家管理
GameEntry.Map        // 地图管理
GameEntry.Execute    // 流程控制
GameEntry.Scene      // 场景管理
```

### 战斗系统（Buff驱动）

**伤害管线**：
```
Attacker.BeforeCauseDamage 
  → Defender.BeforeTakeDamage 
  → IDamageable.TakeDamage() 
  → AfterCauseDamage 
  → AfterTakeDamage
```

**死亡管线**：
```
Victim.BeforeDie(可Cancelled取消) 
  → AfterDie 
  → Killer.AfterKill 
  → IHealth.Die()
```

**核心类型**：
- `IBuff`/`BuffObj`(IReference)：Buff实例，支持叠层和完整生命周期
- `IBuffContainer`：由 CharacterState / BulletState / WeaponState 实现
- `BuffSystem`(IComponent)：全局Buff管理器
- `AddBuffInfo`(IReference)：创建请求，BuffId → BuffModelConfig 自动查找

**15个Trigger Points**：子弹4 + 伤害4 + 生命3 + Buff管理4

**角色** `CharacterState`：IDamageable + IBuffContainer + IEntity，自动收集 ICharacterInitializer(OnInit) + ICharacterController(每帧Tick)，Command模式处理移动/旋转

**Stats数值**：`StatValue<T>`(IReference) = BaseValue + Modifier列表；`IntStat`/`FloatStat`具体实现；`DamageValue`= Σ区域倍率 × 基础伤害 + 附加伤害

**Modifier类型**：Add / FloatMultiply / IntMultiply

### IEntity / EntityCreator 实体工厂

| EntityCreator | 管理接口 | 用途 |
|---|---|---|
| `EntityCreator<CharacterState>` | 角色状态 | 玩家、敌人 |
| `EntityCreator<BulletState>` | 子弹状态 | 新子弹系统 |
| `EntityCreator<WeaponState>` | 武器状态 | 新武器系统 |
| `EntityCreator<IEnemy>` | 敌人 | 敌人基类 |
| `EntityCreator<IMapBlock>` | 地图块 | 地图分块加载 |

`IEntity` 生命周期：`OnSpawn()` → `EntityUpdate(float)` → `OnRecycle()`

### Singleton 系统

`GenBall.Utils.Singleton<T>` 单例列表：`DamageSystem`, `DeathSystem`, `TeleportSystem`, `SceneSystem`, `InteractSystem`, `PauseManager`, `GameManager`

---

## 常用代码模式

### 使用新对象池系统（IPoolSystem）

```csharp
var pool = SystemRepository.Instance.GetSystem<IPoolSystem>();

// 获取对象
var damageInfo = pool.Acquire<DamageInfo>();
damageInfo.Defender = target;
damageInfo.Damage = 100;

// 使用完毕归还
pool.Release(damageInfo);
```

### 使用旧对象池（ReferencePool）

```csharp
// 旧代码保持不变（71+ 处使用）
var damageInfo = ReferencePool.Acquire<DamageInfo>();
// ... 使用 ...
ReferencePool.Release(damageInfo);
```

### 创建角色实体（旧体系）

```csharp
// 注册（通常在某模块Init中执行一次）
GameEntry.CharacterCreator.AddPrefab<CharacterState>("EnemyName", "Assets/.../Enemy.prefab");

// 创建
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>("EnemyName", position, rotation, parent);

// 回收
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

### 创建新Buff（不变）

1. `BuffId` 枚举添加ID
2. 继承 `BuffObj` 实现所需 Trigger 接口
3. `BuffIdToExtension.ToType()` 注册映射
4. `BuffModelConfig` 配置数据

---

## 重要注意事项

- **双轨并存**：新 ISystem 体系(Resource/UI/Event/Pool已完成) vs 旧 IComponent 体系(全部其余)。**开发新功能必须先判断归属哪套体系**
- **新旧UI完全独立**：新UI走 `UISystemDefault`(MVP)；旧UI走 `UIManager`(MVVM)。**新建UI必须用新框架**
- **UI 文本组件**：统一使用 `UnityEngine.UI.Text`，**禁止使用 TMP_Text / TextMeshProUGUI**
- **测试入口目录**：`GenBall/Tests/`（MonoBehaviour 入口脚本，纯 Log 输出优先）
- **新旧武器并存**：`WeaponBase`(IEffect旧) vs `WeaponState`(IBuff新)
- **资源系统切换靠编译宏**：`FrameworkDefault.DoInit()` 中 `#if UNITY_EDITOR` 选择实现
- **代码生成禁止手改**：Generated目录下(`GlobalEventSystem.Generated.cs`/`EffectEvents.Generated.cs`/`.Bind.cs`)
