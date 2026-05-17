# 业务逻辑迁移总计划

> **文档目的**：制定从旧 IComponent 体系向新 ISystem 体系迁移业务逻辑的完整方案
> 
> **执行原则**：分阶段、可验证、可回滚、新旧共存
> 
> **最后更新**：2026-05-16

---

## 📋 执行摘要

### 当前状态
- ✅ **新框架基建**：已完成（SystemRepository, FrameworkBase, 四大基建系统）
- 🔄 **旧框架运行**：GameEntry + IComponent 体系保持运行
- 📊 **业务模块**：12 个 IComponent 模块待迁移

### 迁移目标
将所有业务逻辑从 GameEntry/IComponent 体系迁移到 SystemRepository/ISystem 体系，最终移除 GameEntry。

### 预计周期
- **总工作量**：约 8-12 个会话
- **关键路径**：EntityCreator → BuffSystem → 其他业务模块 → 移除 GameEntry

---

## 🔍 新体系设计审查

### 核心架构检查

#### ✅ 1. 单例系统
- **Singleton<T>**：纯 C# 单例，线程安全，懒加载
- **MonoSingleton<T>**：MonoBehaviour 单例（未使用）
- **状态**：设计合理，无漏洞

#### ✅ 2. 系统仓库（SystemRepository）
- **职责**：IoC 容器，管理系统注册/获取/注销
- **特性**：
  - 强制接口注册（警告非接口注册）
  - 自动调用 Init/UnInit
  - 自动注册到 SystemUpdaterManager
- **状态**：设计合理，无漏洞

#### ✅ 3. 更新管理器（SystemUpdaterManager）
- **职责**：统一调度系统更新，管理暂停逻辑
- **特性**：
  - 双轨更新器（Framework/Game）
  - 三种更新接口（ILogicUpdate/IFrameUpdate/ILateFrameUpdate）
  - SystemScope 控制暂停范围
- **状态**：设计合理，无漏洞

#### ✅ 4. 框架基类（FrameworkBase）
- **职责**：唯一 MonoBehaviour 入口，DontDestroyOnLoad
- **特性**：
  - 调度 SystemUpdaterManager 更新
  - 手动更新 CPoolManager 和 CEventSystem
  - 提供虚方法供子类扩展
- **状态**：设计合理，无漏洞

### 基建系统检查

#### ⚠️ 1. 资源系统（CResourceManager）
**问题**：
- ❌ **缺少 IResourceSystem 接口定义**（文件为空）
- ❌ **未注册到 SystemRepository**
- ✅ 使用 Helper 模式支持 Editor/AssetBundle 切换

**修复方案**：
```csharp
// 1. 定义 IResourceSystem 接口
public interface IResourceSystem : ISystem {
    void Load(string path, Action<object> onSuccess, Action<string> onFailed);
    T LoadSync<T>(string path) where T : UnityEngine.Object;
    void Unload(string path, bool unloadAll = false);
}

// 2. CResourceManager 实现 IResourceSystem
public class CResourceManager : Singleton<CResourceManager>, IResourceSystem

// 3. 在 FrameworkDefault 中注册
SystemRep.RegisterSystem<IResourceSystem>(CResourceManager.Instance);
```

#### ⚠️ 2. 事件系统（CEventSystem）
**问题**：
- ❌ **未实现 IEventSystem 接口**
- ❌ **未注册到 SystemRepository**
- ❌ **需要手动调用 Update()**（在 FrameworkBase.DoFrameUpdate 中）

**修复方案**：
```csharp
// 1. CEventSystem 实现 IEventSystem
public class CEventSystem : IEventSystem {
    public void Init() { }
    public void UnInit() { Clear(); }
    // ... 其他方法保持不变
}

// 2. 实现 IFrameUpdate 接口（自动调度）
public class CEventSystem : IEventSystem, IFrameUpdate {
    public SystemScope FrameUpdateScope => SystemScope.Framework;
    public void FrameUpdate(float deltaTime) {
        while (_pendingEvents.Count > 0)
            _pendingEvents.Dequeue().Invoke();
    }
}

// 3. 在 FrameworkDefault 中注册
SystemRep.RegisterSystem<IEventSystem>(new CEventSystem());
```

