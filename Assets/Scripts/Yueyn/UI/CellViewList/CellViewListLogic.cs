using System.Collections.Generic;
using UnityEngine;

namespace Yueyn.UI
{
    /// <summary>
    /// 普通 ICellView Cell 的 Logic 包装。
    /// 组合 CellViewList，提供 SetItems 便捷 API 和生命周期集成。
    /// </summary>
    public class CellViewListLogic : BusinessPartLogic<CellViewListView>
    {
        // ### GENERATED_BINDINGS_START ###
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/CellViewList/CellViewList.prefab";
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

        /// <summary>设置列表数据</summary>
        public void SetItems(IReadOnlyList<object> data, GameObject cellPrefab)
        {
            CellViewList?.SetItems(data, cellPrefab);
        }

        protected override void OnPartCreated()
        {
            // BoundView 已在基类 LoadAndBindPart 中设置
        }
    }
}
