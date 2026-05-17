# 框架重构计划

> **状态追踪**：本文档记录 GenBall_Impact 从旧 IComponent 体系向新 ISystem 体系迁移的进度和方案。
> 
> **最新状态**：新体系基建已完成，发现 4 个需修复的问题（详见 `design-review.md`）
> 
> **下一步**：修复基建问题 → 开始业务迁移（详见 `migration-master-plan.md`）

## 进度总览

| 模块 | 状态 | 优先级 | 负责人 |
|---|---|---|---|
| 单例系统 | ✅ 已完成 | P0 | - |
| 系统管理（SystemRepository） | ✅ 已完成 | P0 | - |
| 资源系统（IResourceSystem） | ⚠️ 需修复 | P1 | - |
| UI 系统（IUISystem） | ⚠️ 需修复 | P1 | - |
| 事件系统（IEventSystem） | ⚠️ 需修复 | P1 | - |
| 对象池系统（IPoolSystem） | ⚠️ 需修复 | P2 | - |
| **业务迁移** | ⬜ 待开始 | P0 | - |
| FSM 状态机 | ⬜ 待做 | P2 | - |
| Entity 对象池 | ⬜ 待做 | P3 | - |
| Buff 配置优化 | ⬜ 待做 | P3 | - |
| 指令系统 | ⬜ 待做 | P3 | - |
| 流程控制 | ⬜ 待做 | P4 | - |
| 启动流程 | ⬜ 待做 | P4 | - |
| 存档系统 | ⬜ 待做 | P4 | - |

## 🚨 当前阻塞问题

**问题**：基建系统未完全接入新体系
- IResourceSystem 接口定义缺失
- CEventSystem 未实现 IEventSystem 接口
- CPoolManager 未实现 IPoolSystem 接口
- 四大基建系统未注册到 SystemRepository

**影响**：阻塞所有业务模块迁移

**修复方案**：详见 `design-review.md` 和 `migration-master-plan.md` 阶段 0

---

## 已完成模块

### ✅ 单例系统（P0）
**痛点**：旧 `ISingleton` 只能支持有 `new()` 构造函数的类，不支持 MonoBehaviour。

**解决方案**：
- `Singleton<T>`：纯 C# 类单例
- `MonoSingleton<T>`：MonoBehaviour 单例

**位置**：`Yueyn/Base/Singleton.cs`

---

### ✅ 系统管理（P0）
**痛点**：
- GameEntry 与系统管理紧耦合
- 切换场景时重复注册系统
- 系统需要自己实现跨场景逻辑

**解决方案**：
- `SystemRepository`：IoC 容器，管理系统注册/注销/获取
- `FrameworkBase`：唯一 MonoBehaviour 入口，标记 `DontDestroyOnLoad`
- `ISystem` 接口：最小接口（Init/UnInit）
- `IRenderUpdate` / `ILogicUpdate`：可选帧更新接口

**位置**：
- `Yueyn/Main/SystemRepository.cs`
- `Yueyn/Main/FrameworkBase.cs`
- `GenBall/Framework/FrameworkDefault.cs`

**关键设计**：
- 所有系统不再继承 MonoBehaviour
- 强制面向接口注册
- 自动检测并分发帧更新

---

### ✅ 资源系统（P1）
**痛点**：旧资源管理仅支持 Editor 模式，打包后不可用。

**解决方案**：
- `IResourceSystem` 接口：`Load` / `LoadSync` / `Unload`
- `ResourceSystemEditor`：编辑器模式，使用 `AssetDatabase`
- `ResourceSystemAssetBundle`：生产环境，使用 AB 包 + Manifest + 引用计数

**位置**：
- `Yueyn/Resource/IResourceSystem.cs`
- `Yueyn/Resource/ResourceSystemEditor.cs`
- `Yueyn/Resource/ResourceSystemAssetBundle.cs`

**切换机制**：
```csharp
#if UNITY_EDITOR
    RegisterSystem<IResourceSystem>(new ResourceSystemEditor());
#else
    RegisterSystem<IResourceSystem>(new ResourceSystemAssetBundle());
#endif
```

---

### ✅ UI 系统（P1）
**痛点**：
- UI 框架层与业务耦合
- 生命周期管理依赖简单栈实现
- MVVM 架构过于繁杂
- 代码在 GenBall 命名空间（应该在 Yueyn）

**解决方案**：重构为三层 MVP 架构
- **UISystem**：页面容器管理（开闭、生命周期、层级、渐显渐隐）
- **UIScript**：组件容器管理（生命周期、时序问题）
- **UIComponent**：实际功能（MVP 分层）

**位置**：
- `Yueyn/UI/IUISystem.cs`
- `Yueyn/UI/UISystemDefault.cs`
- `Yueyn/UI/UIFormScript.cs`
- `Yueyn/UI/UILogicBase.cs` / `UIFormLogic.cs` / `UIPartLogic.cs`
- `Yueyn/UI/UIComponent.cs` / `UIFormView.cs` / `UIPartView.cs`

**三种页面类型**：
- `Persistent`：常驻 UI（sortingOrder: 0-99）
- `Popup`：弹窗 UI（sortingOrder: 100+，后开在上）
- `Transition`：过场 UI（sortingOrder: 1000，独占显示）