#### ⚠️ 3. 对象池系统（CPoolManager）
**问题**：
- ❌ **未定义 IPoolSystem 接口**
- ❌ **未注册到 SystemRepository**
- ❌ **需要手动调用 Update()**（在 FrameworkBase.Update 中）

**修复方案**：
```csharp
// 1. 定义 IPoolSystem 接口
public interface IPoolSystem : ISystem {
    IObjectPool<T> CreateSingleSpawnObjectPool<T>() where T : ObjectBase;
    // ... 其他方法
}

// 2. CPoolManager 实现 IPoolSystem + IFrameUpdate
public sealed partial class CPoolManager : Singleton<CPoolManager>, IPoolSystem, IFrameUpdate {
    public SystemScope FrameUpdateScope => SystemScope.Framework;
    public void FrameUpdate(float deltaTime) {
        Update(deltaTime, Time.realtimeSinceStartup);
    }
}

// 3. 在 FrameworkDefault 中注册
SystemRep.RegisterSystem<IPoolSystem>(CPoolManager.Instance);
```

#### ✅ 4. UI 系统（UISystemDefault）
**状态**：
- ✅ 已实现 IUISystem 接口
- ✅ 已实现 IFrameUpdate 接口
- ⚠️ **未注册到 SystemRepository**（需要在 FrameworkDefault 中注册）

**修复方案**：
```csharp
// 在 FrameworkDefault.DoInit() 中注册
SystemRep.RegisterSystem<IUISystem>(new UISystemDefault());
```

### 🔧 基建修复优先级

| 问题 | 优先级 | 工作量 | 阻塞项 |
|---|---|---|---|
| 定义 IResourceSystem 接口 | P0 | 10 分钟 | 所有业务模块 |
| CEventSystem 实现接口并注册 | P0 | 15 分钟 | 所有业务模块 |
| CPoolManager 实现接口并注册 | P0 | 15 分钟 | EntityCreator |
| UISystemDefault 注册 | P0 | 5 分钟 | UI 相关模块 |
| 移除手动 Update 调用 | P1 | 5 分钟 | - |

**总计**：约 50 分钟（1 个会话）

---

## 📊 业务模块清单

### 旧体系模块（IComponent）

| 模块 | 类型 | 依赖 | 复杂度 | 优先级 |
|---|---|---|---|---|
| `EntityCreator<T>` | 基础设施 | ObjectPoolManager | 高 | P0 |
| `BuffSystem` | 战斗核心 | EntityCreator, EventManager | 高 | P1 |
| `BulletSystem` | 战斗核心 | BuffSystem, EntityCreator | 中 | P2 |
| `DamageSystem` | 战斗核心 | BuffSystem | 低 | P2 |
| `DeathSystem` | 战斗核心 | BuffSystem | 低 | P2 |
| `TimelineSystem` | 战斗辅助 | - | 低 | P3 |
| `EvolutionSystem` | 战斗辅助 | - | 低 | P3 |
| `PlayerManager` | 游戏逻辑 | EntityCreator | 低 | P2 |
| `MapModule` | 游戏逻辑 | EntityCreator, EventManager | 中 | P3 |
| `SceneModule` | 游戏逻辑 | - | 低 | P3 |
| `SaveComponent` | 游戏逻辑 | - | 中 | P4 |
| `ExecuteComponent` | 启动流程 | FsmManager | 中 | P4 |

### 单例服务（已解耦）

以下单例服务不依赖 GameEntry，无需迁移：
- `DamageSystem.Instance`
- `DeathSystem.Instance`
- `TeleportSystem.Instance`
- `SceneSystem.Instance`
- `InteractSystem.Instance`
- `PauseManager.Instance`
- `GameManager.Instance`

---

## 🗺️ 迁移路线图

### 阶段 0：基建修复（必须先完成）

