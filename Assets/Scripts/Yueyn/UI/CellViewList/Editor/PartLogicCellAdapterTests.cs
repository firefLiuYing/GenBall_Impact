using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Yueyn.UI.Editor.Tests
{
    // ===== Mock PartView types for adapter tests =====

    /// <summary>Mock PartView used with MockCellViewPartLogic (implements ICellViewDataReceiver).</summary>
    public class MockPartView : PartViewBase { }

    /// <summary>Mock PartLogic that implements ICellViewDataReceiver. For T7, T10, T12.</summary>
    public class MockCellViewPartLogic : BusinessPartLogic<MockPartView>, ICellViewDataReceiver
    {
        public override string PrefabPath => "";
        public bool WasCreated { get; private set; }
        public bool WasDestroyed { get; private set; }
        public int LastReceivedIndex { get; private set; } = -1;
        public object LastReceivedData { get; private set; }

        protected override void OnPartCreated()
        {
            base.OnPartCreated();
            WasCreated = true;
        }

        protected override void OnDestroyInternal()
        {
            base.OnDestroyInternal();
            WasDestroyed = true;
        }

        void ICellViewDataReceiver.ReceiveData(int index, object data)
        {
            LastReceivedIndex = index;
            LastReceivedData = data;
        }
    }

    /// <summary>Mock PartView used with MockNonReceiverPartLogic (does NOT implement ICellViewDataReceiver). For T11.</summary>
    public class MockNonReceiverPartView : PartViewBase { }

    /// <summary>Mock PartLogic that does NOT implement ICellViewDataReceiver. For T11.</summary>
    public class MockNonReceiverPartLogic : BusinessPartLogic<MockNonReceiverPartView>
    {
        public override string PrefabPath => "";
        public bool WasCreated { get; private set; }

        protected override void OnPartCreated()
        {
            base.OnPartCreated();
            WasCreated = true;
        }
    }

    /// <summary>Mock PartView with no registered PartLogic. For T9.</summary>
    public class MockUnregisteredPartView : PartViewBase { }

    // ===== Test Fixture =====

    [TestFixture]
    public class PartLogicCellAdapterTests
    {
        private GameObject _adapterGo;

        [SetUp]
        public void SetUp()
        {
            _adapterGo = new GameObject("AdapterGO");
        }

        [TearDown]
        public void TearDown()
        {
            if (_adapterGo != null)
                Object.DestroyImmediate(_adapterGo);
        }

        // ===== T7: OnCreate successfully creates PartLogic =====

        [Test]
        public void OnCreate_WithValidPartView_CreatesPartLogic()
        {
            _adapterGo.AddComponent<MockPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();

            // Call OnCreate through the ICellView interface
            ((ICellView)adapter).OnCreate();

            Assert.IsNotNull(adapter.CreatedLogic, "PartLogic should be created");
            Assert.IsInstanceOf<MockCellViewPartLogic>(adapter.CreatedLogic,
                "Should create the registered PartLogic type");

            var logic = (MockCellViewPartLogic)adapter.CreatedLogic;
            Assert.IsTrue(logic.WasCreated, "PartLogic.OnCreate should be called (via OnPartCreated)");
        }

        // ===== T8: Missing PartViewBase logs warning and returns null =====

        [Test]
        public void OnCreate_WithoutPartViewBase_LogsWarningAndSkips()
        {
            // [RequireComponent(typeof(PartViewBase))] prevents AddComponent without a
            // concrete PartViewBase. Workaround: add a temporary PartViewBase, then
            // destroy it so GetComponent<PartViewBase>() returns null when OnCreate runs.
            _adapterGo.AddComponent<MockPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();
            var tempView = _adapterGo.GetComponent<MockPartView>();
            Object.DestroyImmediate(tempView);

            // Verify PartViewBase is truly gone (Unity recovers from DestroyImmediate)
            var viewAfterDestroy = _adapterGo.GetComponent<PartViewBase>();
            if (viewAfterDestroy != null)
            {
                Assert.Inconclusive(
                    "GetComponent<PartViewBase>() returns destroyed component in this Unity version. " +
                    "Cannot isolate adapter without PartViewBase.");
                return;
            }

            LogAssert.Expect(LogType.Warning,
                $"[PartLogicCellAdapter] No PartViewBase on {_adapterGo.name}");
            Assert.DoesNotThrow(() => ((ICellView)adapter).OnCreate());

            Assert.IsNull(adapter.CreatedLogic, "No PartLogic when PartViewBase is missing");
        }

        // ===== T9: Unregistered PartView type logs warning =====

        [Test]
        public void OnCreate_WithUnregisteredPartViewType_LogsWarningAndSkips()
        {
            _adapterGo.AddComponent<MockUnregisteredPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();

            LogAssert.Expect(LogType.Warning,
                $"[PartLogicCellAdapter] No PartLogic registered for {nameof(MockUnregisteredPartView)}");
            Assert.DoesNotThrow(() => ((ICellView)adapter).OnCreate());

            Assert.IsNull(adapter.CreatedLogic,
                "No PartLogic when PartView type is not registered");
        }

        // ===== T10: OnRefresh passes data to PartLogic (ICellViewDataReceiver) =====

        [Test]
        public void OnRefresh_WithDataReceiver_PassesData()
        {
            _adapterGo.AddComponent<MockPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();
            ((ICellView)adapter).OnCreate();
            Assert.IsNotNull(adapter.CreatedLogic);

            var testData = new { Name = "test", Value = 42 };
            ((ICellView)adapter).OnRefresh(3, testData);

            var logic = (MockCellViewPartLogic)adapter.CreatedLogic;
            Assert.AreEqual(3, logic.LastReceivedIndex, "Index should be passed to ReceiveData");
            Assert.AreSame(testData, logic.LastReceivedData, "Data should be passed to ReceiveData");
        }

        // ===== T11: OnRefresh skips safely when PartLogic does not implement ICellViewDataReceiver =====

        [Test]
        public void OnRefresh_WithoutDataReceiver_NoException()
        {
            _adapterGo.AddComponent<MockNonReceiverPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();
            ((ICellView)adapter).OnCreate();
            Assert.IsNotNull(adapter.CreatedLogic);
            Assert.IsInstanceOf<MockNonReceiverPartLogic>(adapter.CreatedLogic);

            var testData = "some data";
            Assert.DoesNotThrow(() => ((ICellView)adapter).OnRefresh(0, testData));
        }

        // ===== T12: OnRemove does NOT call PartLogic.OnDestroy =====

        [Test]
        public void OnRemove_DoesNotCallPartLogicOnDestroy()
        {
            _adapterGo.AddComponent<MockPartView>();
            var adapter = _adapterGo.AddComponent<PartLogicCellAdapter>();
            ((ICellView)adapter).OnCreate();
            Assert.IsNotNull(adapter.CreatedLogic);

            var logic = (MockCellViewPartLogic)adapter.CreatedLogic;

            ((ICellView)adapter).OnRemove();

            Assert.IsNull(adapter.CreatedLogic, "CreatedLogic should be nulled");
            Assert.IsFalse(logic.WasDestroyed,
                "PartLogic.OnDestroy should NOT be called — handled by PartLogicCellViewListLogic");
        }
    }
}
