using GenBall.Utils.CodeGenerator.UI;
using UnityEngine;
using Yueyn.UI;

namespace GenBall.UI
{
    public class InteractTipPartView : PartViewBase<InteractTipPartViewData>
    {
        // ### GENERATED_BINDINGS_START ###
        private UiViewBinding _binding;

        public RectTransform RectSlotContainer { get; private set; }

        private void BindControls()
        {
            _binding = GetComponent<UiViewBinding>();
            RectSlotContainer = _binding.GetBinding<RectTransform>("RectSlotContainer");
        }

        // ### GENERATED_BINDINGS_END ###

        protected override void DoBusinessStart()
        {
            base.DoBusinessStart();
            BindControls();
        }

        protected override void RefreshView()
        {
            if (ViewData == null) return;
            gameObject.SetActive(ViewData.Visible);
        }
    }
}
