# 代码修改规则

## 项目架构概述

### 新框架体系（ISystem - 正在迁移中）

**核心理念**：基于 GameFramework 设计，解耦、接口化、可替换

**关键组件**：
- `FrameworkBase` - 唯一 MonoBehaviour 入口，标记 DontDestroyOnLoad
- `SystemRepository` - IoC 容器，管理业务系统注册/获取
- `SystemUpdaterManager` - 统一调度系统更新，支持暂停
- `ISystem` - 最小系统接口（Init/UnInit）
- `IFrameUpdate/ILogicUpdate/ILateFrameUpdate` - 可选更新接口

**基建模块**（由 FrameworkBase 直接管理）：
- `IEventSystem` (CEventSystem) - 事件系统
- `IResourceSystem` (ResourceSystemEditor/AssetBundle) - 资源管理
- `IUISystem` (UISystemDefault) - UI 系统
- `IPoolSystem` (PoolSystemDefault) - 对象池

**访问方式**：
```csharp
// 基建模块
SystemRepository.Instance.GetSystem<IEventSystem>()
SystemRepository.Instance.GetSystem<IUISystem>()

// 业务系统（未来）
SystemRepository.Instance.GetSystem<IBuffSystem>()
```

### 旧框架体系（IComponent - 保持运行）

**访问方式**：
```csharp
GameEntry.Event      // 旧事件系统（代码生成）
GameEntry.UI         // 旧 UI 系统（MVVM）
GameEntry.Buff       // Buff 系统
GameEntry.Player     // 玩家管理
// ... 等等
```

**状态**：保持运行，不再新增功能，逐步迁移到新体系

---

## 禁止修改的文件

### 1. 代码生成器生成的文件

**规则**：所有由代码生成器生成的文件**严禁手动修改**。

**识别方式**：
- 文件路径包含 `Generated/` 目录
- 文件名包含 `.Generated.` 后缀
- 文件头部包含 `Auto-generated` 或 `DO NOT EDIT MANUALLY` 注释
- 文件名包含 `.Bind.` 后缀（UI 绑定代码）

**示例**：
```
Assets/Scripts/GenBall/Event/Generated/GlobalEventSystem.Generated.cs
Assets/Scripts/GenBall/BattleSystem/Generated/EffectEvents.Generated.cs
Assets/Scripts/GenBall/UI/*/XXXForm.Bind.cs
```

**如需修改**：
- 修改代码生成器的模板或配置文件
- 重新运行代码生成器
- 不要直接编辑生成的文件

---

## 迁移策略

### 渐进式迁移原则

当重构或替换现有系统时，必须遵循以下原则：

1. **保留旧系统**：不要删除或覆盖旧的实现
2. **创建新系统**：在新的文件中实现新功能
3. **并行运行**：确保新旧系统可以共存
4. **逐步迁移**：一次迁移一个模块或文件
5. **验证后删除**：只有在所有使用点都迁移完成后才删除旧代码

### 新系统开发规范

#### 1. 接口先行（必须）
```csharp
// 框架层定义接口
namespace Yueyn.SomeModule {
    public interface ISomeSystem : ISystem {
        void DoSomething();
    }
}

// 框架层提供默认实现
namespace Yueyn.SomeModule {
    public class SomeSystemDefault : ISomeSystem {
        public void Init() { }
        public void UnInit() { }
        public void DoSomething() { }
    }
}
```

#### 2. 框架层不定义业务（必须）
- **禁止**在 Yueyn 命名空间定义业务 enum/常量
- **必须**业务相关定义放在 GenBall 命名空间

#### 3. 系统不继承 MonoBehaviour（必须）
- **禁止**新系统继承 MonoBehaviour
- **必须**实现 ISystem 接口
- 需要帧更新时实现 IFrameUpdate/ILogicUpdate

#### 4. 注册到 SystemRepository（必须）
```csharp
// 在 FrameworkDefault.DoInit() 中注册
SystemRepository.Instance.RegisterSystem<ISomeSystem>(new SomeSystemDefault());
```

---

## 特殊规则

### UI 开发规范

**必须**：
- 新 UI 使用 MVP 架构（UIFormLogic + UIFormView）
- 文本组件使用 `UnityEngine.UI.Text`（Legacy Text）
- Logic 文件命名：`XxxLogic.cs`
- View 文件命名：`XxxView.cs`

**禁止**：
- 使用旧 MVVM 架构（FormBase + VmBase）
- 使用 TextMeshPro（TMP_Text/TextMeshProUGUI）
- 使用旧命名方式（FormName.cs + FormNameVm.cs）

### 资源加载规范

**必须**使用编译宏切换：
```csharp
#if UNITY_EDITOR
    CResourceManager.Instance.SetHelper(new ResourceHelperEditor());
#else
    CResourceManager.Instance.SetHelper(new ResourceHelperAssetBundle());
#endif
```

**禁止**运行时判断：
```csharp
// ❌ 错误
if (Application.isEditor) { }
```

---

## 检查清单

在修改代码前，请检查：

- [ ] 文件是否在 `Generated/` 目录下？
- [ ] 文件名是否包含 `.Generated.` 或 `.Bind.`？
- [ ] 文件头部是否有 `DO NOT EDIT` 注释？
- [ ] 是否有大量代码依赖此文件？
- [ ] 是否需要采用渐进式迁移策略？
- [ ] 新系统是否继承了 MonoBehaviour？（应该实现 ISystem）
- [ ] 是否在框架层定义了业务逻辑？（应该在业务层）

如果以上任何一项为"是"，请重新考虑修改方案。

---

## 参考文档

- `.codebuddy/refactoring-plan.md` - 框架重构计划和进度
- `.codebuddy/rules/module-migration.md` - 模块迁移准则
- `.codebuddy/rules/system-architecture.md` - 系统架构双轨体系
- `.codebuddy/rules/ui-framework.md` - UI 框架开发规范