**目标**：修复新体系基建漏洞，确保基础设施完整

**任务清单**：
1. ✅ 定义 `IResourceSystem` 接口
2. ✅ `CResourceManager` 实现 `IResourceSystem`
3. ✅ `CEventSystem` 实现 `IEventSystem` + `IFrameUpdate`
4. ✅ `CPoolManager` 实现 `IPoolSystem` + `IFrameUpdate`
5. ✅ 在 `FrameworkDefault` 中注册所有基建系统
6. ✅ 移除 `FrameworkBase` 中的手动 Update 调用
7. ✅ 编写基建验证测试

**验收标准**：
- 所有基建系统通过 `SystemRepository.GetSystem<T>()` 可访问
- 事件系统和对象池自动更新（无需手动调用）
- 资源系统在 Editor 和 AssetBundle 模式下均可用

**预计时间**：1 个会话

---

### 阶段 1：EntityCreator 迁移

**目标**：将 EntityCreator 从 IComponent 迁移到独立系统

**背景**：
- EntityCreator 是对象池的高级封装，管理 IEntity 生命周期
- 被 PlayerManager, MapModule, BuffSystem, BulletSystem 依赖
- 当前有 8 个 EntityCreator 实例注册到 GameEntry

**迁移方案**：

#### 方案 A：EntityCreator 作为 ISystem（推荐）
```csharp
// 1. 定义接口
public interface IEntitySystem : ISystem {
    void RegisterCreator<T>(string name) where T : IEntity;
    T CreateEntity<T>(string name, Vector3 pos, Quaternion rot) where T : IEntity;
    void RecycleEntity(GameObject entity);
}

// 2. 实现类
public class EntitySystemDefault : IEntitySystem, IFrameUpdate, ILogicUpdate {
    private Dictionary<Type, EntityCreator<T>> _creators;
    // ... 实现
}

// 3. 注册
SystemRep.RegisterSystem<IEntitySystem>(new EntitySystemDefault());
```

#### 方案 B：保持 EntityCreator 独立（简单）
```csharp
// 1. EntityCreator 不继承 IComponent
public class EntityCreator<T> where T : IEntity {
    // 移除 IComponent 接口
    // 保持现有 API 不变
}

// 2. 手动管理生命周期
public class FrameworkDefault : FrameworkBase {
    private EntityCreator<CharacterState> _characterCreator;
    
    protected override void DoInit() {
        _characterCreator = new EntityCreator<CharacterState>();
        _characterCreator.Init();
    }
}

// 3. 通过静态访问器暴露
public static class EntityCreators {
    public static EntityCreator<CharacterState> Character { get; internal set; }
    public static EntityCreator<BulletState> Bullet { get; internal set; }
    // ...
}
```

**推荐**：方案 B（简单、低风险、保持现有 API）

**任务清单**：
1. ✅ 移除 EntityCreator 的 IComponent 接口
2. ✅ 创建 `EntityCreators` 静态访问器类
3. ✅ 在 `FrameworkDefault` 中初始化所有 EntityCreator
4. ✅ 替换所有 `GameEntry.GetModule<EntityCreator<T>>()` 为 `EntityCreators.Xxx`
5. ✅ 验证所有 Entity 创建/回收功能正常

**影响范围**：
- `PlayerManager.cs`
- `MapModule.cs`
- `BuffSystem.cs`
- `BulletSystem.cs`
- 所有创建 Entity 的代码

**验收标准**：
- 所有 Entity 创建/回收功能正常
- 无 GameEntry 依赖
- 性能无退化

**预计时间**：1-2 个会话

---

### 阶段 2：核心战斗系统迁移

**目标**：迁移 BuffSystem, BulletSystem, DamageSystem, DeathSystem

#### 2.1 BuffSystem 迁移

**当前状态**：
- 继承 MonoBehaviour + IComponent
- 依赖 GameEntry.Event（旧事件系统）
- 管理全局 Buff 生命周期

