# Code Patterns

This document provides practical implementation examples and recipes for common tasks.

## Combat Patterns

### Creating a New Buff

1. 在`BuffId`枚举中添加新的Buff ID
2. 创建Buff类继承自`BuffObj`
3. 实现需要的回调接口（如`ITriggerBeforeTakeDamage`）
4. 在`BuffIdToExtension.ToType()`中注册类型映射
5. 在`BuffModelConfig`中配置Buff数据

**Example**:
```csharp
public class MyBuff : BuffObj, ITriggerAfterCauseDamage
{
    public override void OnAdd(AddBuffInfo addBuffInfo)
    {
        // Buff添加时的初始化逻辑
    }

    public void TriggerAfterCauseDamage(DamageInfo damageInfo)
    {
        // 造成伤害后触发的逻辑
    }

    public override void Clear()
    {
        base.Clear();
        // 清理资源
    }
}
```

See [Buff System Reference](buff-system-reference.md) for more details.

### Adding a Buff to Entity

```csharp
var info = AddBuffInfo.Create(
    BuffId.MyBuff, 
    targetGameObject, 
    addStacks: 1, 
    caster: casterGameObject
);
GameEntry.Buff.AddBuff(info);
```

### Applying Damage

```csharp
var damageInfo = DamageInfo.Create(
    defender: targetGameObject,
    damage: 10,
    tags: new List<string> { "bullet" },
    direction: hitDirection,
    impactForce: 5,
    attacker: sourceGameObject
);
DamageSystem.Instance.ApplyDamage(damageInfo);
// DamageInfo is auto-released to ReferencePool after processing
```

### Firing a Bullet

```csharp
var launchInfo = BulletLaunchInfo.Create(
    model: bulletModel,
    logicSpawnPoint: muzzlePosition,
    rendererSpawnPoint: muzzleVisualPosition,
    spawnDirection: aimDirection,
    source: playerGameObject
);
GameEntry.Bullet.FireBullet(launchInfo);
// BulletLaunchInfo is auto-released after processing
```

## Entity Patterns

### Creating a New Character

1. 创建GameObject并添加`CharacterState`组件
2. 配置`CharacterStatsModel`字段（baseHealth等）
3. 添加所需的`ICharacterInitializer`和`ICharacterController`组件
4. 实现`IMove`和`IRotate`接口（如需要）
5. 通过EntityCreator注册和创建

**Example Setup**:
```
CharacterPrefab (GameObject)
├── CharacterState (Component)
│   └── CharacterStatsModel: baseHealth = 100
├── PlayerMover (ICharacterController, IMove)
├── JumpController (ICharacterController)
├── PlayerArmorInitializer (ICharacterInitializer)
└── PlayerUiInitializer (ICharacterInitializer)
```

### Creating a New Weapon (New Architecture)

1. 创建WeaponState预制体
2. 添加`IWeaponTriggerController`实现（如继承已有的NormalTriggerController）
3. 添加`IWeaponReloadController`实现（如NormalReloadController）
4. 配置`WeaponModel`序列化字段（damage, fireInterval, reloadTime）
5. 注册预制体到EntityCreator

**Example Setup**:
```
WeaponPrefab (GameObject)
├── WeaponState (Component)
│   └── WeaponModel: damage, fireInterval, reloadTime
├── NormalTriggerController (IWeaponTriggerController)
└── NormalReloadController (IWeaponReloadController)
```

### Creating a New Enemy

1. 创建敌人prefab，添加`CharacterState`组件
2. 配置`CharacterStatsModel`定义属性
3. 添加所需的Controller和Initializer组件
4. 在场景中放置或通过SceneSystem配置EnemyUnitModel

**Example Setup**:
```
EnemyPrefab (GameObject)
├── CharacterState (Component)
│   └── CharacterStatsModel: baseHealth = 50
├── EnemyAI (ICharacterController)
├── EnemyMover (ICharacterController, IMove)
└── EnemyAttack (ICharacterController)
```

### Using the EntityCreator

```csharp
// Register prefab (usually in Init)
GameEntry.CharacterCreator.AddPrefab<CharacterState>(
    "EnemyName", 
    "Assets/path/to/prefab.prefab"
);

// Create entity
var entity = GameEntry.CharacterCreator.CreateEntity<CharacterState>(
    "EnemyName", 
    position, 
    rotation, 
    parent
);

// Recycle entity
GameEntry.CharacterCreator.RecycleEntity(entity.gameObject);
```

## UI Patterns

### Creating a New UI Form

1. Create prefab with Canvas component
2. Add `UiBindTool` component and configure bindings
3. Create form class inheriting from `FormBase`
4. Generate `.Bind.cs` using UiBindTool inspector
5. Override OnInit/OnOpen/OnClose/OnFocus/OnUnfocus as needed
6. (Optional) Create VmBase子类管理UI数据
7. Open via `GameEntry.UI.OpenForm<TForm>()`

**Example Form Class**:
```csharp
public partial class MyForm : FormBase
{
    protected override void OnInit()
    {
        base.OnInit();
        // Initialize UI elements
        _closeButton.onClick.AddListener(OnCloseClicked);
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        // Form opened
    }

    protected override void OnClose()
    {
        base.OnClose();
        // Form closed
    }

    private void OnCloseClicked()
    {
        GameEntry.UI.CloseForm(this);
    }
}
```

**Example with ViewModel**:
```csharp
public class MyFormVm : VmBase
{
    public Variable<int> Score { get; private set; }

    public override void Init()
    {
        Score = new Variable<int>(0);
    }

    public override void Clear()
    {
        Score = null;
    }
}

public partial class MyForm : FormBase
{
    private MyFormVm _vm;

    protected override void OnInit()
    {
        base.OnInit();
        _vm = ReferencePool.Acquire<MyFormVm>();
        _vm.Init();
        
        // Bind to ViewModel
        _vm.Score.Observe(score => _scoreText.text = score.ToString());
    }

    protected override void OnClose()
    {
        ReferencePool.Release(_vm);
        base.OnClose();
    }
}
```

## Event Patterns

### Subscribing to Events

```csharp
// Global events (type-safe generated methods)
GameEntry.Event.SubscribePlayerKillPoints(OnKillPointsChanged);
GameEntry.Event.FireWeaponLevel(newLevel);

// Global events (manual)
GameEntry.Event.Subscribe(eventId, OnEventHandler);

// Local events (weapons)
weapon.Subscribe(WeaponEventId, OnWeaponEvent);
```

**Example Event Handler**:
```csharp
private void OnKillPointsChanged(object sender, GameEventArgs e)
{
    var args = (PlayerKillPointsEventArgs)e;
    Debug.Log($"Kill points: {args.KillPoints}");
}
```

## Interaction Patterns

### Using the Interact System

```csharp
// Implement IInteractable on a MonoBehaviour
public class MyInteractable : MonoBehaviour, IInteractable
{
    public string OperationDescription => "打开";
    
    public void Interact()
    {
        // 交互逻辑
        Debug.Log("Interacted!");
    }
}
// InteractController will auto-detect via SphereCast on interactableLayer
```

**Triggering Interaction from Code**:
```csharp
// Trigger current selected interactable
InteractSystem.Instance.TriggerInteractable();

// Cycle through interactables
InteractSystem.Instance.NextSelection();
InteractSystem.Instance.LastSelection();
```

## Related Documentation

- [Battle Systems](battle-systems.md) - Combat system architecture
- [Buff System Reference](buff-system-reference.md) - Detailed Buff documentation
- [Systems Overview](systems-overview.md) - Game systems overview
- [Architecture Guide](architecture.md) - Core architectural patterns