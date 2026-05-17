# 迁移执行检查清单

> **快速参考**：每个阶段开始前检查此清单，完成后打勾
> 
> **详细计划**：参见 `migration-master-plan.md`

---

## 阶段 0：基建修复 ⬜

**目标**：修复新体系基建漏洞

- [ ] 定义 `IResourceSystem` 接口
- [ ] `CResourceManager` 实现 `IResourceSystem`
- [ ] `CEventSystem` 实现 `IEventSystem` + `IFrameUpdate`
- [ ] `CPoolManager` 实现 `IPoolSystem` + `IFrameUpdate`
- [ ] 在 `FrameworkDefault` 中注册所有基建系统
- [ ] 移除 `FrameworkBase` 中的手动 Update 调用
- [ ] 编写基建验证测试

**验收**：所有基建系统可通过 `SystemRepository.GetSystem<T>()` 访问

---

## 阶段 1：EntityCreator 迁移 ⬜

**目标**：将 EntityCreator 从 IComponent 迁移到 ISystem

- [ ] 设计 EntityCreator 迁移方案（方案 A/B/C）
- [ ] 实现新 EntityCreator 系统
- [ ] 注册到 SystemRepository
- [ ] 编写迁移测试
- [ ] 验证旧 EntityCreator 仍可用

**验收**：新旧 EntityCreator 共存，测试通过

---

## 阶段 2：BuffSystem 迁移 ⬜

**目标**：将 BuffSystem 从 IComponent 迁移到 ISystem

- [ ] 定义 `IBuffSystem` 接口
- [ ] `BuffSystem` 实现 `IBuffSystem` + `ILogicUpdate`
- [ ] 移除 MonoBehaviour 继承
- [ ] 注册到 SystemRepository
- [ ] 更新所有 `GameEntry.Buff` 调用点
- [ ] 编写迁移测试

**验收**：BuffSystem 通过新体系访问，功能正常

---

## 阶段 3：战斗系统迁移 ⬜

**目标**：迁移 BulletSystem, DamageSystem, DeathSystem

### BulletSystem
- [ ] 定义 `IBulletSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

### DamageSystem（单例）
- [ ] 验证无 GameEntry 依赖
- [ ] 标记为"无需迁移"

### DeathSystem（单例）
- [ ] 验证无 GameEntry 依赖
- [ ] 标记为"无需迁移"

**验收**：战斗系统功能正常

---

## 阶段 4：游戏逻辑迁移 ⬜

**目标**：迁移 PlayerManager, MapModule, SceneModule

### PlayerManager
- [ ] 定义 `IPlayerSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

### MapModule
- [ ] 定义 `IMapSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

### SceneModule
- [ ] 定义 `ISceneSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

**验收**：游戏逻辑功能正常

---

## 阶段 5：辅助系统迁移 ⬜

**目标**：迁移 TimelineSystem, EvolutionSystem

### TimelineSystem
- [ ] 定义 `ITimelineSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

### EvolutionSystem
- [ ] 定义 `IEvolutionSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

**验收**：辅助系统功能正常

---

## 阶段 6：启动流程迁移 ⬜

**目标**：迁移 ExecuteComponent, SaveComponent

### ExecuteComponent
- [ ] 分析启动流程依赖
- [ ] 设计新启动流程
- [ ] 实现并验证

### SaveComponent
- [ ] 定义 `ISaveSystem` 接口
- [ ] 实现并注册
- [ ] 更新调用点

**验收**：游戏可正常启动和存档

---

## 阶段 7：GameEntry 移除 ⬜

**目标**：移除 GameEntry 和 IComponent 体系

- [ ] 确认所有 `GameEntry.XXX` 调用已迁移
- [ ] 搜索 `GameEntry.GetModule` 调用（应为 0）
- [ ] 搜索 `IComponent` 实现（应仅剩旧代码）
- [ ] 删除 `GameEntry.cs`
- [ ] 删除 `Entry.cs`
- [ ] 删除 `IComponent.cs`
- [ ] 删除旧 EventManager（代码生成）
- [ ] 删除旧 UIManager（MVVM）
- [ ] 删除旧 ResourceManager
- [ ] 删除旧 ObjectPoolManager
- [ ] 全量测试

**验收**：游戏完全运行在新体系上，无编译错误

---

## 阶段 8：文档更新 ⬜

**目标**：更新所有文档和示例

- [ ] 更新 `CLAUDE.md`
- [ ] 更新 `.claude/docs/architecture.md`
- [ ] 更新 `.claude/docs/framework-reference.md`
- [ ] 更新 `.claude/docs/code-patterns.md`
- [ ] 删除 `.claude/docs/migration-guide.md`（已完成迁移）
- [ ] 更新 `.codebuddy/refactoring-plan.md`（标记完成）
- [ ] 编写迁移总结文档

**验收**：文档与代码一致

---

## 快速状态查询

### 当前阶段
⬜ 阶段 0（基建修复）

### 已完成阶段
无

### 阻塞问题
无

### 下一步行动
执行阶段 0：基建修复

---

## 使用说明

1. **开始新阶段前**：阅读 `migration-master-plan.md` 对应章节
2. **执行过程中**：逐项勾选此清单
3. **阶段完成后**：更新"当前阶段"和"已完成阶段"
4. **遇到问题时**：记录到"阻塞问题"，暂停当前阶段
5. **跨会话恢复**：查看"当前阶段"和"下一步行动"

---

**最后更新**：2026-05-16
