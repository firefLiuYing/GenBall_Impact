using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Yueyn.UI.Editor.Tests
{
    /// <summary>
    /// Mock ICellView that tracks lifecycle calls for verification.
    /// </summary>
    public class MockCellView : MonoBehaviour, ICellView
    {
        public int CreateCount;
        public int RemoveCount;
        public int RefreshCount;
        public int LastRefreshIndex = -1;
        public object LastRefreshData;
        /// <summary>Static counter so tests can verify OnRemove was called even after Destroy.</summary>
        public static int GlobalRemoveCounter;

        void ICellView.OnCreate() { CreateCount++; }

        void ICellView.OnRefresh(int index, object data)
        {
            RefreshCount++;
            LastRefreshIndex = index;
            LastRefreshData = data;
        }

        void ICellView.OnRemove()
        {
            RemoveCount++;
            GlobalRemoveCounter++;
        }
    }

    [TestFixture]
    public class CellViewListTests
    {
        private GameObject _contentGo;
        private CellViewList _cellViewList;

        [SetUp]
        public void SetUp()
        {
            MockCellView.GlobalRemoveCounter = 0;
            _contentGo = new GameObject("Content");
            _cellViewList = _contentGo.AddComponent<CellViewList>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_contentGo != null)
                Object.DestroyImmediate(_contentGo);
        }

        private static GameObject CreateCellPrefab()
        {
            var prefab = new GameObject("CellPrefab");
            prefab.AddComponent<MockCellView>();
            prefab.SetActive(false);
            return prefab;
        }

        // ===== T1: SetItems creates correct number of Cells =====

        [Test]
        public void SetItems_CreatesExactCellCount()
        {
            var prefab = CreateCellPrefab();
            var data = new List<object> { "data0", "data1", "data2" };
            var createdCount = 0;
            _cellViewList.OnCellCreated += _ => createdCount++;

            _cellViewList.SetItems(data, prefab);

            Assert.AreEqual(3, _cellViewList.CellCount);
            Assert.AreEqual(3, createdCount, "OnCellCreated should fire 3 times");

            var cells = _contentGo.GetComponentsInChildren<MockCellView>(includeInactive: true);
            Assert.AreEqual(3, cells.Length);

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.AreEqual(1, cells[i].CreateCount, $"Cell[{i}] OnCreate should be called once");
                Assert.AreEqual(1, cells[i].RefreshCount, $"Cell[{i}] OnRefresh should be called once");
                Assert.AreEqual(i, cells[i].LastRefreshIndex, $"Cell[{i}] index should be {i}");
                Assert.AreEqual(data[i], cells[i].LastRefreshData, $"Cell[{i}] data mismatch");
            }

            Object.DestroyImmediate(prefab);
        }

        // ===== T2: SetItems reduces Cell count =====

        [Test]
        public void SetItems_ReducesCellCount_RemovesExcessAndRefreshesKept()
        {
            var prefab = CreateCellPrefab();
            var data3 = new List<object> { "data0", "data1", "data2" };
            _cellViewList.SetItems(data3, prefab);
            Assert.AreEqual(3, _cellViewList.CellCount);

            var cellsBefore = _contentGo.GetComponentsInChildren<MockCellView>(includeInactive: true);
            Assert.AreEqual(3, cellsBefore.Length);
            var cell0 = cellsBefore[0];
            var cell1 = cellsBefore[1];
            var cell2 = cellsBefore[2];

            // Track OnCellRemoved events and capture RemoveCount at event time (before OnRemove)
            var removeCountsAtEventTime = new List<int>();
            var removedCount = 0;
            _cellViewList.OnCellRemoved += cell =>
            {
                var mv = ((MonoBehaviour)cell).GetComponent<MockCellView>();
                removeCountsAtEventTime.Add(mv.RemoveCount);
                removedCount++;
            };

            MockCellView.GlobalRemoveCounter = 0;

            var data1 = new List<object> { "data0_updated" };
            _cellViewList.SetItems(data1, prefab);

            Assert.AreEqual(1, _cellViewList.CellCount);
            Assert.AreEqual(2, removedCount, "OnCellRemoved should fire twice");

            // OnCellRemoved fires BEFORE OnRemove — RemoveCount should be 0 at event time
            Assert.AreEqual(2, removeCountsAtEventTime.Count);
            Assert.AreEqual(0, removeCountsAtEventTime[0], "OnRemove called AFTER OnCellRemoved (cell2)");
            Assert.AreEqual(0, removeCountsAtEventTime[1], "OnRemove called AFTER OnCellRemoved (cell1)");

            // After the full operation, 2 removed cells should have OnRemove called
            Assert.AreEqual(2, MockCellView.GlobalRemoveCounter,
                "OnRemove called on both removed cells");

            // Kept cell should be refreshed with new index and data (cell0 is still alive)
            Assert.AreEqual(2, cell0.RefreshCount, "Kept cell refreshed twice");
            Assert.AreEqual(0, cell0.LastRefreshIndex);
            Assert.AreEqual("data0_updated", cell0.LastRefreshData);

            Object.DestroyImmediate(prefab);
        }

        // ===== T3: Empty list clears all Cells =====

        [Test]
        public void SetItems_EmptyList_ClearsAllCells()
        {
            var prefab = CreateCellPrefab();
            var data3 = new List<object> { "data0", "data1", "data2" };
            _cellViewList.SetItems(data3, prefab);
            Assert.AreEqual(3, _cellViewList.CellCount);

            Assert.AreEqual(3, _contentGo.GetComponentsInChildren<MockCellView>(includeInactive: true).Length);
            var removedCount = 0;
            _cellViewList.OnCellRemoved += _ => removedCount++;

            MockCellView.GlobalRemoveCounter = 0;

            _cellViewList.SetItems(new List<object>(), prefab);

            Assert.AreEqual(0, _cellViewList.CellCount);
            Assert.AreEqual(3, removedCount, "OnCellRemoved should fire 3 times");
            Assert.AreEqual(3, MockCellView.GlobalRemoveCounter,
                "OnRemove called 3 times (cells destroyed, use static counter)");

            // All child GameObjects should be destroyed
            var remaining = _contentGo.GetComponentsInChildren<MockCellView>(includeInactive: true);
            Assert.AreEqual(0, remaining.Length, "All cell GameObjects destroyed");

            Object.DestroyImmediate(prefab);
        }

        // ===== T4: SetItems updates data of existing Cells (same count, new data) =====

        [Test]
        public void SetItems_SameCountNewData_OnlyRefreshes()
        {
            var prefab = CreateCellPrefab();
            var oldData = new List<object> { "old0", "old1" };
            _cellViewList.SetItems(oldData, prefab);
            Assert.AreEqual(2, _cellViewList.CellCount);

            var cells = _contentGo.GetComponentsInChildren<MockCellView>(includeInactive: true);
            Assert.AreEqual(2, cells.Length);

            var createdCount = 0;
            var removedCount = 0;
            _cellViewList.OnCellCreated += _ => createdCount++;
            _cellViewList.OnCellRemoved += _ => removedCount++;

            var newData = new List<object> { "new0", "new1" };
            _cellViewList.SetItems(newData, prefab);

            Assert.AreEqual(2, _cellViewList.CellCount, "Cell count unchanged");
            Assert.AreEqual(0, createdCount, "No cells created");
            Assert.AreEqual(0, removedCount, "No cells removed");

            for (int i = 0; i < cells.Length; i++)
            {
                Assert.AreEqual(2, cells[i].RefreshCount, $"Cell[{i}] refreshed twice");
                Assert.AreEqual(i, cells[i].LastRefreshIndex);
                Assert.AreEqual(newData[i], cells[i].LastRefreshData);
            }

            Object.DestroyImmediate(prefab);
        }

        // ===== T5: prefab without ICellView component =====

        [Test]
        public void SetItems_PrefabWithoutICellView_LogsWarningAndKeepsZeroCells()
        {
            var badPrefab = new GameObject("BadPrefab");
            badPrefab.SetActive(false);

            var data = new List<object> { "data0" };

            LogAssert.Expect(LogType.Warning, "[CellViewList] Prefab 'BadPrefab' has no ICellView component");
            Assert.DoesNotThrow(() => _cellViewList.SetItems(data, badPrefab));

            Assert.AreEqual(0, _cellViewList.CellCount, "No cells when ICellView missing");

            Object.DestroyImmediate(badPrefab);
        }

        // ===== T6: OnDestroy cleans all Cells =====

        [Test]
        public void OnDestroy_CleansAllCells()
        {
            var prefab = CreateCellPrefab();
            var data3 = new List<object> { "data0", "data1", "data2" };
            _cellViewList.SetItems(data3, prefab);
            Assert.AreEqual(3, _cellViewList.CellCount);

            // OnCellRemoved should NOT fire during OnDestroy
            var removedCount = 0;
            _cellViewList.OnCellRemoved += _ => removedCount++;

            // Reset global counter before triggering OnDestroy
            MockCellView.GlobalRemoveCounter = 0;

            // Invoke OnDestroy via reflection (avoids hierarchy-destruction side effects)
            var onDestroy = typeof(CellViewList).GetMethod("OnDestroy",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onDestroy.Invoke(_cellViewList, null);

            Assert.AreEqual(3, MockCellView.GlobalRemoveCounter,
                "All 3 cells should have OnRemove called during OnDestroy");
            Assert.AreEqual(0, removedCount,
                "OnCellRemoved should NOT fire during OnDestroy (_isDestroying=true)");

            Object.DestroyImmediate(prefab);
        }
    }
}
