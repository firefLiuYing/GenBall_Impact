---
description: 在 GenBall_Impact 项目中进行框架模块迁移时必须遵守的核心准则，确保新旧体系平滑过渡。
alwaysApply: false
enabled: true
updatedAt: 2026-05-13T14:32:00.000Z
provider: ""
---

# 模块迁移准则

## 目标
确保从旧 IComponent 体系向新 ISystem 体系迁移时，代码质量可控、新旧系统可共存、迁移过程可追溯。

## 触发条件
- 涉及框架层模块的重构、迁移、新建
- 文件路径匹配 `**/Yueyn/**/*.cs`
- 用户提到"迁移"、"重构"、"新系统"、"ISystem"

## 行为约束

### 1. 先建新后拆旧（必须）
- **必须**保留旧模块不动，新开发一套（接口 + 实现）
- **必须**验证新模块可用后再逐步迁移业务代码
- **禁止**直接修改或删除旧模块代码
- 最后再移除旧模块

### 2. 接口先行（必须）
- **必须**所有框架层模块接口化（`IXxxSystem : ISystem`）
- **必须**先定义接口，再写实现
- **建议**提供默认实现类（如 `XxxSystemDefault`）

### 3. 框架层不定义业务（必须）
- **必须**框架层（Yueyn 命名空间）只提供通用接口和默认实现
- **禁止**在框架层定义任何业务 enum/常量
- **必须**业务层（GenBall 命名空间）自行定义 enum 和特化子类

### 4. 新旧代码可以共存（必须）
- **必须**接受当前大量旧代码仍在使用中
- **禁止**主动删除或修改未涉及的旧代码
- **建议**在注释中标注新旧体系差异

### 5. 编写测试验证（必须）
- **必须**框架层模块完成后编写测试脚本
- **必须**测试脚本放在 `GenBall/Tests/` 目录
- **必须**优先使用 `Debug.Log` 输出结果
- 仅在 Log 无法满足时才考虑借助 UI

### 6. 任务完成同步文档（必须）
- **必须**每完成一个迁移任务或模块开发，同步更新 `CODEBUDDY.md`
- **必须**更新对应的状态标记（✅/🔄/⬜）
- **必须**添加新增模块的架构说明

## 示例

### ✅ 正确示例：对象池系统迁移
```csharp
// 1. 定义新接口（框架层）
namespace Yueyn.Pool {
    public interface IPoolSystem : ISystem {
        T Acquire<T>() where T : class, IReference, new();
        void Release<T>(T obj) where T : class, IReference;
    }
}

// 2. 实现类（框架层）
namespace Yueyn.Pool {
    public class PoolSystemDefault : IPoolSystem {
        public void Init() { /* 初始化 */ }
        public T Acquire<T>() => ReferencePool.Acquire<T>();
        public void Release<T>(T obj) => ReferencePool.Release(obj);
        public void UnInit() { /* 清理 */ }
    }
}

// 3. 注册到 SystemRepository（业务层）
namespace GenBall.Framework {
    public class FrameworkDefault : FrameworkBase {
        protected override void DoInit() {
            SystemRepository.Instance.RegisterSystem<IPoolSystem>(new PoolSystemDefault());
        }
    }
}

// 4. 旧代码保持不变（共存）
// ReferencePool 静态类仍然可用，71+ 处旧代码继续工作
```

### ❌ 错误示例：直接修改旧模块
```csharp
// ❌ 错误：直接修改 ReferencePool 类，破坏现有代码
public static class ReferencePool {
    // 添加新方法会影响所有使用者
    public static void NewMethod() { /* ... */ }
}

// ❌ 错误：在框架层定义业务枚举
namespace Yueyn.Pool {
    public enum PoolType { Character, Bullet, Weapon } // 业务相关
}

// ❌ 错误：没有接口直接实现
public class PoolSystemDefault : ISystem { /* 无法替换实现 */ }
```

## 例外情况
- 旧模块存在严重 bug 且影响新系统时，可在充分测试后修复
- 旧模块与新模块命名冲突时，可重命名旧模块（添加 Legacy 后缀）
