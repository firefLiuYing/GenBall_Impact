---
description: 命名约定与代码组织规范
alwaysApply: true
enabled: true
updatedAt: 2026-05-16T00:00:00.000Z
---

# 命名约定

## 触发条件
创建新文件 / 重命名 / 用户提到"命名"、"规范"

## 核心规则

### UI 框架（必须）
- Logic 层：`XxxLogic.cs`
- View 层：`XxxView.cs`
- ❌ 禁止旧命名：`FormName.cs` + `FormNameVm.cs`

### 系统模块（必须）
- 接口：`IXxxSystem`
- 默认实现：`XxxSystemDefault`
- 特定实现：`XxxSystemEditor` / `XxxSystemAssetBundle`

### 代码生成文件（禁止修改）
- `.Generated.cs` / `.Bind.cs` 后缀文件

### 测试脚本
- 放在 `GenBall/Tests/` 目录
- 命名：`TestXxx.cs`（单个 MonoBehaviour）
