using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Yueyn.Utils;
using Object = UnityEngine.Object;

namespace Yueyn.UI.Editor.Tests
{
    // ===== Mock types for PartLogicCellViewListLogic tests =====

    /// <summary>Mock PartView for cell prefab. Used with CellPartLogic.</summary>
    public class CellPartView : PartViewBase { }

    /// <summary>Mock PartLogic for cell. Implements ICellViewDataReceiver so adapter can forward data.</summary>
    public class CellPartLogic : BusinessPartLogic<CellPartView>, ICellViewDataReceiver
    {
        public override string PrefabPath => "";
        public bool WasDestroyed { get; private set; }

        protected override void OnDestroyInternal()
        {
            base.OnDestroyInternal();
            WasDestroyed = true;
        }

        void ICellViewDataReceiver.ReceiveData(int index, object data) { }
    }

    /// <summary>Mock plain ICellView (NOT PartLogicCellAdapter). For T15.</summary>
    public class MockPlainCellView : MonoBehaviour, ICellView
    {
        void ICellView.OnCreate() { }
        void ICellView.OnRefresh(int index, object data) { }
        void ICellView.OnRemove() { }
    }

    // ===== Test Fixture =====

    [TestFixture]
    public class PartLogicCellViewListLogicTests
    {
        private GameObject _viewGo;
        private PartLogicCellViewListView _view;
        private CellViewList _cellViewList;
        private PartLogicCellViewListLogic _logic;

        [SetUp]
        public void SetUp()
        {
            // Create the View GameObject with PartLogicCellViewListView + CellViewList
            _viewGo = new GameObject("PartLogicCellViewList");
            _view = _viewGo.AddComponent<PartLogicCellViewListView>();
            _cellViewList = _viewGo.AddComponent<CellViewList>();
            _view.CellViewList = _cellViewList;

            // Create and initialize the PartLogicCellViewListLogic
            _logic = new PartLogicCellViewListLogic();
            _logic.ParentTransform = _viewGo.transform;
            _logic.OnCreate();
        }

        [TearDown]
        public void TearDown()
        {
            if (_viewGo != null)
                Object.DestroyImmediate(_viewGo);
        }

        /// <summary>Helper: get the private _partLogics field via reflection.</summary>
        private SafeIterableList<BusinessPartLogic> GetPartLogics()
        {
            var field = typeof(BusinessPartLogicContainer).GetField("_partLogics",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (SafeIterableList<BusinessPartLogic>)field.GetValue(_logic);
        }

        /// <summary>Helper: create a cell prefab with CellPartView + PartLogicCellAdapter.</summary>
        private static GameObject CreateCellPrefabWithAdapter()
        {
            var prefab = new GameObject("CellWithAdapter");
            prefab.AddComponent<CellPartView>();
            prefab.AddComponent<PartLogicCellAdapter>();
            prefab.SetActive(false);
            return prefab;
        }

        /// <summary>Helper: create a cell prefab with plain ICellView (no adapter).</summary>
        private static GameObject CreatePlainCellPrefab()
        {
            var prefab = new GameObject("PlainCell");
            prefab.AddComponent<MockPlainCellView>();
            prefab.SetActive(false);
            return prefab;
        }

        // ===== T13: Event integration — AddPartLogic on OnCellCreated =====

        [Test]
        public void HandleCellCreated_WithValidAdapter_AddsPartLogicToContainer()
        {
            var cellPrefab = CreateCellPrefabWithAdapter();
            var data = new List<object> { "data0" };

            _cellViewList.SetItems(data, cellPrefab);

            // Get the created PartLogic from the adapter on the instantiated cell
            var adapters = _viewGo.GetComponentsInChildren<PartLogicCellAdapter>(includeInactive: true);
            Assert.AreEqual(1, adapters.Length, "One adapter should be created");
            var createdLogic = adapters[0].CreatedLogic;
            Assert.IsNotNull(createdLogic, "Adapter should have created PartLogic");

            // Verify the PartLogic was registered in the container
            var partLogics = GetPartLogics();
            Assert.IsTrue(partLogics.Contains(createdLogic),
                "PartLogic should be registered via AddPartLogic");

            Object.DestroyImmediate(cellPrefab);
        }

        // ===== T14: Event integration — RemovePartLogic on OnCellRemoved =====

        [Test]
        public void HandleCellRemoved_WithValidAdapter_RemovesAndDestroysPartLogic()
        {
            var cellPrefab = CreateCellPrefabWithAdapter();
            var data = new List<object> { "data0" };
            _cellViewList.SetItems(data, cellPrefab);

            // Get reference to the PartLogic before removal
            var adapters = _viewGo.GetComponentsInChildren<PartLogicCellAdapter>(includeInactive: true);
            Assert.AreEqual(1, adapters.Length);
            var logic = adapters[0].CreatedLogic as CellPartLogic;
            Assert.IsNotNull(logic);

            // Remove by setting empty list
            _cellViewList.SetItems(new List<object>(), cellPrefab);

            // PartLogic.OnDestroy should have been called by RemovePartLogic
            Assert.IsTrue(logic.WasDestroyed,
                "PartLogic.OnDestroy should be called by RemovePartLogic");

            // Container should no longer contain the PartLogic
            var partLogics = GetPartLogics();
            Assert.IsFalse(partLogics.Contains(logic),
                "PartLogic should be removed from container");

            Object.DestroyImmediate(cellPrefab);
        }

        // ===== T15: Non-PartLogicCellAdapter Cell safely skips =====

        [Test]
        public void HandleCellCreated_WithPlainICellView_SkipsSafely()
        {
            var cellPrefab = CreatePlainCellPrefab();
            var data = new List<object> { "data0" };

            // Verify no errors are logged
            Assert.DoesNotThrow(() => _cellViewList.SetItems(data, cellPrefab));

            // Container should still be empty (no PartLogic registered)
            var partLogics = GetPartLogics();
            Assert.AreEqual(0, partLogics.Count,
                "Plain ICellView should not trigger AddPartLogic");

            Object.DestroyImmediate(cellPrefab);
        }
    }
}
