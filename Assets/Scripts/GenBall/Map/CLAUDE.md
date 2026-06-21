# Map 模块

## 场景编辑器

### 可放置物类型

| 类别 | IsDynamic | 组件 | Prefab |
|------|-----------|------|--------|
| Enemy (敌人) | true | `EnemyUnitConfigBase` 子类 | 各类 Orbis prefab |
| SavePoint (存档点/篝火) | true | `SavePointConfig` | `SavePoint.prefab` (placeholder) |
| SceneTrigger (触发器) | true | `TriggerVolume` | `Trigger.prefab` |
| Mechanism (机关) | false | `MechanismConfig` | 各机关 prefab |

新增类型：创建 MonoBehaviour，实现 `IScenePlaceable`，加 `[PlaceableCategory]` 属性即可。
编辑器通过反射自动发现。

### 配置资产

- **资产路径**: `Assets/Resources/Configs/SceneConfigCollection.asset`
- **类型**: `SceneConfigCollection : ScriptableObject`
- **运行时加载**: `SystemRepository.Instance.GetSystem<IConfigProvider>().GetConfig<SceneConfigCollection>()`
- **注册位置**: `AppConfigManager.Init()`

### 数据模型

```
SceneConfigCollection
  └─ List<SceneConfigEntry> scenes
       ├─ string sceneName
       ├─ string displayName
       ├─ int defaultSavePointId
       ├─ List<SavePointData> savePoints
       │    └─ id, displayName, position, rotation, bonfireType, initiallyActive, bonfirePosition, bonfireRotation
       ├─ List<EnemySpawnData> enemySpawns
       │    └─ id, enemyType, position, rotation, patrolRadius, detectRadius, aiBehavior
       ├─ List<SceneTriggerData> triggers
       │    └─ id, triggerName, eventId, paramTypeName, serializedParams, position, radius, activationType, triggerMode, triggerBehavior, maxFireCount, cooldownSeconds, listenerEventId, layerMask
       └─ List<MechanismData> mechanisms
            └─ id, mechanismName, mechanismType, position, rotation, customDataJson
```

### 运行时消费示例

```csharp
var sceneConfig = SystemRepository.Instance.GetSystem<IConfigProvider>().GetConfig<SceneConfigCollection>();
var sceneName = SceneManager.GetActiveScene().name;
var entry = sceneConfig.scenes.FirstOrDefault(s => s.sceneName == sceneName);

// 场景默认出生存档点（第一个进入 / 死亡复活时）
var defaultSpawn = entry.savePoints.FirstOrDefault(sp => sp.id == entry.defaultSavePointId);
var spawnPos = defaultSpawn?.position ?? Vector3.zero;

// 篝火生成：bonfireType 非空即为篝火，通过 BonfirePrefabRegistry 查 prefab
foreach (var sp in entry.savePoints.Where(sp => !string.IsNullOrEmpty(sp.bonfireType)))
{
    if (BonfirePrefabRegistry.TryGetPath(sp.bonfireType, out var path))
    {
        var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
        Instantiate(prefab, sp.bonfirePosition, sp.bonfireRotation);
    }
}
```

### 编辑器工具

- **窗口**: `Tools/Map Scene Editor` — 可放置物树、属性面板、Scene Settings（展示名 + 默认存档点下拉+定位按钮）
- **独立烘焙**: `Tools/Bake Current Scene` — 不打开窗口直接烘焙
- **烘焙管线**: `BakingPipeline.BakeCurrentScene()` — 校验 → 稳定分配ID（已有ID保留不重排，新增取max+1，冲突检测）→ 写入 SceneConfigCollection → 更新 GO 名称
- **类型发现**: `PlaceableTypeDiscovery.DiscoverAll()` — 反射扫描所有可放置类型
- **Scene View**: `PlaceableSceneGUI` — 彩色 Gizmo + Handle 编辑；存档点额外画 PlayerSpawnPoint 十字 + 虚线 + 篝火图标
- **Scene Settings**: displayName 和 defaultSavePointId 存于 EditorPrefs，不依赖场景中 GO。有 SceneConfig 组件时 displayName 以其为初始值。

