# 新体系设计审查报告

> **审查日期**：2026-05-16
> 
> **审查范围**：新 ISystem 体系基建（SystemRepository, FrameworkBase, 四大基建系统）
> 
> **审查结论**：✅ 整体设计合理，发现 4 个需修复的问题

---

## 📋 审查摘要

### 总体评价
新体系架构设计**清晰、解耦、可扩展**，符合 GameFramework 设计理念。核心架构无重大缺陷，但基建系统存在**接口定义缺失**和**注册遗漏**问题。

### 发现问题统计
- 🔴 **严重问题**：0 个
- 🟡 **中等问题**：4 个（基建系统未完全接入新体系）
- 🟢 **轻微问题**：0 个

### 修复优先级
所有问题均为 **P0 优先级**，必须在业务迁移前修复。

---

## ✅ 核心架构审查

### 1. 单例系统

**设计**：
```csharp
public abstract class Singleton<T> where T : class, new() {
    private static T _instance;
    private static readonly object _lock = new object();
    
    public static T Instance {
        get {
            if (_instance == null) {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new T();
                        (_instance as Singleton<T>)?.Init();
                    }
                }
            }
            return _instance;
        }
    }
}
```

**评价**：✅ 优秀
- 双重检查锁定（DCL）保证线程安全
- 懒加载，按需初始化
- 支持虚方法 `Init()` 供子类扩展

**无需修改**

---

### 2. 系统仓库（SystemRepository）

**设计**：
```csharp
public class SystemRepository : Singleton<SystemRepository> {
    private readonly Dictionary<Type, ISystem> _systems = new();
    
    public void RegisterSystem<T>(T system) where T : ISystem {
        if (_systems.ContainsKey(typeof(T)))
            throw new Exception($"System {typeof(T)} is already registered");
        
        if (!typeof(T).IsInterface)
            Debug.LogWarning($"System {typeof(T)} is a class, but not an interface.");
        
        system.Init();
        _systems.Add(typeof(T), system);
        SystemUpdaterManager.Instance.RegisterSystem(system);
    }
    
    public T GetSystem<T>() where T : ISystem {
        if (_systems.TryGetValue(typeof(T), out var system))
            return (T)system;
        Debug.LogError($"System {typeof(T)} is not registered");
        return default(T);
    }
}
```

**评价**：✅ 优秀
- IoC 容器职责单一
- 强制接口注册（警告非接口）
- 自动调用生命周期方法
- 自动注册到更新管理器

**设计亮点**：
1. 泛型约束 `where T : ISystem` 保证类型安全
2. 注册时立即调用 `Init()`，确保系统可用
3. 注销时先从更新器移除，再调用 `UnInit()`，顺序正确

**无需修改**

---

### 3. 更新管理器（SystemUpdaterManager）

**设计**：
```csharp
public class SystemUpdaterManager : Singleton<SystemUpdaterManager> {
    private SystemUpdater _gameUpdater;
    private SystemUpdater _frameworkUpdater;
    private bool _isPaused;
    
    public void RegisterSystem(ISystem system) {
        if (system is ILogicUpdate logicUpdate) {
            var updater = logicUpdate.LogicUpdateScope == SystemScope.Game 
                ? _gameUpdater : _frameworkUpdater;
            updater.RegisterLogicUpdate(logicUpdate);
        }
        // ... FrameUpdate, LateFrameUpdate 同理
    }
    
    public void LogicUpdate(float deltaTime) {
        _frameworkUpdater.DoLogicUpdate(deltaTime);
        if (!_isPaused)
            _gameUpdater.DoLogicUpdate(deltaTime);
    }
}
```

**评价**：✅ 优秀
- 双轨更新器（Framework/Game）设计精妙
- SystemScope 控制暂停范围，避免暂停基建系统
- 三种更新接口（Logic/Frame/LateFrame）覆盖所有需求

