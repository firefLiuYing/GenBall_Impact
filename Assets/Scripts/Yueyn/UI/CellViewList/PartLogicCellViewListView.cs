using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Yueyn.UI
{
    public class PartLogicCellViewListView : PartViewBase<PartLogicCellViewListViewData>
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
