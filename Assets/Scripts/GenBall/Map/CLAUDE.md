# Map 模块

## 场景编辑器

### 可放置物类型

| 类别 | IsDynamic | 组件 | Prefab |
|------|-----------|------|--------|
| Enemy (敌人) | true | `EnemyUnitConfigBase` 子类 | 各类 Orbis prefab |
| SavePoint (存档点) | false | `SavePointConfig` | `SavePoint.prefab` |
| SceneTrigger (触发器) | true | `SceneTriggerConfig` | `Trigger.prefab` |
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
       ├─ List<SavePointData> savePoints
       │    └─ id, displayName, position, rotation
       ├─ List<EnemySpawnData> enemySpawns
       │    └─ id, enemyType, position, rotation, patrolRadius, detectRadius, aiBehavior
       ├─ List<SceneTriggerData> triggers
       │    └─ id, triggerName, eventName, position, radius, activationType
       └─ List<MechanismData> mechanisms
            └─ id, mechanismName, mechanismType, position, rotation, customDataJson
```

### 运行时消费示例

```csharp
// 获取场景配置
var configProvider = SystemRepository.Instance.GetSystem<IConfigProvider>();
var sceneConfig = configProvider.GetConfig<SceneConfigCollection>();

// 查找当前场景的配置
var sceneName = SceneManager.GetActiveScene().name;
var entry = sceneConfig.scenes.FirstOrDefault(s => s.sceneName == sceneName);

// 获取所有存档点（供传送系统用）
foreach (var sp in entry.savePoints)
{
    Debug.Log($"SavePoint {sp.id}: {sp.displayName} at {sp.position}");
}

// 获取敌人生成点
foreach (var es in entry.enemySpawns)
{
    // 用 EnemyPrefabRegistry 解析 prefab 路径
    if (EnemyPrefabRegistry.TryGetPath(es.enemyType, out var path))
    {
        var prefab = CResourceManager.Instance.LoadSync<GameObject>(path);
        Instantiate(prefab, es.position, es.rotation);
    }
}
```

### 编辑器工具

- **菜单**: `Tools/Map/Map Scene Editor`
- **烘焙管线**: `BakingPipeline.BakeCurrentScene()` — 校验 → 分配ID → 写入 SceneConfigCollection
- **类型发现**: `PlaceableTypeDiscovery.DiscoverAll()` — 反射扫描所有可放置类型
- **Scene View**: `PlaceableSceneGUI` — 彩色 Gizmo + Handle 编辑

### 敌人 Prefab 注册

`EnemyPrefabRegistry` 是运行时静态注册表，映射敌人类型名 → prefab 路径。

```csharp
// 注册新敌人类型（在系统 Init 中调用）
EnemyPrefabRegistry.Register("AcidOrbis", "Assets/AssetBundles/Common/Orbis/AcidOrbis/Prefab/AcidOrbis.prefab");

// 查找
if (EnemyPrefabRegistry.TryGetPath("AcidOrbis", out var path)) { ... }
```
