# Test Spec: SimpleFsm

> Status: implemented
> Created: 2026-07-05

`Yueyn.Fsm.SimpleFsm<TContext>` — 轻量泛型状态机，87 行纯 C#。测试覆盖构造、状态转换、生命周期回调。

## TC-001: 构造 — null context 应抛异常
- **Given**: 无
- **When**: `new SimpleFsm<object>(null, new TestState())`
- **Then**: 抛出 `ArgumentNullException`

## TC-002: 构造 — 空 states 数组应抛异常
- **Given**: 无
- **When**: `new SimpleFsm<object>(new object())`
- **Then**: 抛出 `ArgumentException`

## TC-003: 构造 — states 含 null 元素应抛异常
- **Given**: 无
- **When**: `new SimpleFsm<object>(new object(), null)`
- **Then**: 抛出 `ArgumentException`

## TC-004: Start — 正确启动并调用 OnEnter
- **Given**: SimpleFsm 已构造，注册了 TestState
- **When**: `fsm.Start<TestState>()`
- **Then**: `IsRunning == true`, `CurrentStateType == typeof(TestState)`, TestState.OnEnter 被调用 1 次

## TC-005: Start — 未注册的状态类型应抛异常
- **Given**: SimpleFsm 已构造，注册了 TestStateA
- **When**: `fsm.Start<TestStateB>()`
- **Then**: 抛出 `ArgumentException`

## TC-006: Start — 重复 Start 应抛异常
- **Given**: SimpleFsm 已 Start<TestState>()
- **When**: 再次调用 `fsm.Start<TestState>()`
- **Then**: 抛出 `InvalidOperationException`

## TC-007: ChangeState — 正确切换并调用 OnExit/OnEnter
- **Given**: SimpleFsm 已 Start<TestStateA>()，注册了 TestStateA 和 TestStateB
- **When**: `fsm.ChangeState<TestStateB>()`
- **Then**: TestStateA.OnExit 被调用 1 次，TestStateB.OnEnter 被调用 1 次，`CurrentStateType == typeof(TestStateB)`

## TC-008: ChangeState — 未启动时调用应抛异常
- **Given**: SimpleFsm 已构造但未 Start
- **When**: `fsm.ChangeState<TestState>()`
- **Then**: 抛出 `InvalidOperationException`

## TC-009: ChangeState — CurrentStateTime 应重置为 0
- **Given**: SimpleFsm 已 Start<TestStateA>()，Update(1.0f) 后 CurrentStateTime == 1.0
- **When**: `fsm.ChangeState<TestStateB>()`
- **Then**: `CurrentStateTime == 0`

## TC-010: Update — 未启动时调用不抛异常
- **Given**: SimpleFsm 已构造但未 Start
- **When**: `fsm.Update(1.0f)`
- **Then**: 不抛异常，`IsRunning == false`

## TC-011: Update — 累加 CurrentStateTime 并调用 OnUpdate
- **Given**: SimpleFsm 已 Start<TestState>()
- **When**: `fsm.Update(0.5f)` 然后 `fsm.Update(0.5f)`
- **Then**: `CurrentStateTime ≈ 1.0f`，TestState.OnUpdate 被调用 2 次，deltaTime 参数分别为 0.5f

## TC-012: Shutdown — 调用 OnExit 并清空状态
- **Given**: SimpleFsm 已 Start<TestState>()
- **When**: `fsm.Shutdown()`
- **Then**: TestState.OnExit 被调用 1 次，`IsRunning == false`

## TC-013: Shutdown — 未启动时调用不抛异常
- **Given**: SimpleFsm 已构造但未 Start
- **When**: `fsm.Shutdown()`
- **Then**: 不抛异常