### 存档点 / 篝火

存档点统一为一个 Category，通过 `bonfireType` 区分：
- **空字符串** = 纯锚点（仅定义 Player 出生位置，不可见不可交互）
- **非空** = 篝火（可见可交互，模型通过 `BonfirePrefabRegistry` 查找）

`initiallyActive` 控制场景初始化是否生成；`false` 的篝火需事件解锁（解锁后 ID 进入持久化的 `UnlockedSavePoints`）。

`[SavePointReference]` 属性可用于 int 字段，Inspector 显示下拉选择存档点并支持 PingObject 定位。`crossScene: true` 支持跨场景引用（仅下拉）。

### Prefab 注册表

| 注册表 | 映射 |
|--------|------|
| `EnemyPrefabRegistry` | 敌人类型名 → prefab 路径 |
| `BonfirePrefabRegistry` | 篝火类型名 → prefab 路径 |

```csharp
// 注册（在系统 Init 中调用）
EnemyPrefabRegistry.Register("AcidOrbis", "Assets/AssetBundles/Common/Orbis/AcidOrbis/Prefab/AcidOrbis.prefab");
BonfirePrefabRegistry.Register("Default", "Assets/AssetBundles/Common/Bonfire/Default/Prefab/Default.prefab");

// 查找
if (EnemyPrefabRegistry.TryGetPath("AcidOrbis", out var path)) { ... }
foreach (var t in BonfirePrefabRegistry.RegisteredTypes) { ... }
```

### 触发器系统

#### 编辑器端

- `TriggerVolume` — 场景放置组件，实现 `IScenePlaceable`，通过单个 `EventAdapter` 字段配置事件列表（EventAdapter 自身为容器，类似 UnityEvent，支持多事件依次触发）
- `TriggerVolumeEditor` — Custom Editor，事件区域使用 `PropertyField` 委托给 `EventAdapterDrawer`（紧凑行内列表，每行 [Event下拉] [参数] [-]）
- `EventParameterBase` — 多态参数基类（`[SerializeReference]`），子类用 `[EventParamHint]` 关联事件 ID
- 参数子类：`SpawnEnemyParams(6001)` / `OpenDoorParams(6002)` / `PlayDialogueParams(6003)` / `GrantAccessoryParams(6004)` / `UnlockSavePointParams(6005)`
- `TriggerMode`：`Collision` / `Interact` / `EventListener`；`TriggerBehavior`：`Once` / `Repeatable` / `Limited`

#### 烘焙管线

- `BakingPipeline.BakeCurrentScene()` 扫描 `IScenePlaceable`，调用 `BakeToConfigData()` 序列化到 `SceneConfigCollection.asset`
- `EventAdapter` 序列化为 `paramTypeName` + `serializedParams`（JsonUtility）

#### 事件 ID 体系

- `GlobalEventId` 枚举：框架事件 1-5999（按模块分段：Launch/Player/Input/Weapon/Enemy/System）
- `PlacedEventTable`（`Assets/Resources/Configs/PlacedEventTable.asset`）：投放类事件 >=6000，CSV 导入，自动冲突校验
- `GlobalEventCodeGenerator` 生成类型安全的事件包装器到 `Generated/GlobalEventSystem.Generated.cs`

#### 运行时

- `SceneExecutorSystemDefault` 在场景初始化时调用 `SpawnTriggers()` + `RegisterOrchestrator()`
- `RuntimeEventTrigger` 根据 `TriggerMode` 自动设置碰撞体/交互代理/事件订阅，触发时应用冷却+次数限制
- `RuntimeInteractProxy` — `IInteractable` 桥接，在 Interact 模式下注册到 `IInteractSystem`
- `SceneEventOrchestrator` — 事件→行为映射（当前仅 `SpawnEnemy(6001)` 已实现，其余 handler 待依赖系统就绪后添加）

#### 运行顺序

- LoadEnemyUnit → InGameUIReady → CreatePlayer → **SpawnTriggers → RegisterOrchestrator → SpawnBonfires → CleanupDynamicPlaceables** → SceneReady
