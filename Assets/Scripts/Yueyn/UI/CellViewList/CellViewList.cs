using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yueyn.UI
{
    /// <summary>
    /// 通用 Cell 列表管理器。
    /// 管理 ICellView 生命周期 + 布局配置。
    /// 纯工具组件，框架无关。
    /// </summary>
    public class CellViewList : MonoBehaviour
    {
        public enum LayoutMode
        {
            Vertical,
            Horizontal,
            Grid,
        }

        [SerializeField] private LayoutMode _layoutMode = LayoutMode.Vertical;
        [SerializeField] private float _gridCellWidth = 100f;
        [SerializeField] private float _gridCellHeight = 100f;

        /// <summary>Cell 被创建时触发（在 OnCreate 之后）。订阅者可在此时读取 ICellView 并做额外处理</summary>
        public event Action<ICellView> OnCellCreated;

        /// <summary>Cell 即将被移除时触发（在 OnRemove 之前）。订阅者应在此时清理对该 Cell 的引用</summary>
        public event Action<ICellView> OnCellRemoved;

        private readonly List<ICellView> _cells = new List<ICellView>();
        private readonly List<object> _data = new List<object>();
        private LayoutGroup _layoutGroup;
        private bool _isDestroying;

        /// <summary>返回当前 Cell 数量（只读）</summary>
        public int CellCount => _cells.Count;

        // ===== 公共 API =====

        /// <summary>
        /// 设置列表数据并同步 Cell。
        /// cellPrefab 必须包含 ICellView 组件。
        /// </summary>
        public void SetItems(IReadOnlyList<object> data, GameObject cellPrefab)
        {
            if (cellPrefab == null)
            {
                Debug.LogError("[CellViewList] SetItems: cellPrefab is null");
                return;
            }

            if (data == null)
            {
                Debug.LogError("[CellViewList] SetItems: data is null");
                return;
            }

            ConfigureLayout();

            _data.Clear();
            _data.AddRange(data);

            // 数据多 → 创建新 Cell
            while (_cells.Count < _data.Count)
            {
                var cell = CreateCell(cellPrefab);
                if (cell != null)
                {
                    _cells.Add(cell);
                    OnCellCreated?.Invoke(cell);
                }
                else
                {
                    break; // Prefab cannot produce valid ICellView, stop creating
                }
            }

            // 数据少 → 移除多余 Cell
            while (_cells.Count > _data.Count)
            {
                var lastIndex = _cells.Count - 1;
                var cell = _cells[lastIndex];
                RemoveCell(cell);
                _cells.RemoveAt(lastIndex);
            }

            // 刷新所有 Cell
            for (int i = 0; i < _cells.Count; i++)
            {
                _cells[i].OnRefresh(i, _data[i]);
            }
        }

        // ===== 扩展点（子类可重写以实现回收池） =====

        /// <summary>创建一个新 Cell 实例。默认：Instantiate(prefab, transform)</summary>
        protected virtual ICellView CreateCell(GameObject prefab)
        {
            var go = Instantiate(prefab, transform);
            var cell = go.GetComponent<ICellView>();
            if (cell == null)
            {
                Debug.LogWarning($"[CellViewList] Prefab '{prefab.name}' has no ICellView component");
                return null;
            }

            cell.OnCreate();
            return cell;
        }

        /// <summary>移除一个 Cell。默认：OnCellRemoved → OnRemove → Destroy(GameObject)</summary>
        protected virtual void RemoveCell(ICellView cell)
        {
            if (cell == null) return;

            if (!_isDestroying)
                OnCellRemoved?.Invoke(cell);

            cell.OnRemove();

            var go = (cell as MonoBehaviour)?.gameObject;
            if (go != null)
                SafeDestroy(go);
        }

        // ===== 内部 =====

        private void ConfigureLayout()
        {
            GetOrCreateLayoutGroup();

            switch (_layoutMode)
            {
                case LayoutMode.Vertical:
                    if (!(_layoutGroup is VerticalLayoutGroup))
                        ReplaceLayoutGroup<VerticalLayoutGroup>();
                    break;
                case LayoutMode.Horizontal:
                    if (!(_layoutGroup is HorizontalLayoutGroup))
                        ReplaceLayoutGroup<HorizontalLayoutGroup>();
                    break;
                case LayoutMode.Grid:
                    if (!(_layoutGroup is GridLayoutGroup))
                    {
                        var grid = ReplaceLayoutGroup<GridLayoutGroup>();
                        grid.cellSize = new Vector2(_gridCellWidth, _gridCellHeight);
                    }
                    break;
            }
        }

        private LayoutGroup GetOrCreateLayoutGroup()
        {
            if (_layoutGroup == null)
                _layoutGroup = GetComponent<LayoutGroup>();

            if (_layoutGroup == null)
                _layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();

            return _layoutGroup;
        }

        private T ReplaceLayoutGroup<T>() where T : LayoutGroup
        {
            if (_layoutGroup != null)
                SafeDestroy(_layoutGroup);

            _layoutGroup = gameObject.AddComponent<T>();
            return (T)_layoutGroup;
        }

        private void OnDestroy()
        {
            _isDestroying = true;

            // 清理所有剩余 Cell（从后往前移除，不触发 OnCellRemoved 事件）
            while (_cells.Count > 0)
            {
                var lastIndex = _cells.Count - 1;
                var cell = _cells[lastIndex];
                cell.OnRemove();

                var go = (cell as MonoBehaviour)?.gameObject;
                if (go != null)
                    SafeDestroy(go);

                _cells.RemoveAt(lastIndex);
            }
        }

        /// <summary>Destroy wrapper that works in both EditMode and PlayMode.</summary>
        private static void SafeDestroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(obj);
#else
            UnityEngine.Object.Destroy(obj);
#endif
        }
    }
}
