# GenBall_Impact 代码索引

> 从 915 个节点自动生成 | commit: bf0c222

## 模块结构

- **Assets/** (0 files)
  - **Scripts/** (0 files)
    - **GenBall/** (0 files)
      - **BattleSystem/** (0 files)
        - **Buff/** (13 files)
          - `BulletDamageUpBuff.cs` — 子弹伤害提升Buff实现，继承BuffObj，在添加时获取武器状态并增加伤害倍率区，移除时撤销增益。
          - `BuffEventIds.cs` — 定义Buff系统相关的事件ID常量，包括伤害前/后、死亡前/后的Buff触发事件标识。
          - `BuffId.cs` — 定义Buff ID常量，目前包含玩家护甲和子弹伤害提升两种Buff类型标识。
          - `BuffModelConfig.cs` — Buff模型配置系统，通过ScriptableObject加载Buff数据模型，提供BuffId到BuffModel/BuffType的查询，支持反射解析类型。
          - `BuffObj.cs` — Buff对象基类，管理Buff的生命周期（创建、叠层、移除、Tick更新），通过SystemRepository获取配置并支持对象池回收。
          - ... +8 more
        - **Command/** (24 files)
          - `AbilitySecondaryCommand.cs` — 技能副键命令结构体，实现IArbitratedCommand，传递按键状态和优先级参数，属于动作指令（不阻止移动/旋转，优先级2）。
          - `CharacterCommand.cs` — 角色基础命令定义：ICommand标记接口、MoveCommand（移动，带优先级）、RotateCommand（旋转）、AttackCommand（攻击，实现
          - `DashCommand.cs` — 冲刺命令结构体，实现IArbitratedCommand（优先级5/5），不可缓冲，阻止移动、旋转和重力。
          - `IAbilitySecondary.cs` — 技能副键接口，定义AbilitySecondary和CancelAbilitySecondary两个方法契约。
          - `IArbitratedCommand.cs` — 可仲裁命令接口，定义命令优先级仲裁所需属性：InterruptPriority、AntiInterruptPriority、Bufferable，以及移动/旋转
          - ... +19 more
        - **Executors/** (17 files)
          - `AbilityWeaponExecutor.cs` — 技能武器执行器，处理武器的主要攻击和次要技能调度，将攻击命令桥接到当前激活的武器实例进行处理。
          - `EnemyDashExecutor.cs` — 敌人冲刺执行器，实现多阶段（准备/飞行/蓄力/冲刺/反弹）的复杂冲刺行为，包含碰撞检测、目标追踪和伤害施加逻辑。
          - `EnemyFaceExecutor.cs` — 敌人朝向执行器，根据输入命令平滑旋转敌人朝向目标方向。
          - `EnemyGravityExecutor.cs` — 敌人重力执行器，模拟重力加速度影响敌人垂直速度，并检测地面状态。
          - `EnemyJumpExecutor.cs` — 敌人跳跃执行器，管理跳跃的启动、持续和取消状态，控制垂直速度变化。
          - ... +12 more
        - **Framework/** (26 files)
          - `AIDecisionAttackState.cs` — AI 攻击决策状态，进入时面向玩家并发出攻击指令，每帧持续追踪玩家朝向并维持攻击。
          - `AIDecisionChaseState.cs` — AI 追逐决策状态，在检测到玩家后持续朝向玩家移动，距离过远时切回 Idle，距离足够时切换为 Attack。
          - `AIDecisionIdleState.cs` — AI 闲置决策状态，在未检测到玩家时随机游走，检测到敌人后切换到追逐状态。
          - `AIDecisionWanderState.cs` — AI 漫游决策状态，在巡逻范围内随机移动和转向，检测到敌人后切换到追逐状态。
          - `EnemyDecisionStateBase.cs` — 敌人决策状态基类，提供 Fsm、Agent、Detect、AttackController 等共享引用和 ChangeState/IssueCommand 便捷
          - ... +21 more
        - **Weapons/** (37 files)
          - `AccessoryId.cs` — 配件 ID 枚举定义文件，声明可用的武器配件类型标识。
          - `AccessoryModel.cs` — 配件模型数据定义，描述配件的配置属性结构。
          - `AccessoryModelConfig.cs` — 配件模型配置文件，使用 ScriptableObject 存储配件数据，通过 AccessoryModelConfigProvider 提供编辑器模式下的自动加
          - `AccessoryObj.cs` — 配件运行时对象，管理配件的 Buff 添加和移除，使用引用池进行对象复用。
          - `EvolutionConfig.cs` — 武器进化配置，定义各进化阶段的解锁条件，通过 EvolutionConfigProvider 在编辑器中自动加载。
          - ... +32 more
      - **Entry/** (0 files)
      - **Event/** (0 files)
        - **Params/** (5 files)
          - `GrantAccessoryParams.cs` — 定义授予配件事件参数的轻量数据类，包含配件ID字段并通过Dispatch方法将事件通过CEventRouter全局事件总线分发。
          - `OpenDoorParams.cs` — 定义开门事件参数的数据类，包含门对象ID和开门速度两个字段，通过Dispatch方法触发全局事件。
          - `PlayDialogueParams.cs` — 定义播放对话事件参数的数据类，持有对话ID并通过CEventRouter分发事件通知。
          - `SpawnEnemyParams.cs` — 定义生成敌人事件参数的数据类，包含敌人类型、生成位置/旋转、巡逻/侦测半径和AI行为等六项配置字段。
          - `UnlockSavePointParams.cs` — 定义解锁存档点事件参数的数据类，包含存档点ID用于通过全局事件总线通知存档系统。
        - **Templates/** (1 files)
          - `ValueEventTemplateConfig.cs` — 定义值事件模板配置系统，包含ValueEventTemplateConfig（事件列表容器）和ValueEventDefinition（单个事件定义的Schem
      - **Framework/** (0 files)
        - **Config/** (4 files)
          - `AppConfigManager.cs` — 应用配置管理器，实现ISystem接口，负责加载和管理游戏运行所需的各类配置（AppSettings、Player、Buff、Bullet、Scene、Plac
          - `AppSettingsConfig.cs` — 应用全局设置配置的ScriptableObject数据容器，定义最大存档数、启动场景、运行模式、加载参数、默认玩家生成点和开发者模式等核心启动参数。
          - `BulletConfig.cs` — 子弹配置系统，包含BulletConfigCollection（基于Dictionary的ID查找集合）和BulletConfigEntry（定义检测模式、视觉
          - `IConfigProvider.cs` — 配置提供者接口，定义ISystem级别的配置服务契约，通过泛型GetConfig<T>方法提供类型安全的配置获取能力。
        - **Entity/** (4 files)
          - `EntityUpdateSystem.cs` — 实体更新调度系统，实现ISystem和IFrameUpdate/ILogicUpdate接口，通过SafeIterableList管理实体的FrameUpdat
          - `IEntityFrameUpdate.cs` — 实体帧更新接口，定义FrameUpdate(float deltaTime)方法，由EntityUpdateSystem每帧调用实体进行帧级更新。
          - `IEntityLogicUpdate.cs` — 实体逻辑更新接口，定义LogicUpdate(float deltaTime)方法，由EntityUpdateSystem按逻辑帧调用实体进行逻辑更新。
          - `IEntityUpdateSystem.cs` — 实体更新系统接口，继承ISystem，定义AddFrameUpdate/RemoveFrameUpdate/AddLogicUpdate/RemoveLogic
        - **TimeScale/** (1 files)
          - `TimeScaleSystemDefault.cs` — 时间缩放系统默认实现，支持多个来源按优先级请求不同的时间缩放倍率，通过Request/ReleaseRequest管理缩放句柄，EffectiveScale计算
      - **Player/** (0 files)
        - **Initializer/** (3 files)
          - `PlayerArmorInitializer.cs` — 玩家护甲初始化器，在玩家角色创建时为角色添加ArmorBuff护甲Buff，通过IBuffRegistry和AddBuffInfo完成Buff的生命周期绑定。
          - `PlayerCameraInitializer.cs` — 玩家相机初始化器，将主相机挂载到角色Transform下并设置为特定渲染层，SetCamera方法包含空检查和异常处理防御逻辑。
          - `PlayerUiInitializer.cs` — 玩家UI初始化器，在角色初始化时通过GameEntry.Event触发PlayerMaxHealth事件，并从ArmorBuff读取护甲值更新UI；提供Upda
        - **Input/** (3 files)
          - `InputController.cs` — 玩家输入控制器，将 Unity Input System 的原始输入事件转化为游戏内标准事件并派发到全局事件管理器。
          - `InputEventArgs.cs` — 定义输入事件参数数据结构，包含泛型和非泛型两个版本，支持引用池回收。
          - `InputHandler.cs` — 玩家输入处理器，监听 InputController 派发的输入事件，维护各输入按键的状态并暴露给 Player 控制系统。
        - **States/** (5 files)
          - `PlayerDashState.cs` — 玩家冲刺状态，实现短距离快速位移、无敌时间和倒计时冷却逻辑。
          - `PlayerInitState.cs` — 玩家初始化状态，首次进入时初始化 AccessoryController，按地面状态自动转换到 PlayerMoveState 或 PlayerJumpStat
          - `PlayerJumpState.cs` — 玩家跳跃状态，实现短按/长按跳跃、土狼时间（coyote time）、输入缓冲、重力加速度和空中移动等完整跳跃物理。
          - `PlayerMoveState.cs` — 玩家地面移动状态，处理地面行走移动、视角旋转，监听跳跃/冲刺/离地事件进行状态转换。
          - `PlayerStateBase.cs` — 玩家状态基类，所有玩家 FSM 状态（Move/Jump/Dash/Init）的公共抽象基类。
      - **Procedure/** (0 files)
        - **Execute/** (7 files)
          - `ILaunchSystem.cs` — 定义启动系统接口，声明启动模式、起始场景名、跳过启动加载和带上下文启动游戏的方法签名。
          - `ISceneExecutorSystem.cs` — 定义场景执行系统接口和 SceneInitContext 数据上下文（包含 SpawnPosition/SpawnRotation）。
          - `ISceneLoadSystem.cs` — 场景加载系统接口，定义场景异步加载的属性和方法，包括加载进度、加载状态和目标场景名称。
          - `LaunchStates.cs` — 游戏启动流程的状态机状态定义，包含启动加载、开始表单和加载场景三个阶段的状态逻辑，负责启动流程中的场景配置加载与执行调度。
          - `LaunchSystemDefault.cs` — 启动系统的默认实现，管理游戏启动状态机（SimpleFsm），在 FrameUpdate 中驱动状态更新，支持跳过启动加载和带上下文开始游戏。
          - ... +2 more
        - **Game/** (7 files)
          - `GameManager.cs` — 游戏管理器，实现 IGameManagerSystem 接口，负责存档数据提供者注册、游戏存档的保存/加载/更新，以及存档字段的合并管理。
          - `GameProcedure.cs` — 游戏流程空文件，仅包含命名空间声明，可能作为目录占位或预留扩展。
          - `GameStartSystemDefault.cs` — 游戏开始系统的默认实现，处理新游戏、继续游戏和加载存档三种启动模式，构建 GameStartContext 并委托给启动系统执行。
          - `IGameManagerSystem.cs` — 游戏管理器接口，定义存档数据提供者注册、游戏数据存取和运行模式管理的契约方法。
          - `IGameStartSystem.cs` — 游戏开始系统接口及相关数据模型，定义 GameStartRequest（启动请求）、GameStartContext（启动上下文）和 IGameStartSys
          - ... +2 more
      - **Utils/** (0 files)
        - **Attributes/** (1 files)
          - `LiveDataAttribute.cs` — 定义 LiveDataAttribute 自定义特性，用于标记需要生成 LiveData 属性的字段，支持事件名称、属性名称和跳过相等检查等配置选项。
        - **CodeGenerator/** (4 files)
          - `UiBindingConfig.cs` — UI 绑定配置中心，维护 GameObject 命名前缀到 UI 组件类型的映射表，支持前缀匹配和配置导出为 JSON，为 UiBindTool 提供绑定规则。
          - `IBindable.cs` — 定义可绑定 UI 控件的接口 IBindable，要求实现类提供 Type 属性来标识其绑定的组件类型。
          - `UiBindTool.cs` — UI 绑定工具核心类，维护 Text/Image/Button/RectTransform 四类 UI 控件的绑定映射表，提供 Get/Set/Clear 操作
          - `UiViewBinding.cs` — UI 视图绑定配置的 ScriptableObject，存储 Form/Part 类型、命名空间、输出路径、生成目标等代码生成所需元数据。
        - **Countdown/** (2 files)
          - `CountdownController.CountdownEvent.cs` — CountdownController 分部类文件，定义内部 CountdownEvent 类实现 IReference 接口，封装倒计时状态管理和回调触发。
          - `CountdownController.cs` — CountdownController 主分部类，管理多个命名倒计时事件的生命周期，提供开始/暂停/恢复/重置/增删/帧更新等完整接口。
        - **Editor/** (6 files)
          - `BakingPipeline.cs` — 编辑器场景烘焙管线，收集当前场景中 IScenePlaceable 组件并按类别归类后写入全局 SceneConfigCollection 资源，包含验证和重复
          - `MapSceneEditorWindow.cs` — Unity EditorWindow 实现的地图场景编辑器，提供分类树视图、属性面板、场景设置、对象创建（Prefab/Type 实例化）、验证和烘焙等完整 I
          - `PlaceableContextMenu.cs` — Hierarchy 右键菜单扩展，提供快速创建 NormalOrbis/SavePoint/TriggerVolume 等 Map Placeable 对象的菜
          - `PlaceableSceneGUI.cs` — Scene View 可视化绘制工具，为 IScenePlaceable 组件绘制 Gizmos 和 Handles，按类别使用不同颜色区分。
          - `PlaceableTypeDiscovery.cs` — 基于反射的 IScenePlaceable 类型发现工具，扫描所有程序集筛选实现了 IScenePlaceable 的非抽象 MonoBehaviour 子类。
          - ... +1 more
        - **Singleton/** (2 files)
          - `ISingleton.cs` — 定义 ISingleton 单例标记接口，用于标识可被 SingletonManager 管理的单例类型。
          - `SingletonManager.cs` — 单例管理器，维护一个类型到单例实例的字典，通过泛型 GetSingleton 方法按需创建并缓存单例对象。
        - **Trigger/** (1 files)
          - `TriggerObject.cs` — 通用触发器 MonoBehaviour 组件，监听 Unity 物理触发事件（OnTriggerEnter/Stay/Exit），通过 LayerMask 过滤
    - **Yueyn/** (0 files)
      - **Base/** (0 files)
        - **EventPool/** (4 files)
          - `BaseEventArgs.cs` — 定义事件参数基类 BaseEventArgs，实现 IReference 接口以支持对象池复用，包含事件 ID 属性和 Clear 清理方法。
          - `EventPool.cs` — 泛型事件池核心实现，支持事件的订阅、取消订阅、触发（Fire 入队延迟执行 / FireNow 立即执行）和默认处理器，通过 EventPoolMode 控制行
          - `EventPool.Event.cs` — EventPool 的分部类文件，定义内部私有 Event 类实现 IReference 接口，封装事件发送者和参数，提供静态工厂 Create 方法和 Cle
          - `EventPoolMode.cs` — 定义 EventPoolMode 标志枚举，控制事件池的行为模式：默认模式、允许无处理器、允许多处理器和允许重复处理器四种组合。
        - **ReferencePool/** (3 files)
          - `IReference.cs` — 定义 IReference 接口，要求实现类提供 Clear 方法用于引用池回收时重置对象状态。
          - `ReferencePool.cs` — 引用池核心管理类，提供泛型和非泛型的 Acquire（获取）/Release（释放）/Add（预分配）/Remove（移除）等对象池操作，内部通过 Intern
          - `ReferencePool.InternalPool.cs` — ReferencePool 的分部类文件，定义内部私有 InternalPool 类，封装 Queue 队列管理单个引用类型的对象池、使用计数统计、线程安全的 
        - **Variable/** (2 files)
          - `GenericVariable.cs` — 泛型变量系统实现，包含 Variable<T>（基于观察者列表的响应式变量）和 LiveDelegate<TDelegate>（基于委托的动态变量）两种变量类型
          - `Variable.cs` — 定义 Variable 抽象基类，包含 Type 只读属性标识变量类型，声明 GetValue、SetValue 和 Clear 抽象方法，作为所有具体变量类的
      - **Event/** (0 files)
      - **Fsm/** (0 files)
      - **Main/** (0 files)
        - **Entry/** (1 files)
          - `Entry.cs` — 旧框架核心入口类，管理 IComponent 组件的注册/初始化/帧更新/注销，按优先级排序调度。
      - **ObjectPool/** (0 files)
      - **Pool/** (0 files)
      - **Resource/** (0 files)
      - **Timer/** (0 files)
      - **UI/** (0 files)
      - **Utils/** (0 files)

## 架构要点 (来自图谱分析)

### 接口 → 实现

| 接口 | 实现 |
|---|---|
| **IAmmoSystem** | HeatComponent, InfiniteAmmoComponent, WeaponMagazineExecutor |
| **IArbitratedCommand** | AbilitySecondaryCommand, AttackCommand, DashCommand, InteractCommand |
| **IBuff** | BulletDamageUpBuff, BuffObj, ArmorBuff |
| **IBuffRegistry** | BuffRegistry |
| **IBuffTickSystem** | BuffTickSystem |
| **IDamageCalculator** | BulletDamageCalculator, SimpleDamageCalculator, WeaponDamageCalculator |
| **IDecisionLayer** | EnemyDecisionLayer, PlayerDecisionLayer |
| **IEvolutionSystem** | EvolutionSystem |
| **IObjectPool<T>** | ObjectPoolManager.ObjectPool.cs |
| **IResourceHelper** | ResourceHelperAssetBundle, ResourceHelperEditor |
| **ISpreadProvider** | SpreadComponent |
| **ITriggerBehavior** | ChargeTriggerBehavior, FullAutoTriggerBehavior, SemiAutoTriggerBehavior, ShotgunTriggerBehavior |
| **IWeaponTrigger** | WeaponFireDecision |

### 关键依赖关系

- `BuffRegistry.cs` → `BuffObj.cs`
- `BuffTickSystem.cs` → `IBuffRegistry.cs`
- `BuffTickSystem.cs` → `IBuffContainer.cs`
- `SystemUpdaterManager` → `SystemUpdater`
- `SystemUpdaterManager.cs` → `SystemUpdater.cs`
- `SystemUpdaterManager.cs` → `SystemScope.cs`
- `ObjectPoolManager` → `ObjectPoolBase`
- `ObjectPoolManager` → `IObjectPool<T>`
- `ObjectPoolManager.Object.cs` → `ObjectPoolManager`
- `ObjectPoolManager.ObjectPool.cs` → `ObjectPoolManager`
- `CPoolManager.Object.cs` → `CPoolManager`
- `CPoolManager.ObjectPool.cs` → `CPoolManager`
- `CPoolManager` → `IObjectPool<T>`
- `CPoolManager.cs` → `IObjectPool.cs`
- `CResourceManager` → `IResourceHelper`
- `ResourceHelperAssetBundle` → `AssetBundleLoader`
- `CResourceManager.cs` → `ResourceHelperAssetBundle.cs`
- `CResourceManager.cs` → `ResourceHelperEditor.cs`
- `Timer.Event.cs` → `Timer`
- `TimerManager.cs` → `Timer`
- `TimerManager.cs` → `Timer.cs`
- `BusinessLogicManager` → `BusinessLogicBase`
- `BusinessFormLogic.cs` → `BusinessLogicManager`
- `BusinessFormLogic.cs` → `BusinessLogicBase.cs`
- `BusinessLogicManager.cs` → `BusinessLogicBase.cs`
- `BusinessPartLogic<TView, TViewData>` → `PartViewBase`
- `BusinessPartLogicContainer` → `Resolve`
- `UIFormScript` → `UIComponent`
- `UIManager` → `UIFormScript`
- `SingletonManager` → `ISingleton.cs`
