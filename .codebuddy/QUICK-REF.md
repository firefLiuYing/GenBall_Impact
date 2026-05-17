# 迁移快速参考卡片

> **一页纸速查**：跨会话恢复时快速了解状态

---

## 📍 当前状态

```
阶段：⬜ 阶段 0（基建修复）
进度：0/8 阶段完成
阻塞：4 个基建问题待修复
```

---

## 🎯 下一步行动

**执行阶段 0：基建修复**（约 50 分钟）

```
[ ] 定义 IResourceSystem 接口
[ ] CResourceManager 实现 IResourceSystem
[ ] CEventSystem 实现 IEventSystem + IFrameUpdate
[ ] CPoolManager 实现 IPoolSystem + IFrameUpdate
[ ] 在 FrameworkDefault 中注册所有基建系统
[ ] 移除 FrameworkBase 中的手动 Update 调用
[ ] 编写基建验证测试
```

---

## 📚 关键文档

| 文档 | 用途 |
|---|---|
| `MIGRATION-SUMMARY.md` | 总览（先看这个） |
| `migration-checklist.md` | 执行清单（逐项勾选） |
| `migration-master-plan.md` | 详细方案（8 阶段完整计划） |
| `design-review.md` | 设计审查（问题详情） |

---

## 🔧 基建问题速查

| 系统 | 问题 | 修复 |
|---|---|---|
| 资源 | 接口缺失 | 定义 IResourceSystem |
| 事件 | 未实现接口 | 实现 IEventSystem + IFrameUpdate |
| 对象池 | 接口缺失 | 定义 IPoolSystem + IFrameUpdate |
| UI | 未注册 | 在 FrameworkDefault 中注册 |

---

## 🗺️ 8 阶段路线图

```
0. 基建修复 ⬜ (1 会话) ← 当前
1. EntityCreator ⬜ (2 会话)
2. BuffSystem ⬜ (1 会话)
3. 战斗系统 ⬜ (1 会话)
4. 游戏逻辑 ⬜ (2 会话)
5. 辅助系统 ⬜ (1 会话)
6. 启动流程 ⬜ (1 会话)
7. GameEntry 移除 ⬜ (1 会话)
8. 文档更新 ⬜ (1 会话)
```

---

## ⚡ 快速命令

### 查看当前阶段
```bash
cat .codebuddy/migration-checklist.md | grep "当前阶段"
```

### 查看待办事项
```bash
cat .codebuddy/migration-checklist.md | grep "^- \[ \]"
```

### 查看阻塞问题
```bash
cat .codebuddy/refactoring-plan.md | grep -A 10 "阻塞问题"
```

---

## 🚨 注意事项

1. **不要动代码生成文件**（Generated/, .Bind., .Generated.）
2. **保持旧体系运行**（新旧共存）
3. **每阶段完成后提交**（便于回滚）
4. **更新 checklist**（勾选完成项）

---

**最后更新**：2026-05-16
