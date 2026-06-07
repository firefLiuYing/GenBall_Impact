# 框架层 (Yueyn)

## 核心架构

`FrameworkBase`（MonoBehaviour, DontDestroyOnLoad）是唯一入口。Awake 中初始化基础设施单例，Start/Update/FixedUpdate/LateUpdate 中驱动 `SystemUpdaterManager` 和 `CPoolManager`。

业务项目通过 `GenBall.Framework.FrameworkDefault` 继承 `FrameworkBase`，在 `DoInit()` 中注册所有业务 ISystem。

## 基础设施模块

非 ISystem，通过 `Singleton<T>.Instance` 直接访问，在 `FrameworkBase.Awake()` 中惰性初始化：

| 模块 | 访问 | 说明 |
|------|------|------|
| 事件 | `CEventRouter.Instance` | 全局事件总线 |
| 资源 | `CResourceManager.Instance` | 通过 `IResourceHelper`（Editor/AssetBundle）加载 |
| UI | `UIManager.Instance` | Form 生命周期 + UIEventRouter + 层管理 |
| 对象池 | `CPoolManager.Instance` | GameObject 池化 |
| 业务逻辑 | `BusinessLogicManager.Instance` | FormLogic/PartLogic 生命周期管理 |

## 业务系统 (ISystem)

通过 `SystemRepository.Instance` 注册和获取：

```csharp
// 注册（在 FrameworkDefault.DoInit() 中）
SystemRepository.Instance.RegisterSystem<IBuffSystem>(new BuffSystemDefault());

// 获取（任意位置）
SystemRepository.Instance.GetSystem<IBuffSystem>();
```

`RegisterSystem` 自动调用 `system.Init()`，并检测是否实现更新接口后注册到 `SystemUpdaterManager`。`UnregisterSystem` 自动调用 `UnInit()`。

## 更新接口（系统级）

| 接口 | 方法 | 调度位置 | SystemScope |
|------|------|----------|-------------|
| `IFrameUpdate` | `FrameUpdate(float)` | Update | Game/Framework |
| `ILogicUpdate` | `LogicUpdate(float)` | FixedUpdate | Game/Framework |
| `ILateFrameUpdate` | `LateFrameUpdate(float)` | LateUpdate | Game/Framework |

`SystemScope.Game` 的更新受暂停控制，`Framework` 不受影响。

> 注意：实体级更新接口（`IEntityFrameUpdate`/`IEntityLogicUpdate`）在 `GenBall.Framework.Entity`，是 BattleEntity 组件专用，由 `EntityUpdateSystem` 调度，与上述系统级接口无关。

## 新系统开发规则

- **实现 `ISystem`**，禁止继承 MonoBehaviour
- 需要帧更新时实现 `IFrameUpdate`/`ILogicUpdate`/`ILateFrameUpdate`
- 通过 `SystemRepository.Instance.RegisterSystem<T>(impl)` 注册
- 框架层（Yueyn）不定义业务枚举/常量，只定义接口和默认实现

## 新旧体系

旧 `IComponent` + `GameEntry` + `Entry` 体系已完全废弃。新代码禁止使用。现存旧代码保持运行，逐步迁移。

`ISystem` 替代 `IComponent` 的核心变化：解耦（接口 + 默认实现）、统一调度（SystemUpdaterManager）、纯 C#（不依赖 MonoBehaviour）、可暂停（Game/Framework Scope 隔离）。

## 关键工具

| 类 | 位置 | 用途 |
|----|------|------|
| `Singleton<T>` | Utils/ | 纯 C# 单例，双检锁 |
| `SafeIterableList<T>` / `SafeIterableDict<K,V>` | Utils/ | 双缓冲集合，迭代中可安全增删 |
| `EventDispatcher` | Event/ | 局部事件总线，可被任意对象持有 |
| `Fsm<T>` | Fsm/ | 完整泛型 FSM，IReference 体系 |
| `SimpleFsm<TContext>` | Fsm/ | 轻量 FSM，无 IReference 依赖 |
| `ReferencePool` | Base/ReferencePool/ | 静态泛型引用池 |
| `ObjectPoolManager` | ObjectPool/ | GameObject 池化（新版用 CPoolManager） |
| `Timer` | Timer/ | 静态 Timer（Subscribe/Pause/Resume） |
| `CoroutineRunner` | Utils/ | 非 MB 类启动协程的桥 |