**设计亮点**：
1. Framework 更新器不受暂停影响，保证基建系统持续运行
2. 自动检测系统实现的更新接口，无需手动注册
3. 支持同一系统实现多个更新接口

**无需修改**

---

### 4. 框架基类（FrameworkBase）

**设计**：
```csharp
public class FrameworkBase : MonoBehaviour {
    protected SystemRepository SystemRep;
    
    private void Awake() {
        SystemRep = SystemRepository.Instance;
        DontDestroyOnLoad(this);
        
        #if UNITY_EDITOR
        CResourceManager.Instance.SetHelper(new ResourceHelperEditor());
        #else
        CResourceManager.Instance.SetHelper(new ResourceHelperAssetBundle());
        #endif
        
        DoInit();
    }
    
    private void Update() {
        float deltaTime = Time.deltaTime;
        DoFrameUpdate();
        SystemUpdaterManager.Instance.FrameUpdate(deltaTime);
        CPoolManager.Instance.Update(deltaTime, Time.realtimeSinceStartup);
    }
    
    private void FixedUpdate() {
        float deltaTime = Time.fixedDeltaTime;
        DoLogicUpdate();
        SystemUpdaterManager.Instance.LogicUpdate(deltaTime);
    }
}
```

**评价**：✅ 良好，有改进空间
- 唯一 MonoBehaviour 入口，符合设计目标
- DontDestroyOnLoad 保证跨场景存活
- 编译宏切换资源加载模式

**⚠️ 发现问题**：
1. 手动调用 `CPoolManager.Instance.Update()` 和 `_eventSystem?.Update()`
2. 应该让这些系统实现 `IFrameUpdate` 接口，由 SystemUpdaterManager 自动调度

**修复方案**：见"基建系统审查"章节

---

## ⚠️ 基建系统审查

### 1. 资源系统（CResourceManager）

**当前状态**：
```csharp
// IResourceSystem.cs 文件为空！
// CResourceManager.cs
public class CResourceManager : Singleton<CResourceManager> {
    private IResourceHelper _helper;
    
    public void SetHelper(IResourceHelper helper) { _helper = helper; }
    public void Load(string path, Action<object> onSuccess, Action<string> onFailed) { ... }
    public T LoadSync<T>(string path) where T : UnityEngine.Object { ... }
    public void Unload(string path, bool unloadAll = false) { ... }
}
```

**🟡 问题 1：缺少 IResourceSystem 接口定义**
- **影响**：无法通过 SystemRepository 注册和访问
- **严重性**：中等（阻塞业务迁移）

**🟡 问题 2：未注册到 SystemRepository**
- **影响**：新体系无法使用资源系统
- **严重性**：中等（阻塞业务迁移）

**修复方案**：
```csharp
// 1. 定义接口
public interface IResourceSystem : ISystem {
    void Load(string path, Action<object> onSuccess, Action<string> onFailed);
    void Load(string path, Action<object> onSuccess, Action<string> onFailed, Action<float> onProgress);
    T LoadSync<T>(string path) where T : UnityEngine.Object;
    void Unload(string path, bool unloadAll = false);
}

// 2. 实现接口
public class CResourceManager : Singleton<CResourceManager>, IResourceSystem {
    public void Init() { }
    public void UnInit() { }
    // ... 其他方法保持不变
}

// 3. 注册到 SystemRepository
// 在 FrameworkDefault.DoInit() 中
SystemRep.RegisterSystem<IResourceSystem>(CResourceManager.Instance);
```

---

### 2. 事件系统（CEventSystem）

**当前状态**：
```csharp
// CEventSystem.cs
public class CEventSystem {
    private readonly Queue<Action> _pendingEvents = new();
    
    public void Update() {
        while (_pendingEvents.Count > 0)
            _pendingEvents.Dequeue().Invoke();
    }
    
    public void Subscribe<T1>(int id, Action<T1> handler) { ... }
    public void Fire<T1>(int id, T1 arg) { ... }
}
```

