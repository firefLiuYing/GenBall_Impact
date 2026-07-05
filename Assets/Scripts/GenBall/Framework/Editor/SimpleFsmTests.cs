using System;
using NUnit.Framework;
using Yueyn.Fsm;

namespace Yueyn.Fsm.Tests
{
    [TestFixture]
    public class SimpleFsmTests
    {
        private class RecordState : SimpleFsmState<object>
        {
            public int EnterCount, ExitCount, UpdateCount;
            public float LastDeltaTime;
            public override void OnEnter(object ctx) { EnterCount++; }
            public override void OnUpdate(object ctx, float dt) { UpdateCount++; LastDeltaTime = dt; }
            public override void OnExit(object ctx) { ExitCount++; }
        }
        private class RecordStateA : RecordState { }
        private class RecordStateB : RecordState { }

        private object ctx;
        private RecordStateA stateA;
        private RecordStateB stateB;
        private SimpleFsm<object> fsm;

        [SetUp] public void SetUp() { ctx = new object(); stateA = new RecordStateA(); stateB = new RecordStateB(); fsm = null; }
        [TearDown] public void TearDown() { fsm?.Shutdown(); }

        [Test] public void Constructor_NullContext_Throws() { Assert.Throws<ArgumentNullException>(() => new SimpleFsm<object>(null, stateA)); }
        [Test] public void Constructor_EmptyStates_Throws() { Assert.Throws<ArgumentException>(() => new SimpleFsm<object>(ctx)); }
        [Test] public void Constructor_NullElement_Throws() { Assert.Throws<ArgumentException>(() => new SimpleFsm<object>(ctx, null)); }
        [Test] public void Start_CallsOnEnter() { fsm = new SimpleFsm<object>(ctx, stateA); fsm.Start<RecordStateA>(); Assert.IsTrue(fsm.IsRunning); Assert.AreEqual(1, stateA.EnterCount); }
        [Test] public void Start_Unregistered_Throws() { fsm = new SimpleFsm<object>(ctx, stateA); Assert.Throws<ArgumentException>(() => fsm.Start<RecordStateB>()); }
        [Test] public void Start_AlreadyRunning_Throws() { fsm = new SimpleFsm<object>(ctx, stateA); fsm.Start<RecordStateA>(); Assert.Throws<InvalidOperationException>(() => fsm.Start<RecordStateA>()); }
        [Test] public void ChangeState_SwitchesState() { fsm = new SimpleFsm<object>(ctx, stateA, stateB); fsm.Start<RecordStateA>(); fsm.ChangeState<RecordStateB>(); Assert.AreEqual(1, stateA.ExitCount); Assert.AreEqual(1, stateB.EnterCount); }
        [Test] public void ChangeState_NotRunning_Throws() { fsm = new SimpleFsm<object>(ctx, stateA); Assert.Throws<InvalidOperationException>(() => fsm.ChangeState<RecordStateA>()); }
        [Test] public void ChangeState_ResetsTime() { fsm = new SimpleFsm<object>(ctx, stateA, stateB); fsm.Start<RecordStateA>(); fsm.Update(1f); fsm.ChangeState<RecordStateB>(); Assert.AreEqual(0f, fsm.CurrentStateTime); }
        [Test] public void Update_NotRunning_NoThrow() { fsm = new SimpleFsm<object>(ctx, stateA); Assert.DoesNotThrow(() => fsm.Update(1f)); }
        [Test] public void Update_AccumulatesTime() { fsm = new SimpleFsm<object>(ctx, stateA); fsm.Start<RecordStateA>(); fsm.Update(0.5f); fsm.Update(0.5f); Assert.AreEqual(1f, fsm.CurrentStateTime, 0.001f); Assert.AreEqual(2, stateA.UpdateCount); }
        [Test] public void Shutdown_CallsOnExit() { fsm = new SimpleFsm<object>(ctx, stateA); fsm.Start<RecordStateA>(); fsm.Shutdown(); Assert.AreEqual(1, stateA.ExitCount); Assert.IsFalse(fsm.IsRunning); }
        [Test] public void Shutdown_NotRunning_NoThrow() { fsm = new SimpleFsm<object>(ctx, stateA); Assert.DoesNotThrow(() => fsm.Shutdown()); }
    }
}
