# Implementation Spec: SimpleFsm 测试

## 目标

为 `Yueyn.Fsm.SimpleFsm<TContext>` 编写 EditMode 测试。**不修改** SimpleFsm.cs 的任何代码。

## 文件

### 新建

- `Assets/Scripts/GenBall/Framework/Editor/SimpleFsmTests.cs` — 13 个测试用例

### 不改动

- `Assets/Scripts/Yueyn/Fsm/SimpleFsm.cs` — 被测代码，不做任何修改

## 约束

- SimpleFsm 的 `OnEnter`/`OnUpdate`/`OnExit` 是 virtual 方法。测试中用子类重写它们来记录调用次数和参数，不修改 SimpleFsm 本身
- 使用 `[SetUp]` 创建 Context 对象和 TestState 实例
- 使用 `[TearDown]` 清理

## 测试模式

```csharp
// 记录调用的假状态
class RecordState : SimpleFsmState<object>
{
    public int EnterCount;
    public int ExitCount;
    public int UpdateCount;
    public float LastDeltaTime;

    public override void OnEnter(object ctx) { EnterCount++; }
    public override void OnUpdate(object ctx, float dt) { UpdateCount++; LastDeltaTime = dt; }
    public override void OnExit(object ctx) { ExitCount++; }
}
```

## 注意

- 不要添加 .asmdef 文件（项目不使用 asmdef）
- SimpleFsm 在 Assembly-CSharp，测试在 Assembly-CSharp-Editor，可以直接引用
- 测试类放在 Editor 文件夹下即自动编译到正确的 assembly