**迁移方案**：
```csharp
// 1. 定义接口
public interface IBuffSystem : ISystem, ILogicUpdate {
    BuffObj AddBuff(AddBuffInfo info);
    void RemoveBuff(BuffObj buffObj);
}

// 2. 实现类（不继承 MonoBehaviour）
public class BuffSystemDefault : IBuffSystem {
    public SystemScope LogicUpdateScope => SystemScope.Game;
    
    public void Init() { }
    public void UnInit() { /* 清理所有 Buff */ }
    public void LogicUpdate(float deltaTime) { /* Tick Buff */ }
    
    public BuffObj AddBuff(AddBuffInfo info) { /* 现有逻辑 */ }
    public void RemoveBuff(BuffObj buffObj) { /* 现有逻辑 */ }
}

// 3. 注册
SystemRep.RegisterSystem<IBuffSystem>(new BuffSystemDefault());
```

**任务清单**：
1. ✅ 定义 `IBuffSystem` 接口
2. ✅ 创建 `BuffSystemDefault` 实现类
3. ✅ 迁移 Buff 管理逻辑（AddBuff/RemoveBuff/Tick）
4. ✅ 替换事件系统调用（旧 → 新）
5. ✅ 替换所有 `GameEntry.Buff` 为 `SystemRepository.Instance.GetSystem<IBuffSystem>()`
6. ✅ 验证 Buff 触发器功能正常

**验收标准**：
- 所有 Buff 添加/移除/叠加/Tick 功能正常
- Buff 触发器正常工作
- 无 GameEntry 依赖

**预计时间**：1-2 个会话

#### 2.2 BulletSystem 迁移

**迁移方案**：
```csharp
public interface IBulletSystem : ISystem {
    void FireBullet(BulletLaunchInfo launchInfo);
}

public class BulletSystemDefault : IBulletSystem {
    // 迁移现有逻辑
}
```

**任务清单**：
1. ✅ 定义 `IBulletSystem` 接口
2. ✅ 创建 `BulletSystemDefault` 实现类
3. ✅ 迁移子弹发射逻辑
4. ✅ 替换所有 `GameEntry.Bullet` 调用
5. ✅ 验证子弹发射功能正常

**预计时间**：1 个会话

#### 2.3 DamageSystem & DeathSystem

**当前状态**：已经是单例，不依赖 GameEntry

**任务**：无需迁移，保持现状

---

### 阶段 3：游戏逻辑系统迁移

**目标**：迁移 PlayerManager, MapModule, SceneModule

#### 3.1 PlayerManager 迁移

**迁移方案**：
```csharp
public interface IPlayerSystem : ISystem {
    GameObject Player { get; }
    void CreatePlayer(Vector3 pos, Quaternion rot);
    void CreatePlayer(Transform spawnTransform);
}

public class PlayerSystemDefault : IPlayerSystem {
    // 迁移现有逻辑
}
```

**任务清单**：
1. ✅ 定义 `IPlayerSystem` 接口
2. ✅ 创建 `PlayerSystemDefault` 实现类
3. ✅ 迁移玩家创建逻辑
4. ✅ 替换所有 `GameEntry.Player` 调用
5. ✅ 验证玩家创建功能正常

**预计时间**：1 个会话

#### 3.2 MapModule 迁移

**迁移方案**：
```csharp
public interface IMapSystem : ISystem {
    void LoadSavePointAround(int savePointIndex);
    SavePointInfo GetSavePointInfo(int savePointIndex);
}

public class MapSystemDefault : IMapSystem {
    // 迁移现有逻辑
}
```

**任务清单**：
1. ✅ 定义 `IMapSystem` 接口
2. ✅ 创建 `MapSystemDefault` 实现类
3. ✅ 迁移地图块加载逻辑
4. ✅ 替换事件系统调用（旧 → 新）
5. ✅ 替换所有 `GameEntry.Map` 调用
6. ✅ 验证地图加载功能正常

**预计时间**：1-2 个会话

#### 3.3 SceneModule 迁移

