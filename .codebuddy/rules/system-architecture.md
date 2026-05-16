---
description: 新旧双轨体系识别与使用规范
alwaysApply: true
enabled: true
updatedAt: 2026-05-16T00:00:00.000Z
---

# 系统架构双轨体系

## 触发条件
创建系统模块 / 路径匹配 `**/Yueyn/**/*.cs` 或 `**/GenBall/**/*.cs`

## 核心规则

### 新体系（ISystem）
- 继承 `ISystem` 接口
- 注册：`SystemRepository.Instance.RegisterSystem<IInterface>(instance)`
- ❌ 禁止继承 `MonoBehaviour`
- 需要帧更新：继承 `IFrameUpdate` / `ILogicUpdate` / `ILateFrameUpdate`

### 旧体系（IComponent）
- 访问：`GameEntry.GetModule<T>()`
- ❌ 禁止修改旧体系代码

### 资源加载切换
- 使用 `#if UNITY_EDITOR` 编译宏
- ❌ 禁止运行时判断 `Application.isEditor`

### 代码生成文件（禁止修改）
- `GlobalEventSystem.Generated.cs`
- `EffectEvents.Generated.cs`
- 所有 `.Bind.cs` 文件

## 已迁移模块
Resource ✅ / UI ✅ / Event ✅ / Pool ✅
