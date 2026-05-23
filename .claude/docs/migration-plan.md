# GenBall 业务系统迁移到 ISystem 框架

## Context

Framework 脚手架（ISystem/SystemRepository/SystemUpdaterManager/FrameworkBase）已完成。
基建系统（Event/Resource/UI/Pool）作为框架底盘硬编码在 FrameworkBase 中，不需要 ISystem 接口。
现在需要将旧的 IComponent 业务系统迁移到 ISystem 规范，同时优化实现。

## 当前状态（2026-05-23）

- **Phase 0 已完成**：IConfigProvider + AppConfigManager ✓
- **Phase 0 已删除**：ITimerService + TimerSystem — 纯透传包装，Timer 静态类本身够用，不需要 ISystem 壳
- **Phase 1 已完成**：ISaveService + SaveSystem（保留，待重新设计后移入 Yueyn 框架层）
- **Phase 1 已删除**：ISceneService + SceneService, IUIService + UIService — 无调用方死代码
- **Phase 2-4**：未开始
- **测试基建已完成**：TestsAutoRunner（文件触发编译 + 测试）、run_editmode_tests.sh（Editor/batch 双模式）

### 当前已知问题（自动编译）

Unity Editor 的 FileSystemWatcher 检测外部文件写入不可靠。解决方案：
- 如果编译触发没反应 → `powershell` 聚焦 Unity 窗口并发送 Ctrl+R 快捷键强制刷新
- 测试触发前自动调用 `AssetDatabase.Refresh()` 确保最新代码已编译
- 参见 `.claude/docs/testing-workflow.md`

## 设计决策

### 配置管理方案：IConfigProvider ISystem

- 创建 `IConfigProvider` ISystem，作为所有配置的唯一入口
- `AppConfigManager` 实现，Init 时通过 `Resources.Load<T>()` 加载所有 ScriptableObject
- 新建 `AppSettingsConfig` ScriptableObject 收纳所有前 `[SerializeField]` 值
- Transform 引用（uiRoot/mapRoot/spawnPoint）改为运行时按约定查找或从配置驱动
- 所有 ScriptableObject 移到 `Assets/Resources/Configs/` 确保构建可用
- 迁移后删除所有 `static class ConfigProvider`

### ISingleton 处理：按需迁移

只有需要 ISystem 生命周期/更新的才迁移，纯服务可保留原样。

### Yueyn 框架层 IComponent：不处理

TimerManager 是死代码，其他是已迁移残留，不纳入本次范围。

## 分阶段计划

### Phase 2：游戏流程系统

**系统**：InteractSystem, SceneSystem, TeleportSystem, PauseManager, GameManager, ExecuteComponent, GameSceneExecuteModule, FsmManager 适配

接口规划：
- `IInteractSystem` — 交互系统
- `ISceneStateSystem` — 场景状态
- `ITeleportSystem` — 传送
- `IPauseSystem` — 暂停（调用 `SystemUpdaterManager.Pause()/Resume()`）
- `IGameStateSystem` — 游戏状态
- `IExecuteSystem` — 执行组件
- `IGameSceneExecutor` — 游戏场景执行器
- `IFsmSystem` — 状态机

关键变更：
- PauseManager 改为直接调用 `SystemUpdaterManager.Pause()/Resume()` 而非 `GameEntry.Event`
- EntityCreator 包装为 `IEntityCreator<T> : ISystem` 注册
- EventManager 作为过渡保留，通过 SystemRepository 访问

### Phase 3：实体管理系统

**系统**：PlayerManager, MapModule, BulletSystem, EvolutionSystem

接口规划：
- `IPlayerSystem` — 玩家管理
- `IMapSystem` — 地图模块
- `IBulletSystem` — 子弹系统
- `IEvolutionSystem` — 进化系统

关键变更：
- EntityCreator 引用改为 `SystemRepository.GetSystem<IEntityCreator<T>>().Creator`

### Phase 4：战斗系统重新设计（待讨论）

**系统**：BuffSystem, DamageSystem, DeathSystem, TimelineSystem

- 不照搬现有实现，先讨论以下问题再动手：
  1. BuffSystem 是否拆分为注册服务 + 独立 Tick 系统（单一职责）
  2. 伤害管线是否改为职责链/事件驱动模式
  3. IBuffContainer 是否预注册而非每次 GetComponent 查询
  4. PauseState 标志位是否简化（ISystem 的 SystemScope 已自动处理暂停）
  5. TimelineSystem 是否需要序列化/并行片段/事件触发支持

## 关键文件

### 需修改
- `Assets/Scripts/GenBall/Framework/FrameworkDefault.cs` — 核心注册入口
- `Assets/Scripts/Yueyn/Main/FrameworkBase.cs` — CoroutineRunner 已添加

### 需新建（约 30 个文件）
接口 + 实现各一对，按 Phase 分批创建。

### 完成后删除
- `GameEntry.cs` 及所有 partial、`GameEntry.Register.cs`、EntityRegister 目录
- `ISingleton.cs`、`SingletonManager.cs`、`IComponent.cs`、`Entry.cs`
- 所有 `static class ConfigProvider`

## 验证方式

1. 每 Phase 完成后 Unity Editor 编译通过
2. 启动 Launcher 场景，确认系统初始化日志正常
3. 运行完整游戏流程（启动→开始游戏→场景加载→战斗→存档→读档）
4. 验证 PauseManager 暂停后战斗系统停止更新（SystemUpdaterManager 自动处理）
5. 每次修改代码后运行 `bash run_editmode_tests.sh --compile` 验证测试通过
