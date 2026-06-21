using Yueyn.UI;

namespace GenBall.UI
{
    public class InteractTipSlotLogic : BusinessPartLogic<InteractTipSlotView>
    {
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/InteractTipSlot/InteractTipSlot.prefab";

        private readonly InteractTipSlotViewData _viewData = new();

        public void SetData(int index, string description, bool isSelected)
        {
            _viewData.Index = index;
            _viewData.OperationDescription = description;
            _viewData.IsSelected = isSelected;
            BoundView?.SetViewData(_viewData);
        }
    }
}