**迁移方案**：
```csharp
public interface ISceneSystem : ISystem {
    void LoadSceneAsync(string sceneName, Action onComplete);
}

public class SceneSystemDefault : ISceneSystem {
    // 迁移现有逻辑
}
```

**任务清单**：
1. ✅ 定义 `ISceneSystem` 接口
2. ✅ 创建 `SceneSystemDefault` 实现类
3. ✅ 迁移场景加载逻辑
4. ✅ 替换所有 `GameEntry.Scene` 调用
5. ✅ 验证场景加载功能正常

**预计时间**：1 个会话

---

### 阶段 4：辅助系统迁移

**目标**：迁移 TimelineSystem, EvolutionSystem

**迁移方案**：
```csharp
public interface ITimelineSystem : ISystem {
    void PlayTimeline(TimelineAsset timeline, GameObject target);
}

public interface IEvolutionSystem : ISystem {
    void RegisterWeapon(WeaponState weapon);
    void AddKillPoints(WeaponState weapon, int points);
}
```

**任务清单**：
1. ✅ 定义接口
2. ✅ 创建实现类
3. ✅ 迁移逻辑
4. ✅ 替换所有 GameEntry 调用
5. ✅ 验证功能正常

**预计时间**：1 个会话

---

### 阶段 5：流程系统迁移

**目标**：迁移 ExecuteComponent, SaveComponent

**注意**：这两个模块复杂度较高，建议最后迁移

**迁移方案**：
```csharp
public interface ISaveSystem : ISystem {
    void Save(SaveData data);
    SaveData Load();
}

public interface IProcedureSystem : ISystem {
    void StartProcedure<T>() where T : ProcedureBase;
}
```

**任务清单**：
1. ✅ 定义接口
2. ✅ 创建实现类
3. ✅ 迁移逻辑
4. ✅ 替换所有 GameEntry 调用
5. ✅ 验证功能正常

**预计时间**：2-3 个会话

---

### 阶段 6：移除 GameEntry

**目标**：完全移除旧体系

**任务清单**：
1. ✅ 确认所有 `GameEntry.Xxx` 调用已替换
2. ✅ 移除 `GameEntry.cs`
3. ✅ 移除 `IComponent` 接口
4. ✅ 移除 `Entry.cs`
5. ✅ 移除旧事件系统（EventManager）
6. ✅ 移除旧 UI 系统（UIManager）
7. ✅ 移除旧资源系统（ResourceManager）
8. ✅ 移除旧对象池系统（ObjectPoolManager）
9. ✅ 清理所有 `using Yueyn.Main.IComponent` 引用
10. ✅ 全项目编译验证

**验收标准**：
- 项目无编译错误
- 所有功能正常运行
- 无 GameEntry 残留

**预计时间**：1 个会话

---

## 📝 迁移规范

### 命名约定

| 旧体系 | 新体系 | 示例 |
|---|---|---|
| `XxxManager` (IComponent) | `IXxxSystem` (接口) | `PlayerManager` → `IPlayerSystem` |
| `XxxModule` (IComponent) | `IXxxSystem` (接口) | `MapModule` → `IMapSystem` |
| `XxxComponent` (IComponent) | `IXxxSystem` (接口) | `SaveComponent` → `ISaveSystem` |
| - | `XxxSystemDefault` (实现) | - → `PlayerSystemDefault` |

### 接口设计原则

1. **最小接口**：只暴露必要的公共方法
2. **无状态暴露**：不暴露内部状态（除非必要）
3. **依赖注入**：通过构造函数或 Init 方法注入依赖
4. **生命周期**：Init/UnInit 管理资源

### 实现类设计原则

1. **不继承 MonoBehaviour**：除非必须（如需要 Coroutine）
2. **实现更新接口**：需要帧更新时实现 IFrameUpdate/ILogicUpdate
3. **指定 SystemScope**：Framework（不受暂停影响）或 Game（受暂停影响）
4. **线程安全**：如果可能被多线程访问，使用 lock

### 迁移检查清单

每个模块迁移完成后，检查：