**🟡 问题 3：未实现 IEventSystem 接口**
- **影响**：虽然接口已定义，但实现类未实现接口
- **严重性**：中等（阻塞业务迁移）

**🟡 问题 4：需要手动调用 Update()**
- **影响**：在 FrameworkBase.DoFrameUpdate() 中手动调用，不符合新体系设计
- **严重性**：中等（设计不一致）

**修复方案**：
```csharp
// 1. 实现接口
public class CEventSystem : IEventSystem, IFrameUpdate {
    public void Init() { }
    public void UnInit() { Clear(); }
    
    public SystemScope FrameUpdateScope => SystemScope.Framework;
    public void FrameUpdate(float deltaTime) {
        while (_pendingEvents.Count > 0)
            _pendingEvents.Dequeue().Invoke();
    }
    
    // ... 其他方法保持不变
}

// 2. 注册到 SystemRepository
// 在 FrameworkDefault.DoInit() 中
SystemRep.RegisterSystem<IEventSystem>(new CEventSystem());

// 3. 移除手动调用
// 删除 FrameworkBase.DoFrameUpdate() 中的 _eventSystem?.Update();
```

---

### 3. 对象池系统（CPoolManager）

**当前状态**：
```csharp
// CPoolManager.cs
public sealed partial class CPoolManager : Singleton<CPoolManager> {
    public void Update(float elapsedSeconds, float realElapseSeconds) {
        foreach (var objectPool in _objectPools.Values)
            objectPool.Update(elapsedSeconds, realElapseSeconds);
    }
    
    public IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase { ... }
}
```

**🟡 问题 5：缺少 IPoolSystem 接口定义**
- **影响**：无法通过 SystemRepository 注册和访问
- **严重性**：中等（阻塞 EntityCreator 迁移）

**🟡 问题 6：需要手动调用 Update()**
- **影响**：在 FrameworkBase.Update() 中手动调用，不符合新体系设计
- **严重性**：中等（设计不一致）

**修复方案**：
```csharp
// 1. 定义接口
public interface IPoolSystem : ISystem {
    IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase;
    IObjectPool<T> CreateSingleSpawnObjectPool<T>(string name) where T : ObjectBase;
    // ... 其他方法
    bool HasObjectPool<T>(string name = "") where T : ObjectBase;
    IObjectPool<T> GetObjectPool<T>(string name = "") where T : ObjectBase;
    bool DestroyObjectPool<T>(string name = "") where T : ObjectBase;
}

// 2. 实现接口
public sealed partial class CPoolManager : Singleton<CPoolManager>, IPoolSystem, IFrameUpdate {
    public void Init() { }
    public void UnInit() { Shutdown(); }
    
    public SystemScope FrameUpdateScope => SystemScope.Framework;
    public void FrameUpdate(float deltaTime) {
        Update(deltaTime, Time.realtimeSinceStartup);
    }
    
    // ... 其他方法保持不变
}

// 3. 注册到 SystemRepository
// 在 FrameworkDefault.DoInit() 中
SystemRep.RegisterSystem<IPoolSystem>(CPoolManager.Instance);

// 4. 移除手动调用
// 删除 FrameworkBase.Update() 中的 CPoolManager.Instance.Update(...);
```

---

### 4. UI 系统（UISystemDefault）

**当前状态**：
```csharp
// IUISystem.cs
public interface IUISystem : ISystem {
    UIFormScript OpenForm(string prefabPath, UILogicBase logic, object param = null);
    // ... 其他方法
}

// UISystemDefault.cs
public class UISystemDefault : IUISystem, IFrameUpdate {
    public void Init() { ... }
    public void UnInit() { ... }
    
    public SystemScope FrameUpdateScope => SystemScope.Framework;
    public void FrameUpdate(float deltaTime) { ... }
    
    // ... 其他方法
}
```

