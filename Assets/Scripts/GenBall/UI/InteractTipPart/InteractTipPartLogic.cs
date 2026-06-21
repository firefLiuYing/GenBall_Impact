using System.Collections.Generic;
using GenBall.Interact;
using Yueyn.Main;
using Yueyn.UI;

namespace GenBall.UI
{
    public class InteractTipPartLogic : BusinessPartLogic<InteractTipPartView>
    {
        public override string PrefabPath =>
            "Assets/AssetBundles/UI/InteractTipPart/InteractTipPart.prefab";

        private readonly InteractTipPartViewData _viewData = new();
        private readonly List<InteractTipSlotLogic> _slotLogics = new();
        private IInteractSystem _interactSystem;

        protected override void OnViewBound(PartViewBase view)
        {
            base.OnViewBound(view);
            _interactSystem = SystemRepository.Instance.GetSystem<IInteractSystem>();
            _interactSystem.Interactables.Observe(OnInteractablesChanged);
            _interactSystem.CurrentSelectionIndex.Observe(OnSelectionChanged);
        }

        protected override void OnViewUnbound(PartViewBase view)
        {
            _interactSystem?.Interactables.Unobserve(OnInteractablesChanged);
            _interactSystem?.CurrentSelectionIndex.Unobserve(OnSelectionChanged);
            _interactSystem = null;
            ClearSlots();
            base.OnViewUnbound(view);
        }

        protected override void OnPartCreated()
        {
            base.OnPartCreated();
        }

        private void OnInteractablesChanged(List<IInteractable> interactables)
        {
            ClearSlots();

            if (interactables == null || interactables.Count == 0 || BoundView == null)
            {
                _viewData.Visible = false;
                BoundView?.SetViewData(_viewData);
                return;
            }

            _viewData.Visible = true;
            BoundView.SetViewData(_viewData);

            var selectedIndex = _interactSystem.CurrentSelectionIndex.Value;

            for (int i = 0; i < interactables.Count; i++)
            {
                var slotLogic = BusinessLogicManager.Instance.CreateLogic<InteractTipSlotLogic>(
                    p => p.ParentTransform = BoundView.RectSlotContainer);
                slotLogic.SetData(i, interactables[i].OperationDescription, i == selectedIndex);
                AddPartLogic(slotLogic);
                _slotLogics.Add(slotLogic);
            }
        }

        private void OnSelectionChanged(int newIndex)
        {
            var interactables = _interactSystem.Interactables.Value;
            if (interactables == null) return;

            for (int i = 0; i < _slotLogics.Count && i < interactables.Count; i++)
            {
                _slotLogics[i].SetData(i, interactables[i].OperationDescription, i == newIndex);
            }
        }

        private void ClearSlots()
        {
            ClearPartLogics();
            _slotLogics.Clear();
        }
    }
}
