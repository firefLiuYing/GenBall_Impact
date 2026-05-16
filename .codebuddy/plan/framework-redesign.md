# 框架架构重新设计计划

> **状态**：🔄 进行中  
> **创建时间**：2026-05-13  
> **目标**：重新设计框架分层，将基建模块与业务系统分离

---

## 问题分析

### 1. 基建模块与业务模块混在一起
- **现状**：事件、UI、对象池、资源管理都注册到 `SystemRepository`
- **问题**：这些是框架基建，应该由 `FrameworkBase` 直接管理生命周期
- **影响**：业务系统初始化时基建可能还没准备好，时序难以控制

### 2. SystemRepository 职责不清
- **现状**：既管理基建模块，又管理业务模块
- **问题**：初始化顺序依赖注册顺序，容易出错
- **影响**：难以保证基建先于业务初始化

### 3. 具体模块设计问题
- 事件系统：待确认
- 资源管理：待确认
- UI 系统：待确认

---

## 重构计划

### 阶段 0：暂停迁移，整理现状
- [x] 停止新模块迁移
- [ ] 记录当前已迁移模块状态
- [ ] 明确待重构的范围

### 阶段 1：重新设计框架分层
- [ ] **1.1 定义基建模块清单**
  - 确定哪些模块属于框架基建
  - 确定哪些模块属于业务系统

- [ ] **1.2 重构 FrameworkBase**
  - 添加基建模块的直接引用和生命周期管理
  - 明确初始化顺序
  - 提供统一的基建模块访问接口

- [ ] **1.3 重构 SystemRepository**
  - 明确定位为**业务系统容器**
  - 移除基建模块的注册逻辑
  - 简化接口

### 阶段 2：重构基建模块
- [ ] **2.1 事件系统（IEventSystem）**
  - 问题：待确认
  - 重构方案：待确认

- [ ] **2.2 资源管理（IResourceSystem）**
  - 问题：待确认
  - 重构方案：待确认

- [ ] **2.3 UI 系统（IUISystem）**
  - 问题：待确认
  - 重构方案：待确认

- [ ] **2.4 对象池（IPoolSystem）**
  - 问题：待确认
  - 重构方案：待确认

### 阶段 3：验证与测试
- [ ] 更新测试代码
- [ ] 验证基建模块初始化顺序
- [ ] 验证业务系统能正常访问基建
- [ ] 更新文档

### 阶段 4：恢复迁移
- [ ] 基于新架构继续迁移旧模块

---

## 当前状态快照

### 已迁移到 SystemRepository 的模块（需重新评估）

| 模块 | 当前状态 | 是否基建 | 重构优先级 |
|---|---|---|---|
| IEventSystem (CEventSystem) | ✅ 已完成 | ✅ 是 | P0 |
| IResourceSystem (Editor/AssetBundle) | ✅ 已完成 | ✅ 是 | P0 |
| IUISystem (UISystemDefault) | ✅ 已完成 | ✅ 是 | P0 |
| IPoolSystem (PoolSystemDefault) | ✅ 已完成 | ✅ 是 | P0 |

### 旧体系模块（暂不动）

| 模块 | 访问方式 | 备注 |
|---|---|---|
| Event | GameEntry.Event | 旧事件系统（代码生成） |
| Fsm | GameEntry.Fsm | 状态机 |
| Buff | GameEntry.Buff | Buff系统 |
| CharacterCreator | GameEntry.CharacterCreator | 角色实体工厂 |
| Timeline | GameEntry.Timeline | 时间轴 |
| Bullet | GameEntry.Bullet | 子弹系统 |
| Evolution | GameEntry.Evolution | 进化系统 |
| Save | GameEntry.Save | 存档系统 |
| Player | GameEntry.Player | 玩家管理 |
| Map | GameEntry.Map | 地图管理 |
| Execute | GameEntry.Execute | 流程控制 |
| Scene | GameEntry.Scene | 场景管理 |

---

## 讨论记录

### 2026-05-13 初始讨论

**用户反馈**：
> 对于大多数业务逻辑，确实可以直接用 ISystem 来管理，但是对于一些框架运行的基建模块是不能直接这么管理的，他们可能需要贯穿程序的整个生命周期，也有可能是其他业务 System 初始化时会用到的，比如我们迁移完的四个模块，事件，UI，对象池，资源管理，这四个应该由 Framework 来直接控制初始化以及 Update 的时序。

**下一步**：
1. 确认基建模块清单
2. 确认事件系统的设计问题

---

## 更新日志

| 日期 | 变更 |
|---|---|
| 2026-05-13 | 创建文档，记录问题分析和初步计划 |