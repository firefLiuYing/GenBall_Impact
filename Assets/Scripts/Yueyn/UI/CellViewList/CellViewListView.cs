using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Yueyn.UI
{
    /// <summary>
    /// CellViewListLogic 的 View。
    /// 暴露对 Content 上 CellViewList 组件的引用和 ScrollRect 绑定。
    /// </summary>
    public class CellViewListView : PartViewBase<CellViewListViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public ScrollRect ScrollView { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            ScrollView = _binding.GetBinding<ScrollRect>("ScrollView");
        }
        // ### GENERATED_BINDINGS_END ###

        /// <summary>Content 上的 CellViewList 组件（lazy discovery，也可直接赋值）</summary>
        [SerializeField] private CellViewList _cellViewList;

        public CellViewList CellViewList
        {
            get
            {
                if (_cellViewList == null)
                    _cellViewList = GetComponentInChildren<CellViewList>();
                return _cellViewList;
            }
            set => _cellViewList = value;
        }

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }
    }
}
