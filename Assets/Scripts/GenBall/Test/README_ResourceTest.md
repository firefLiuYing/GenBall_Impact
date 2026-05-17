# 资源管理器测试指南

## 测试目的
验证新体系的 `CResourceManager` 资源加载功能是否正常工作。

## 测试步骤

### 1. 创建测试预制体

1. 在 Unity 场景中创建一个空物体
2. 添加 `CreateTestPrefab` 组件
3. 在 Inspector 中右键点击组件，选择 `Create Test Cube Prefab`
4. 等待控制台输出 "✅ 测试预制体已创建"
5. 检查 `Assets/Prefabs/TestCube.prefab` 是否已创建

### 2. 运行测试

1. 在场景中创建一个空物体，命名为 `ResourceManagerTest`
2. 添加 `TestResourceManager` 组件
3. 在 Inspector 中配置：
   - `Test Prefab Path`: `Assets/Prefabs/TestCube.prefab`
   - `Test Sync Load`: ✅ 勾选
   - `Test Async Load`: ✅ 勾选
4. 运行场景

### 3. 预期结果

**控制台输出：**
```
=== 开始测试 CResourceManager ===
[TestResourceManager] 测试同步加载...
[TestResourceManager] ✅ 同步加载成功: TestCube
[TestResourceManager] ✅ 实例化成功: SyncLoadedInstance
[TestResourceManager] 测试异步加载...
[TestResourceManager] 加载进度: 0.0%
[TestResourceManager] 加载进度: 100.0%
[TestResourceManager] ✅ 异步加载成功: TestCube
[TestResourceManager] ✅ 实例化成功: AsyncLoadedInstance
[TestCubeComponent] SyncLoadedInstance 已实例化
[TestCubeComponent] AsyncLoadedInstance 已实例化
```

**场景中：**
- 左侧（-2, 0, 0）出现一个旋转的 Cube（同步加载）
- 右侧（2, 0, 0）出现一个旋转的 Cube（异步加载）

### 4. 额外测试（右键菜单）

在 `TestResourceManager` 组件上右键，可以执行：

- **Test Unload**: 测试卸载资源
- **Test Multiple Load**: 测试多次加载同一资源（验证缓存）
- **Test Load Non-Existent**: 测试加载不存在的资源（验证错误处理）

## 测试覆盖

- ✅ 同步加载 (`LoadSync`)
- ✅ 异步加载 (`Load`)
- ✅ 加载进度回调 (`onProgress`)
- ✅ 加载成功回调 (`onLoadSuccess`)
- ✅ 加载失败回调 (`onLoadFailed`)
- ✅ 资源卸载 (`Unload`)
- ✅ 错误处理（不存在的资源）
- ✅ 多次加载同一资源

## 注意事项

1. **编辑器模式**：当前使用 `ResourceHelperEditor`，通过 `AssetDatabase.LoadAssetAtPath` 加载
2. **运行时模式**：打包后会使用 `ResourceHelperAssetBundle`，需要先打包 AssetBundle
3. **路径格式**：必须使用完整路径，如 `Assets/Prefabs/TestCube.prefab`

## 故障排查

### 问题：加载失败
- 检查预制体路径是否正确
- 检查预制体是否存在于 `Assets/Prefabs/` 目录
- 查看控制台的详细错误信息

### 问题：编译错误
- 等待 Unity 完成编译
- 尝试 `Assets` → `Refresh`
- 重启 Unity 编辑器

### 问题：异步加载没有回调
- 检查是否在运行模式下测试
- 检查协程运行器是否正常创建
