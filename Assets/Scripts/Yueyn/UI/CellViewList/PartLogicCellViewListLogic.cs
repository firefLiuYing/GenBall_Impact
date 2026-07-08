using System.Collections.Generic;
using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// PartLogic Cell 的 Logic 包装。
    /// IS-A BusinessPartLogicContainer，管理 Cell PartLogic 的父子关系。
    /// </summary>
    public class PartLogicCellViewListLogic : BusinessPartLogic<PartLogicCellViewListView>
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/PartLogicCellViewList/PartLogicCellViewList.prefab";

        // ### GENERATED_BINDINGS_END ###

        private CellViewList _cellViewList;

        /// <summary>获取 Content 上的 CellViewList 组件（lazy init）</summary>
        public CellViewList CellViewList
        {
            get
            {
                if (_cellViewList == null && BoundView != null)
                    _cellViewList = BoundView.CellViewList;
                return _cellViewList;
            }
        }

        protected override void OnPartCreated()
        {
            // BoundView 已在基类 LoadAndBindPart 中设置
        }

        protected override void OnViewBound(PartViewBase view)
        {
            base.OnViewBound(view);
            if (CellViewList != null)
            {
                CellViewList.OnCellCreated += HandleCellCreated;
                CellViewList.OnCellRemoved += HandleCellRemoved;
            }
        }

        protected override void OnViewUnbound(PartViewBase view)
        {
            if (CellViewList != null)
            {
                CellViewList.OnCellCreated -= HandleCellCreated;
                CellViewList.OnCellRemoved -= HandleCellRemoved;
            }
            base.OnViewUnbound(view);
        }

        /// <summary>设置列表数据</summary>
        public void SetItems(IReadOnlyList<object> data, GameObject partPrefab)
        {
            CellViewList?.SetItems(data, partPrefab);
        }

        /// <summary>Cell 创建后，将其内部的 PartLogic 注册为子容器</summary>
        private void HandleCellCreated(ICellView cell)
        {
            if (cell is PartLogicCellAdapter adapter && adapter.CreatedLogic != null)
                AddPartLogic(adapter.CreatedLogic);
        }

        /// <summary>Cell 移除前，从容器中注销 PartLogic（并销毁）</summary>
        private void HandleCellRemoved(ICellView cell)
        {
            if (cell is PartLogicCellAdapter adapter && adapter.CreatedLogic != null)
                RemovePartLogic(adapter.CreatedLogic);
        }
    }
}