**关键特性**：
- 预制体缓存 + Form 对象池（CanReuse 控制复用）
- 焦点管理 + 层级排序
- CanvasGroup 渐显渐隐动画（各 0.3 秒）

---

### ✅ 事件系统（P1）
**痛点**：
- 旧版与 Buff 系统耦合
- 无全局 EventId 结构
- 必须用 EventArg 包装参数，编写繁琐

**解决方案**：
- `IEventSystem` 接口 + `CEventSystem` 实现
- 参数直达：`Subscribe<T1, T2>` / `FireNow<T1, T2>`（0~4 个泛型参数）
- 延迟触发（`Fire`）通过闭包捕获参数入队，帧末派发
- 立即触发（`FireNow`）同步调用
- `EventDispatcher` 类用于局部事件总线

**位置**：
- `Yueyn/Event/IEventSystem.cs`
- `Yueyn/Event/CEventSystem.cs`
- `Yueyn/Event/EventDispatcher.cs`
- `GenBall/Event/GlobalEvents.cs`（事件 ID 定义）

**状态**：框架层已完成，已注册到 SystemRepository，旧系统保持运行（共存）

**访问方式**：
```csharp
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();
eventSystem.Fire<int>((int)GlobalEvents.Player.HealthChanged, newHealth);
```

**迁移指南**：`.claude/docs/event-system-refactoring-summary.md`

---

### ✅ 对象池系统（P2）
**痛点**：ReferencePool 使用繁琐，需要手动 Acquire/Release，易遗忘。

**解决方案**：
- `IPoolSystem` 接口 + `PoolSystemDefault` 实现
- 包装现有 ReferencePool 能力
- 纳入 SystemRepository 管理

**位置**：
- `Yueyn/Pool/IPoolSystem.cs`
- `Yueyn/Pool/PoolSystemDefault.cs`

**关键特性**：
- 追踪 Acquire/Release 统计信息
- 线程安全（内部 ReferencePool 使用 lock）
- 自动 Clear（Release 时调用 `IReference.Clear()`）

**注意**：旧 ReferencePool 保留不变，71+ 处旧代码继续工作

---

## 待完成模块

### ⬜ FSM 状态机（P2）
**痛点**：FSM 本身无状态转移权限，只能依靠状态类内部事件监听，编写繁琐。

**解决方案**：
- 让 FSM 本身拥有状态转移权限
- 由持有者主动控制更新

**预计工作量**：中等

---

### ⬜ Entity 对象池（P3）
**痛点**：ObjectPoolManager 过度设计，定义过多不必要接口，几乎所有使用者都需要重新实现和管理。

**解决方案**：
- 简化 ObjectPoolManager
- 或重新设计 Entity 池方案

**预计工作量**：中等

---

### ⬜ Buff 配置优化（P3）
**痛点**：用 Enum 作为 Buff 标识写起来很繁琐。

**解决方案**：
- 优化配置方式
- 替换 Enum 为更灵活的标识方案（如字符串 ID + ScriptableObject）

**预计工作量**：中等

---

### ⬜ 指令系统（P3）
**痛点**：CharacterState 接受指令时，缺少优先级覆盖、打断机制、执行条件处理。

**解决方案**：
- 设计完整的指令系统
- 支持优先级覆盖
- 支持打断机制
- 支持执行条件

**预计工作量**：大

---

### ⬜ 流程控制（P4）
**痛点**：流程只有一个启动流程，依托简单状态机，只加载了场景和 Player/Enemy。

**解决方案**：
- 扩展流程控制能力
- 支持更复杂的流程编排

**预计工作量**：中等

---

### ⬜ 启动流程（P4）
**痛点**：写了一系列复杂异步代码，实际并不必要。

**解决方案**：
- 简化启动流程为同步/可控方式

**预计工作量**：小

---

### ⬜ 存档系统（P4）
**痛点**：实现繁琐，且没有验证是否能正常工作。

**解决方案**：
- 验证并简化存档系统

**预计工作量**：中等

---

## 迁移原则

详见 `.codebuddy/rules/module-migration.md`

核心原则：
1. 先建新后拆旧
2. 接口先行
3. 框架层不定义业务
4. 新旧代码可以共存
5. 编写测试验证
6. 任务完成同步文档

---

## 更新日志

| 日期 | 模块 | 变更 |
|---|---|---|
| 2026-05-16 | 事件系统 | ✅ 完成 IEventSystem 迁移，创建 EventDispatcher 和 GlobalEvents |
| 2026-05-13 | 文档 | 重构为结构化计划文档 |
| 2026-05-13 | 对象池系统 | ✅ 完成 IPoolSystem 迁移 |
| 2026-05-13 | UI 系统 | ✅ 完成三层 MVP 架构重构 |
| 2026-05-13 | 资源系统 | ✅ 完成双实现切换方案 |
| 2026-05-13 | 系统管理 | ✅ 完成 SystemRepository + FrameworkBase |
| 2026-05-13 | 单例系统 | ✅ 完成 Singleton + MonoSingleton |
