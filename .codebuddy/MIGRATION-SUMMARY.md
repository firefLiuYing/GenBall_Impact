# 新体系审查与迁移计划总结

> **审查日期**：2026-05-16
> 
> **审查结论**：✅ 新体系设计合理，发现 4 个需修复的基建问题
> 
> **下一步**：修复基建问题 → 执行 8 阶段业务迁移

---

## 📊 审查结论

### 核心架构：✅ 优秀
- **Singleton<T>**：线程安全，懒加载，设计完善
- **SystemRepository**：IoC 容器，职责单一，自动生命周期管理
- **SystemUpdaterManager**：双轨更新器（Framework/Game），暂停逻辑清晰
- **FrameworkBase**：唯一 MonoBehaviour 入口，DontDestroyOnLoad

**结论**：核心架构无重大缺陷，可直接用于业务迁移。

---

### 基建系统：⚠️ 需修复

| 系统 | 问题 | 优先级 | 工作量 |
|---|---|---|---|
| 资源系统 | IResourceSystem 接口缺失，未注册 | P0 | 15 分钟 |
| 事件系统 | 未实现 IEventSystem，未注册 | P0 | 15 分钟 |
| 对象池系统 | IPoolSystem 接口缺失，未注册 | P0 | 15 分钟 |
| UI 系统 | 未注册到 SystemRepository | P0 | 5 分钟 |

**总计**：约 50 分钟（1 个会话）

**结论**：基建系统功能完整，但未完全接入新体系，需补充接口定义和注册逻辑。

---

## 🗺️ 迁移计划

### 总体策略
**渐进式迁移**：新旧共存 → 逐步迁移 → 验证后删除

### 8 个阶段

| 阶段 | 目标 | 工作量 | 依赖 |
|---|---|---|---|
| 0. 基建修复 | 修复 4 个基建问题 | 1 会话 | - |
| 1. EntityCreator | 迁移对象池高级封装 | 2 会话 | 阶段 0 |
| 2. BuffSystem | 迁移战斗核心系统 | 1 会话 | 阶段 1 |
| 3. 战斗系统 | 迁移 Bullet/Damage/Death | 1 会话 | 阶段 2 |
| 4. 游戏逻辑 | 迁移 Player/Map/Scene | 2 会话 | 阶段 1 |
| 5. 辅助系统 | 迁移 Timeline/Evolution | 1 会话 | 阶段 2 |
| 6. 启动流程 | 迁移 Execute/Save | 1 会话 | 阶段 4 |
| 7. GameEntry 移除 | 删除旧体系 | 1 会话 | 阶段 1-6 |
| 8. 文档更新 | 更新所有文档 | 1 会话 | 阶段 7 |

**总计**：约 11 个会话

---

## 📋 关键文档

### 规划文档
- **`migration-master-plan.md`**：完整迁移计划（8 阶段详细方案）
- **`migration-checklist.md`**：执行检查清单（快速参考）
- **`design-review.md`**：设计审查报告（问题详情）

### 现有文档
- **`refactoring-plan.md`**：框架重构进度追踪
- **`.claude/rules/code-modification-rules.md`**：代码修改规则
- **`CLAUDE.md`**：项目总览（需在阶段 8 更新）

---

## 🚀 下一步行动

### 立即执行
**阶段 0：基建修复**（约 1 个会话）

1. 定义 `IResourceSystem` 接口
2. `CResourceManager` 实现 `IResourceSystem`
3. `CEventSystem` 实现 `IEventSystem` + `IFrameUpdate`
4. `CPoolManager` 实现 `IPoolSystem` + `IFrameUpdate`
5. 在 `FrameworkDefault` 中注册所有基建系统
6. 移除 `FrameworkBase` 中的手动 Update 调用
7. 编写基建验证测试

### 验收标准
- 所有基建系统可通过 `SystemRepository.GetSystem<T>()` 访问
- 事件系统和对象池自动更新（无需手动调用）
- 资源系统在 Editor 和 AssetBundle 模式下均可用

### 完成后
开始**阶段 1：EntityCreator 迁移**

---

## 💡 设计亮点

### 1. 双轨更新器
Framework 更新器不受暂停影响，保证基建系统（UI、事件）持续运行，避免暂停时 UI 卡死。

### 2. SystemScope 枚举
```csharp
public enum SystemScope { Framework, Game }
```
让系统自主声明是否受暂停影响，而非硬编码判断。

### 3. 接口先行
强制面向接口注册，支持运行时替换实现（如 ResourceSystemEditor ↔ ResourceSystemAssetBundle）。

### 4. 自动生命周期管理
SystemRepository 自动调用 Init/UnInit，自动注册到 SystemUpdaterManager，减少样板代码。

---

## ⚠️ 注意事项

### 迁移过程中
1. **保持旧体系运行**：所有迁移必须保证旧代码可用
2. **新旧共存验证**：每个阶段完成后必须测试新旧系统共存
3. **分阶段提交**：每个阶段完成后提交代码，便于回滚
4. **文档同步更新**：代码变更后立即更新相关文档

### 风险点
1. **EntityCreator 迁移**：影响面最大，需谨慎设计
2. **BuffSystem 迁移**：战斗核心，需充分测试
3. **启动流程迁移**：涉及 FSM，可能需要重构

---

## 📞 跨会话恢复指南

### 如何快速恢复上下文
1. 查看 `migration-checklist.md` → 找到"当前阶段"
2. 阅读 `migration-master-plan.md` 对应章节 → 了解详细方案
3. 检查 `refactoring-plan.md` → 确认整体进度
4. 开始执行 → 逐项勾选 checklist

### 如何报告进度
1. 更新 `migration-checklist.md` 中的勾选框
2. 更新"当前阶段"和"已完成阶段"
3. 如有阻塞问题，记录到"阻塞问题"
4. 提交代码时引用对应阶段编号（如 `[Stage-0] Fix infrastructure issues`）

---

**最后更新**：2026-05-16