**评价**：✅ 良好
- 接口定义完整
- 实现类正确实现接口
- 自动更新机制正确

**🟡 问题 7：未注册到 SystemRepository**
- **影响**：新体系无法使用 UI 系统
- **严重性**：中等（阻塞 UI 相关业务迁移）

**修复方案**：
```csharp
// 在 FrameworkDefault.DoInit() 中注册
SystemRep.RegisterSystem<IUISystem>(new UISystemDefault());
```

---

## 📊 问题汇总表

| 问题 ID | 系统 | 问题描述 | 优先级 | 工作量 |
|---|---|---|---|---|
| P1 | 资源系统 | 缺少 IResourceSystem 接口定义 | P0 | 10 分钟 |
| P2 | 资源系统 | 未注册到 SystemRepository | P0 | 5 分钟 |
| P3 | 事件系统 | 未实现 IEventSystem 接口 | P0 | 10 分钟 |
| P4 | 事件系统 | 需要手动调用 Update() | P0 | 5 分钟 |
| P5 | 对象池系统 | 缺少 IPoolSystem 接口定义 | P0 | 10 分钟 |
| P6 | 对象池系统 | 需要手动调用 Update() | P0 | 5 分钟 |
| P7 | UI 系统 | 未注册到 SystemRepository | P0 | 5 分钟 |

**总计**：7 个问题，预计修复时间 **50 分钟**

---

## 🎯 修复建议

### 立即修复（阶段 0）
所有 7 个问题必须在业务迁移前修复，建议按以下顺序：

1. **定义接口**（P1, P5）：20 分钟
   - 定义 `IResourceSystem` 接口
   - 定义 `IPoolSystem` 接口

2. **实现接口**（P3）：10 分钟
   - `CResourceManager` 实现 `IResourceSystem`
   - `CEventSystem` 实现 `IEventSystem` + `IFrameUpdate`
   - `CPoolManager` 实现 `IPoolSystem` + `IFrameUpdate`

3. **注册系统**（P2, P7）：10 分钟
   - 在 `FrameworkDefault.DoInit()` 中注册所有基建系统

4. **移除手动调用**（P4, P6）：10 分钟
   - 删除 `FrameworkBase` 中的手动 Update 调用
   - 验证自动更新机制正常工作

### 验证测试
修复完成后，编写测试验证：
```csharp
// 测试基建系统可访问
var resourceSystem = SystemRepository.Instance.GetSystem<IResourceSystem>();
var eventSystem = SystemRepository.Instance.GetSystem<IEventSystem>();
var poolSystem = SystemRepository.Instance.GetSystem<IPoolSystem>();
var uiSystem = SystemRepository.Instance.GetSystem<IUISystem>();

Assert.IsNotNull(resourceSystem);
Assert.IsNotNull(eventSystem);
Assert.IsNotNull(poolSystem);
Assert.IsNotNull(uiSystem);

// 测试事件系统自动更新
int callCount = 0;
eventSystem.Subscribe(1, () => callCount++);
eventSystem.Fire(1);
// 等待一帧
yield return null;
Assert.AreEqual(1, callCount);

// 测试对象池自动更新
// ... 类似测试
```

---

## 📝 审查结论

### 总体评价
新体系架构设计**优秀**，核心组件（SystemRepository, SystemUpdaterManager, FrameworkBase）设计合理，无重大缺陷。

### 主要问题
基建系统存在**接口定义缺失**和**注册遗漏**问题，导致新体系尚未完全可用。

### 修复优先级
所有问题均为 **P0 优先级**，必须在业务迁移前修复。预计修复时间 **50 分钟**（1 个会话）。

### 下一步行动
1. 执行阶段 0：基建修复（参见 `migration-master-plan.md`）
2. 编写基建验证测试
3. 验证通过后，开始阶段 1：EntityCreator 迁移

---

**审查人**：Claude Code  
**审查日期**：2026-05-16  
**文档版本**：1.0