- [ ] 定义了 `IXxxSystem` 接口
- [ ] 创建了 `XxxSystemDefault` 实现类
- [ ] 实现了 `ISystem.Init/UnInit`
- [ ] 如需更新，实现了 `IFrameUpdate/ILogicUpdate`
- [ ] 在 `FrameworkDefault` 中注册
- [ ] 替换了所有 `GameEntry.Xxx` 调用
- [ ] 移除了 `IComponent` 接口
- [ ] 编写了验证测试
- [ ] 更新了文档

---

## 🧪 验证策略

### 单元测试

为每个迁移的系统编写单元测试：

```csharp
[Test]
public void TestBuffSystemAddBuff() {
    var buffSystem = SystemRepository.Instance.GetSystem<IBuffSystem>();
    var info = AddBuffInfo.Create(/* ... */);
    var buff = buffSystem.AddBuff(info);
    Assert.IsNotNull(buff);
}
```

### 集成测试

在测试场景中验证：
1. 创建玩家
2. 生成敌人
3. 发射子弹
4. 触发伤害
5. 添加 Buff
6. 打开 UI

### 性能测试

对比迁移前后性能：
- 帧率（FPS）
- 内存占用
- GC 频率

---

## 📚 文档更新

每个阶段完成后，更新以下文档：

1. **CLAUDE.md**：更新系统访问方式
2. **architecture.md**：更新架构图
3. **systems-overview.md**：更新系统列表
4. **code-patterns.md**：更新代码示例
5. **refactoring-plan.md**：更新进度

---

## 🚨 风险与应对

### 风险 1：EntityCreator 迁移失败
**影响**：阻塞所有后续迁移
**应对**：优先验证，保留回滚方案

### 风险 2：Buff 触发器失效
**影响**：战斗系统崩溃
**应对**：编写完整测试，逐个验证触发器

### 风险 3：事件系统不兼容
**影响**：跨模块通信失败
**应对**：保持新旧事件系统共存，逐步迁移

### 风险 4：性能退化
**影响**：游戏卡顿
**应对**：每个阶段进行性能测试，及时优化

---

## 📅 时间估算

| 阶段 | 任务 | 预计时间 | 累计时间 |
|---|---|---|---|
| 0 | 基建修复 | 1 会话 | 1 会话 |
| 1 | EntityCreator 迁移 | 1-2 会话 | 2-3 会话 |
| 2 | 核心战斗系统迁移 | 2-3 会话 | 4-6 会话 |
| 3 | 游戏逻辑系统迁移 | 2-3 会话 | 6-9 会话 |
| 4 | 辅助系统迁移 | 1 会话 | 7-10 会话 |
| 5 | 流程系统迁移 | 2-3 会话 | 9-13 会话 |
| 6 | 移除 GameEntry | 1 会话 | 10-14 会话 |

**总计**：10-14 个会话（约 2-3 周）

---

## 🎯 下一步行动

### 立即执行（阶段 0）

1. 修复 `IResourceSystem` 接口定义
2. `CEventSystem` 实现 `IEventSystem` + `IFrameUpdate`
3. `CPoolManager` 实现 `IPoolSystem` + `IFrameUpdate`
4. 在 `FrameworkDefault` 中注册所有基建系统
5. 编写基建验证测试

### 准备工作

1. 创建迁移分支：`git checkout -b feature/migrate-to-new-framework`
2. 备份当前代码：`git tag backup-before-migration`
3. 准备测试场景：创建 `MigrationTest.unity`

---

## 📖 参考文档

- `.codebuddy/refactoring-plan.md` - 框架重构计划
- `.codebuddy/rules/module-migration.md` - 模块迁移准则
- `.codebuddy/rules/system-architecture.md` - 系统架构双轨体系
- `.claude/docs/architecture.md` - 架构指南
- `.claude/docs/event-system-guide.md` - 事件系统指南

---

**文档维护者**：Claude Code  
**最后审查**：2026-05-16  
**下次审查**：每个阶段完成后
